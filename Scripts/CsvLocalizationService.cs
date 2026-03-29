using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityBlocks.Localization
{
    public class CsvLocalizationService : ILocalizationService
    {
        private readonly List<string> _availableLanguages = new();
        private readonly Dictionary<string, string> _active = new();
        private string[] _lines;
        private string _currentLang;

        public string CurrentLanguage => _currentLang;
        public IReadOnlyList<string> AvailableLanguages => _availableLanguages;
        public bool IsLoaded { get; private set; }

        public void LoadFromCsv(string csvText)
        {
            _availableLanguages.Clear();
            _active.Clear();
            IsLoaded = false;

            if (string.IsNullOrWhiteSpace(csvText))
            {
                Debug.LogError("[Localization] CSV is empty.");
                return;
            }

            // normalize line endings once so \r never leaks into parsed values
            _lines = csvText.Replace("\r\n", "\n").Replace('\r', '\n')
                            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (_lines.Length < 2)
            {
                Debug.LogError("[Localization] CSV has no data rows.");
                return;
            }

            var headers = SplitCsvLine(_lines[0]);
            for (var i = 1; i < headers.Length; i++)
                _availableLanguages.Add(headers[i].Trim());

            IsLoaded = true;
            Debug.Log($"[Localization] Loaded. Languages: {string.Join(", ", _availableLanguages)}");
            LocalizationEvents.RaiseLoaded();
        }

        public void SetLanguage(string lang)
        {
            if (!_availableLanguages.Contains(lang))
            {
                Debug.LogWarning($"[Localization] Language '{lang}' not available.");
                return;
            }

            _currentLang = lang;
            ParseLanguage(lang);
            LocalizationEvents.RaiseLanguageChanged();
        }

        public string Get(string key) =>
            _active.TryGetValue(key, out var val) ? val : $"[{key}]";

        public bool TryGet(string key, out string value) =>
            _active.TryGetValue(key, out value);

        private void ParseLanguage(string lang)
        {
            _active.Clear();

            var headers = SplitCsvLine(_lines[0]);

            var langIndex = -1;
            for (var i = 1; i < headers.Length; i++)
                if (headers[i].Trim() == lang)
                {
                    langIndex = i;
                    break;
                }

            if (langIndex == -1)
            {
                Debug.LogError($"[Localization] Column for '{lang}' not found.");
                return;
            }

            for (var i = 1; i < _lines.Length; i++)
            {
                var cols = SplitCsvLine(_lines[i]);
                if (cols.Length <= langIndex) continue;

                var key = cols[0].Trim();
                if (!string.IsNullOrEmpty(key))
                    _active[key] = cols[langIndex].Trim();
            }

            Debug.Log($"[Localization] Parsed {_active.Count} keys for '{lang}'.");
        }

        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;
            var i = 0;

            while (i < line.Length)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // escaped quote "" → literal "
                        current.Append('"');
                        i += 2;
                        continue;
                    }
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }

                i++;
            }

            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}
