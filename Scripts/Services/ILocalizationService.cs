using System.Collections.Generic;

namespace UnityBlocks.Localization.Services
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        IReadOnlyList<string> AvailableLanguages { get; }
        bool IsLoaded { get; }
        void Load(string text, LocalizationTableFormat format, bool merge = false);
        void SetLanguage(string lang);
        bool TryGet(string key, out string value);
    }
}
