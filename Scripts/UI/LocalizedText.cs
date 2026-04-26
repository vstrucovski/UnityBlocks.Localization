using TMPro;
using UnityEngine;

namespace UnityBlocks.Localization.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private string[] _params;
        [SerializeField] private TMP_Text textView;

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
            var text = Loc.Get(_key);
            if (_params != null && _params.Length > 0)
                // ReSharper disable once CoVariantArrayConversion
                text = string.Format(text, _params);
            textView.text = text;
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
                textView.text = _params is {Length: > 0} ? $"${_key} [{string.Join(", ", _params)}]" : $"${_key}";
        }
#endif
    }
}