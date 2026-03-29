using TMPro;
using UnityEngine;

namespace UnityBlocks.Localization.UI
{
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;

        private TMP_Text _text;

        private void Awake() => _text = GetComponent<TMP_Text>();

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

        private void Refresh() => _text.text = Loc.Get(_key);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();

            if (!string.IsNullOrEmpty(_key) && Application.isPlaying && Loc.IsReady)
                Refresh();
        }
#endif
    }
}