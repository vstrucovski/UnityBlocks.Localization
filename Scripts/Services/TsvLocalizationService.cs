using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityBlocks.Localization.Services
{
    public class TsvLocalizationService : Localization.ILocalizationService
    {
        // lang → (key → value); all languages parsed upfront so merging is cheap
        private readonly Dictionary<string, Dictionary<string, string>> _data = new();
        private readonly List<string> _availableLanguages = new();
        private Dictionary<string, string> _active = new();
        private string _currentLang;

        public string CurrentLanguage => _currentLang;
        public IReadOnlyList<string> AvailableLanguages => _availableLanguages;
        public bool IsLoaded { get; private set; }

        public void Load(string text, LocalizationTableFormat format, bool merge = false)
        {
            if (!merge)
            {
                _data.Clear();
                _availableLanguages.Clear();
                _active = new Dictionary<string, string>();
                IsLoaded = false;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.LogError("[Localization] TSV is empty.");
                return;
            }

            ParseInto(text, _data);

            if (_data.Count == 0)
            {
                Debug.LogError("[Localization] TSV has no data rows.");
                return;
            }

            foreach (var lang in _data.Keys)
                if (!_availableLanguages.Contains(lang))
                    _availableLanguages.Add(lang);

            if (_currentLang != null && _data.TryGetValue(_currentLang, out var langData))
                _active = langData;

            IsLoaded = true;
            Debug.Log($"[Localization] {(merge ? "Merged" : "Loaded")}. Languages: {string.Join(", ", _availableLanguages)}");
            LocalizationEvents.RaiseLoaded();
        }

        public void SetLanguage(string lang)
        {
            if (!_data.TryGetValue(lang, out var langData))
            {
                Debug.LogWarning($"[Localization] Language '{lang}' not available.");
                return;
            }

            _currentLang = lang;
            _active = langData;
            Debug.Log($"[Localization] Language set to '{lang}'. Keys: {_active.Count}");
            LocalizationEvents.RaiseLanguageChanged();
        }

        public string Get(string key) =>
            _active.TryGetValue(key, out var val) ? val : $"[{key}]";

        public bool TryGet(string key, out string value) =>
            _active.TryGetValue(key, out value);

        private static void ParseInto(string text, Dictionary<string, Dictionary<string, string>> target)
        {
            var rows = ParseRows(text);
            if (rows.Count < 2) return;

            var headers = rows[0];
            var langCount = headers.Count - 1;

            var langDicts = new Dictionary<string, string>[langCount];
            for (var i = 0; i < langCount; i++)
            {
                var lang = headers[i + 1].Trim();
                if (!target.ContainsKey(lang))
                    target[lang] = new Dictionary<string, string>();
                langDicts[i] = target[lang];
            }

            for (var i = 1; i < rows.Count; i++)
            {
                var cols = rows[i];
                var key = cols[0].Trim();
                if (string.IsNullOrEmpty(key)) continue;

                for (var j = 0; j < langCount; j++)
                {
                    if (j + 1 >= cols.Count) break;
                    langDicts[j][key] = Unescape(cols[j + 1]).Trim();
                }
            }
        }

        // Quote-aware TSV row splitter: a cell wrapped in double quotes may itself contain
        // tabs/newlines (Google Sheets exports a cell that way when it has a real line break
        // typed into it), so rows can't be found with a blind Split('\n') first.
        private static List<List<string>> ParseRows(string text)
        {
            var rows = new List<List<string>>();
            var cols = new List<string>();
            var cell = new StringBuilder();
            var inQuotes = false;
            var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
            var i = 0;

            while (i < normalized.Length)
            {
                var c = normalized[i];

                if (inQuotes)
                {
                    if (c == '"' && i + 1 < normalized.Length && normalized[i + 1] == '"')
                    {
                        cell.Append('"');
                        i += 2;
                        continue;
                    }

                    if (c == '"')
                    {
                        inQuotes = false;
                        i++;
                        continue;
                    }

                    cell.Append(c);
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                        i++;
                        continue;
                    }

                    if (c == '\t')
                    {
                        cols.Add(cell.ToString());
                        cell.Clear();
                        i++;
                        continue;
                    }

                    if (c == '\n')
                    {
                        cols.Add(cell.ToString());
                        rows.Add(cols);
                        cols = new List<string>();
                        cell.Clear();
                        i++;
                        continue;
                    }

                    cell.Append(c);
                }

                i++;
            }

            if (cell.Length > 0 || cols.Count > 0)
            {
                cols.Add(cell.ToString());
                rows.Add(cols);
            }

            // mirrors the old Split(StringSplitOptions.RemoveEmptyEntries) behavior: drop blank lines
            rows.RemoveAll(r => r.Count == 1 && r[0].Length == 0);

            return rows;
        }

        private static string Unescape(string value) =>
            value.Replace("\\n", "\n").Replace("\\t", "\t");
    }
}
