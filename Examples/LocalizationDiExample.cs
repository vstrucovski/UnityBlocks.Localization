// This file shows how to wire LocalizationLoader with a DI container.
//
// The compilable class below is framework-agnostic.
// Scroll to the bottom for copy-paste Zenject and Reflex installer code
// (kept in comments so this file compiles without those packages).

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnityBlocks.Localization.Examples
{
    /// <summary>
    /// Drop this on a GameObject in your boot scene.
    /// Your DI framework calls Construct() via [Inject] before Start() runs.
    /// </summary>
    public class LocalizationDiBootstrap : MonoBehaviour
    {
        [SerializeField] private LocalizationLoader _loader;

        // Add [Inject] from your framework's namespace (see examples below).
        public void Construct(ILocalizationService service)
        {
            _loader.Inject(service);
        }

        private async void Start()
        {
            await _loader.LoadAsync(destroyCancellationToken);
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// ZENJECT
// ═══════════════════════════════════════════════════════════════════════════
//
// 1. Create a MonoInstaller and add it to your SceneContext or ProjectContext:
//
//   using UnityBlocks.Localization;
//   using Zenject;
//
//   public class LocalizationInstaller : MonoInstaller
//   {
//       public override void InstallBindings()
//       {
//           Container
//               .Bind<ILocalizationService>()
//               .To<CsvLocalizationService>()
//               .AsSingle();
//       }
//   }
//
// 2. Mark Construct() with [Inject] on your bootstrap MonoBehaviour:
//
//   using UnityBlocks.Localization;
//   using Zenject;
//
//   public class LocalizationDiBootstrap : MonoBehaviour
//   {
//       [SerializeField] private LocalizationLoader _loader;
//
//       [Inject]
//       public void Construct(ILocalizationService service) => _loader.Inject(service);
//
//       private async void Start() => await _loader.LoadAsync(destroyCancellationToken);
//   }
