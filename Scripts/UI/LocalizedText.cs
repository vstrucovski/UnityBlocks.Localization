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
        public string Key => _key;

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
            if (textView == null) return;

            var localized = Loc.Get(_key);
            var hasParams = _params != null && _params.Length > 0;
            var hasTemplate = !string.IsNullOrEmpty(_formatTemplate);

            if (hasTemplate && hasParams)
            {
                var args = new object[1 + _params.Length];
                args[0] = localized;
                _params.CopyTo(args, 1);
                textView.text = string.Format(_formatTemplate, args);
            }
            else if (hasParams)
            {
                textView.text = string.Format(localized, (object[]) _params);
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
                if (_params is {Length: > 0} && !string.IsNullOrEmpty(_formatTemplate))
                {
                    var preview = new object[1 + _params.Length];
                    preview[0] = $"${_key}";
                    _params.CopyTo(preview, 1);
                    textView.text = string.Format(_formatTemplate, preview);
                }
                else
                {
                    textView.text = _params is {Length: > 0}
                        ? $"${_key} [{string.Join(", ", _params)}]"
                        : $"${_key}";
                }
            }
        }
#endif
    }
}