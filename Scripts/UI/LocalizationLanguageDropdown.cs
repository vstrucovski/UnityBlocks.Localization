using System.Globalization;
using System.Linq;
using TMPro;
using UnityBlocks.Localization.Data;
using UnityEngine;

namespace UnityBlocks.Localization.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class LocalizationLanguageDropdown : MonoBehaviour
    {
        [SerializeField] private LocalizationLanguageSettings _settings;

        private TMP_Dropdown _dropdown;

        private void Awake() => _dropdown = GetComponent<TMP_Dropdown>();

        private void OnEnable()
        {
            LocalizationEvents.OnLocalizationLoaded += Populate;
            _dropdown.onValueChanged.AddListener(OnValueChanged);

            if (Loc.IsReady)
                Populate();
        }

        private void OnDisable()
        {
            LocalizationEvents.OnLocalizationLoaded -= Populate;
            _dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void Populate()
        {
            _dropdown.ClearOptions();

            var options = _settings.Aliases
                .Select(a => new TMP_Dropdown.OptionData(DisplayNameFor(a)))
                .ToList();

            _dropdown.AddOptions(options);

            // set current selection without triggering OnValueChanged
            var currentCode = PlayerPrefs.GetString(_settings.PlayerPrefsKey, _settings.DefaultLanguage);
            var index = _settings.Aliases.FindIndex(a => a.code == currentCode);
            _dropdown.SetValueWithoutNotify(Mathf.Max(0, index));
        }

        private void OnValueChanged(int index)
        {
            if (index < 0 || index >= _settings.Aliases.Count) return;
            Loc.SetLanguage(_settings.Aliases[index].code);
        }

        private static string DisplayNameFor(LanguageAlias alias)
        {
            // use native language name from CultureInfo if possible
            try
            {
                var culture = new CultureInfo(alias.code);
                return culture.NativeName;
            }
            catch (CultureNotFoundException)
            {
                return alias.systemLanguage.ToString();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_dropdown == null)
                _dropdown = GetComponent<TMP_Dropdown>();
        }
#endif
    }
}