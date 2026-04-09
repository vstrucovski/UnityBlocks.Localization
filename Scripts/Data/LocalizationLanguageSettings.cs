using System.Collections.Generic;
using UnityEngine;

namespace UnityBlocks.Localization.Data
{
    [CreateAssetMenu(fileName = "LocalizationLanguageSettings",
        menuName = "Unity Blocks/Localization/Language Settings")]
    public class LocalizationLanguageSettings : ScriptableObject
    {
        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private string _playerPrefsKey = "app_language";
        [SerializeField] private List<LanguageAlias> _aliases = new();

        public string DefaultLanguage => _defaultLanguage;
        public string PlayerPrefsKey => _playerPrefsKey;
        public List<LanguageAlias> Aliases => _aliases;

        public string Resolve(SystemLanguage systemLanguage)
        {
            foreach (var alias in _aliases)
                if (alias.systemLanguage == systemLanguage)
                    return alias.code;

            return _defaultLanguage;
        }

        public SystemLanguage? Resolve(string code)
        {
            foreach (var alias in _aliases)
                if (alias.code == code)
                    return alias.systemLanguage;

            return null;
        }
    }
}