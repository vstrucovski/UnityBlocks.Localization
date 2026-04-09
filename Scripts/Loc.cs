using System.Linq;
using UnityBlocks.Localization.Data;
using UnityBlocks.Localization.Services;
using UnityEngine;

namespace UnityBlocks.Localization
{
    public static class Loc
    {
        private static ILocalizationService _service;
        private static LocalizationLanguageSettings _settings;
        private static string _playerPrefsKey = "app_language";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain()
        {
            _service = null;
            _settings = null;
            _playerPrefsKey = "app_language";
        }

        public static bool IsReady => _service?.IsLoaded ?? false;

        public static SystemLanguage? CurrentSystemLanguage =>
            _service != null ? _settings?.Resolve(_service.CurrentLanguage) : null;

        internal static void Init(ILocalizationService service, LocalizationLanguageSettings settings = null, string playerPrefsKey = null)
        {
            _service = service;
            _settings = settings;
            _playerPrefsKey = playerPrefsKey ?? "app_language";
        }

        public static string Get(string key)
        {
            if (_service == null || !_service.IsLoaded) return $"[{key}]";
            return _service.TryGet(key, out var value) ? value : $"[{key}]";
        }

        public static void SetLanguage(string lang)
        {
            if (_service == null || !_service.AvailableLanguages.Contains(lang))
            {
                Debug.LogWarning($"[Localization] Cannot set language '{lang}': service not ready or language unavailable.");
                return;
            }

            PlayerPrefs.SetString(_playerPrefsKey, lang);
            _service.SetLanguage(lang);
        }
    }
}
