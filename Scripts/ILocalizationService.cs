using System.Collections.Generic;

namespace UnityBlocks.Localization
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        IReadOnlyList<string> AvailableLanguages { get; }
        bool IsLoaded { get; }
        void SetLanguage(string lang);
        string Get(string key);
        bool TryGet(string key, out string value);
    }
}