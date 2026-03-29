using System;
using UnityEngine;

namespace UnityBlocks.Localization.Data
{
    [Serializable]
    public struct LanguageAlias
    {
        public SystemLanguage systemLanguage;

        [Tooltip("Must match column header in CSV, e.g. 'en', 'uk', 'de'")]
        public string code;
    }
}