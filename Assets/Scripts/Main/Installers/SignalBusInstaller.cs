using Zenject;

namespace SteelSurge.Main.Installers
{
    public class SignalBusInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBus signalBus = new();
            Container.Bind<SignalBus>().FromInstance(signalBus).AsSingle().NonLazy();
        }
    }
}