using UnityEngine;

namespace UnityBlocks.Localization.Examples
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