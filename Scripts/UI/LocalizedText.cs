using TMPro;
using UnityEngine;

namespace UnityBlocks.Localization.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;
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

        private void Refresh() => textView.text = Loc.Get(_key);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (textView == null)
                textView = GetComponent<TMP_Text>();

            if (!string.IsNullOrEmpty(_key) && Application.isPlaying && Loc.IsReady)
                Refresh();
        }
#endif
    }
}