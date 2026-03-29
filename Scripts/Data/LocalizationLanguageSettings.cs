using System.Collections.Generic;
using UnityEngine;

namespace UnityBlocks.Localization.Data
{
    [CreateAssetMenu(fileName = "LocalizationLanguageSettings",
        menuName = "Unity.Blocks/Localization/Language Settings")]
    public class LocalizationLanguageSettings : ScriptableObject
    {
      

        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private List<LanguageAlias> _aliases = new();

        public string DefaultLanguage => _defaultLanguage;
        public List<LanguageAlias> Aliases => _aliases;

        public string Resolve(SystemLanguage systemLanguage)
        {
            foreach (var alias in _aliases)
                if (alias.systemLanguage == systemLanguage)
                    return alias.code;

            return _defaultLanguage;
        }
    }
}