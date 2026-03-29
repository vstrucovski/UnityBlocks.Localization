using UnityEngine;

namespace UnityBlocks.Localization
{
    public class Loc
    {
        private static ILocalizationService _service;

        public static bool IsReady => _service?.IsLoaded ?? false;

        internal static void Init(ILocalizationService service) => _service = service;

        public static string Get(string key)
        {
            if (_service == null || !_service.IsLoaded) return $"[{key}]";
            return _service.TryGet(key, out var value) ? value : $"[{key}]";
        }

        public static void SetLanguage(string lang)
        {
            PlayerPrefs.SetString("app_language", lang);
            _service?.SetLanguage(lang);
        }
    }
}