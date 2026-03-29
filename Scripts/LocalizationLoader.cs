using System;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityBlocks.Localization.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityBlocks.Localization
{
    public class LocalizationLoader : MonoBehaviour
    {
        [SerializeField] private LocalizationLanguageSettings _settings;
        [SerializeField] private string _remoteUrl;
        [SerializeField] private TextAsset _fallbackCsv;
        private const string CacheFileName = "localization.csv";
        private readonly CsvLocalizationService _localization = new();

        public async UniTask LoadAsync(CancellationToken ct = default)
        {
            Loc.Init(_localization);

            var csvText = await TryDownloadAsync(ct)
                          ?? TryReadFromPersistentPath()
                          ?? FallbackCsv();

            if (csvText == null)
            {
                Debug.LogError("[Localization] All sources failed.");
                return;
            }

            Apply(csvText);
        }

        private string FallbackCsv()
        {
            if (_fallbackCsv == null) return null;
            Debug.LogWarning("[Localization] Using fallback embedded CSV.");
            return _fallbackCsv.text;
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

        private void Apply(string csvText)
        {
            _localization.LoadFromCsv(csvText);
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
            var saved = PlayerPrefs.GetString("app_language", null);
            if (!string.IsNullOrEmpty(saved) && _localization.AvailableLanguages.Contains(saved))
                return saved;

            var code = _settings != null
                ? _settings.Resolve(Application.systemLanguage)
                : "en";

            return _localization.AvailableLanguages.Contains(code)
                ? code
                : _localization.AvailableLanguages.Count > 0
                    ? _localization.AvailableLanguages[0]
                    : "en";
        }
    }
}