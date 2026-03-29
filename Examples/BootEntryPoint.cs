using UnityEngine;

namespace UnityBlocks.Localization
{
    public class BootEntryPoint : MonoBehaviour
    {
        [SerializeField] private LocalizationLoader _localizationLoader;

        private async void Start()
        {
            await _localizationLoader.LoadAsync(destroyCancellationToken);
        }
    }
}