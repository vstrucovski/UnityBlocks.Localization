using System;
using UnityEngine;

namespace UnityBlocks.Localization
{
    public static class LocalizationEvents
    {
        public static event Action OnLanguageChanged;
        public static event Action OnLocalizationLoaded;
        public static event Action<string> OnReloadRequested;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain()
        {
            OnLanguageChanged = null;
            OnLocalizationLoaded = null;
            OnReloadRequested = null;
        }

        internal static void RaiseLanguageChanged()  => OnLanguageChanged?.Invoke();
        internal static void RaiseLoaded()           => OnLocalizationLoaded?.Invoke();

        public static void RaiseReloadRequested(string fileName = null)
            => OnReloadRequested?.Invoke(fileName);
    }
}