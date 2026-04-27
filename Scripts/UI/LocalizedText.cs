using TMPro;
using UnityEngine;

namespace UnityBlocks.Localization.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private string[] _params;
        [SerializeField] private string _formatTemplate;
        [SerializeField] private TMP_Text textView;

        public TMP_Text TextView => textView;

        private void Awake() => textView ??= GetComponent<TMP_Text>();

        private void OnEnable()
        {
            LocalizationEvents.OnLanguageChanged += Refresh;
            LocalizationEvents.OnLocalizationLoaded += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            LocalizationEvents.OnLanguageChanged -= Refresh;
            LocalizationEvents.OnLocalizationLoaded -= Refresh;
        }

        public void SetKey(string value)
        {
            _key = value;
            Refresh();
        }

        public void SetParams(params string[] args)
        {
            _params = args;
            Refresh();
        }

        private void Refresh()
        {
            var localized = Loc.Get(_key);

            if (!string.IsNullOrEmpty(_formatTemplate))
            {
                var args = new string[1 + (_params?.Length ?? 0)];
                args[0] = localized;
                _params?.CopyTo(args, 1);
                // ReSharper disable once CoVariantArrayConversion
                textView.text = string.Format(_formatTemplate, args);
            }
            else if (_params != null && _params.Length > 0)
            {
                // ReSharper disable once CoVariantArrayConversion
                textView.text = string.Format(localized, _params);
            }
            else
            {
                textView.text = localized;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (textView == null)
                textView = GetComponent<TMP_Text>();

            if (string.IsNullOrEmpty(_key)) return;

            if (Application.isPlaying && Loc.IsReady)
                Refresh();
            else if (!Application.isPlaying)
            {
                var preview = _params is {Length: > 0} ? $"${_key} [{string.Join(", ", _params)}]" : $"${_key}";
                textView.text = !string.IsNullOrEmpty(_formatTemplate)
                    ? string.Format(_formatTemplate, preview)
                    : preview;
            }
        }
#endif
    }
}