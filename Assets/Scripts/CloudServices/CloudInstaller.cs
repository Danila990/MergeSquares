using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Zenject;

namespace CloudServices
{
    public enum ECloudType
    {
        Yandex = 0,
        GamePush = 1,
        CrazyGames = 2
    }

    [Serializable]
    public class CloudServiceData
    {
        public ECloudType type;
        public CloudService prefab;
    }
    
    public class CloudInstaller : MonoInstaller
    {
        [SerializeField] private ECloudType type;
        [SerializeField] private List<CloudServiceData> services = new();

        public ECloudType Type => type;
        
        public override void InstallBindings()
        {
            Container.Bind<CloudService>().FromComponentOn(Instantiate(Current()).gameObject).AsSingle().NonLazy();
        }

        private CloudService Current() => services.GetBy(s => s.type == type).prefab;
    }
}