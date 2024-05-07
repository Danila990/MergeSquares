using UnityEngine;
using Zenject;

namespace Hellmade
{
    public class SoundManagerInstaller: MonoInstaller
    {
        [SerializeField] private EazySoundManager prefab;
        
        public override void InstallBindings()
        {
            Container.Bind<EazySoundManager>().FromComponentOn(CreateSoundManager()).AsSingle().NonLazy(); 
        }

        private GameObject CreateSoundManager()
        {
            var soundManager = Instantiate(prefab);
            soundManager.Init();
            return soundManager.gameObject;
        }
    }
}