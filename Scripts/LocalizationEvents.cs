using System;

namespace UnityBlocks.Localization
{
    public class LocalizationEvents
    {
        public static event Action OnLanguageChanged;
        public static event Action OnLocalizationLoaded;
        public static event Action<string> OnReloadRequested;

        internal static void RaiseLanguageChanged()  => OnLanguageChanged?.Invoke();
        internal static void RaiseLoaded()           => OnLocalizationLoaded?.Invoke();

        public static void RaiseReloadRequested(string fileName = null)
            => OnReloadRequested?.Invoke(fileName);
    }
}