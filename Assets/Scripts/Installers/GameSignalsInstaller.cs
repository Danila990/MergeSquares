using Core.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Installers
{
    public class DeclaredSignalsContainer
    {
        public IReadOnlyList<Type> Types => _declaredSignalTypes;
        private readonly List<Type> _declaredSignalTypes;

        public DeclaredSignalsContainer(List<Type> declaredSignalTypes)
        {
            _declaredSignalTypes = declaredSignalTypes;
        }
    }

    public class GameSignalsInstaller : Installer<GameSignalsInstaller>
    {
        private DeclaredSignalsContainer _declaredSignalsContainer;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            InstallSignals();
        }

        private void InstallSignals()
        {
            var signalInterfaceType = typeof(ISignal);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(type => signalInterfaceType.IsAssignableFrom(type) && !type.IsAbstract);

            var declaredSignalTypes = new List<Type>();
            foreach (var signalType in types)
            {
                Container.DeclareSignal(signalType).OptionalSubscriber();
                declaredSignalTypes.Add(signalType);
            }

            _declaredSignalsContainer = new DeclaredSignalsContainer(declaredSignalTypes);
            Container.Bind<DeclaredSignalsContainer>().FromInstance(_declaredSignalsContainer).AsSingle();
        }
    }
}