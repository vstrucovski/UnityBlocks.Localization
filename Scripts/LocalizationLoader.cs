using System;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityBlocks.Localization.Data;
using UnityBlocks.Localization.Services;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityBlocks.Localization
{
    public class LocalizationLoader : MonoBehaviour
    {
        [SerializeField] private LocalizationLanguageSettings _settings;
        [SerializeField] private LocalizationTableFormat _format = LocalizationTableFormat.Csv;
        [SerializeField] private string _remoteUrl;
        [SerializeField] private TextAsset _fallbackCsv;
        [SerializeField] private TextAsset _fallbackTsv;

        private const string CacheFileName = "localization.csv";
        private ILocalizationService _localization;
        private bool _isLoading;

        /// <summary>
        /// Call before LoadAsync to supply a custom service (e.g. from a DI container).
        /// Falls back to CsvLocalizationService if not called.
        /// </summary>
        public void Inject(ILocalizationService service) => _localization = service;

        public async UniTask LoadAsync(CancellationToken ct = default)
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                _localization ??= new CsvLocalizationService();
                Loc.Init(_localization, _settings, _settings?.PlayerPrefsKey);

                var csvText = await TryDownloadAsync(ct)
                              ?? TryReadFromPersistentPath()
                              ?? FallbackText();

                if (csvText == null)
                {
                    Debug.LogError("[Localization] All sources failed.");
                    return;
                }

                Apply(csvText);
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void LoadFromTextAsset(TextAsset textAsset)
        {
            if (textAsset == null)
            {
                Debug.LogWarning("[Localization] TextAsset is null.");
                return;
            }

            Apply(textAsset.text);
        }

        public void MergeFromTextAsset(TextAsset textAsset)
        {
            if (textAsset == null)
            {
                Debug.LogWarning("[Localization] TextAsset is null.");
                return;
            }

            Apply(textAsset.text, merge: true);
        }

        public async UniTask MergeFromUrlAsync(string url, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("[Localization] MergeFromUrlAsync: URL is empty.");
                return;
            }

            try
            {
                using var req = UnityWebRequest.Get(url);
                await req.SendWebRequest().WithCancellation(ct);

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[Localization] Merge download failed: {req.error}");
                    return;
                }

                Apply(req.downloadHandler.text, merge: true);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                Debug.LogWarning($"[Localization] Merge download exception: {e.Message}");
            }
        }

        public bool LoadFromPersistentPath(string fileName = null)
        {
            var text = TryReadFromPersistentPath(fileName ?? CacheFileName);
            if (text == null)
            {
                Debug.LogWarning($"[Localization] File not found: {fileName ?? CacheFileName}");
                return false;
            }

            Apply(text);
            return true;
        }

        private void OnEnable() => LocalizationEvents.OnReloadRequested += OnReloadRequested;
        private void OnDisable() => LocalizationEvents.OnReloadRequested -= OnReloadRequested;

        private void OnReloadRequested(string fileName) => LoadFromPersistentPath(fileName);

        private void Apply(string text, bool merge = false)
        {
            if (merge && _localization == null)
            {
                Debug.LogWarning("[Localization] Call LoadAsync before merging additional tables.");
                return;
            }

            _localization ??= new CsvLocalizationService();
            _localization.Load(text, _format, merge);

            if (!merge)
                _localization.SetLanguage(ResolveInitialLanguage());
        }

        private async UniTask<string> TryDownloadAsync(CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_remoteUrl)) return null;

            try
            {
                using var req = UnityWebRequest.Get(_remoteUrl);
                await req.SendWebRequest().WithCancellation(ct);

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[Localization] Download failed: {req.error}");
                    return null;
                }

                var text = req.downloadHandler.text;
                SaveToCache(text);
                Debug.Log("[Localization] Downloaded fresh CSV.");
                return text;
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                Debug.LogWarning($"[Localization] Download exception: {e.Message}");
                return null;
            }
        }

        private string FallbackText()
        {
            if (_format == LocalizationTableFormat.Tsv)
            {
                if (_fallbackTsv == null) return null;
                Debug.LogWarning("[Localization] Using fallback embedded TSV.");
                return _fallbackTsv.text;
            }

            if (_fallbackCsv == null) return null;
            Debug.LogWarning("[Localization] Using fallback embedded CSV.");
            return _fallbackCsv.text;
        }

        private static string TryReadFromPersistentPath(string fileName = CacheFileName)
        {
            var path = GetCachePath(fileName);
            if (!File.Exists(path)) return null;
            Debug.Log($"[Localization] Read from: {path}");
            return File.ReadAllText(path);
        }

        private static void SaveToCache(string text, string fileName = CacheFileName)
        {
            try
            {
                File.WriteAllText(GetCachePath(fileName), text);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Localization] Cache write failed: {e.Message}");
            }
        }

        private static string GetCachePath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName);

        private string ResolveInitialLanguage()
        {
            var key = _settings?.PlayerPrefsKey ?? "app_language";
            var saved = PlayerPrefs.GetString(key, null);
            if (!string.IsNullOrEmpty(saved) && _localization.AvailableLanguages.Contains(saved))
                return saved;

            var code = _settings != null
                ? _settings.Resolve(Application.systemLanguage)
                : null;

            if (code != null && _localization.AvailableLanguages.Contains(code))
                return code;

            return _localization.AvailableLanguages.Count > 0
                ? _localization.AvailableLanguages[0]
                : _settings?.DefaultLanguage ?? string.Empty;
        }
    }
}
