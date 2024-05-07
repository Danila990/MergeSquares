using GamePush;
using UnityEngine;
using Zenject;

namespace CloudServices.GamePush
{
    public class GamePushTester : MonoBehaviour
    {
        private CloudService _cloudService;
        private GamePushCloudProvider _gamePush;
        
        [Inject]
        public void Construct(CloudService cloudService)
        {
            _cloudService = cloudService;
            _gamePush = _cloudService.GetComponent<GamePushCloudProvider>();
        }

        public void PlayerFetchFields()
        {
            if (_gamePush != null)
            {
                Debug.Log($"[GamePushTester][PlayerFetchFields]");
                GP_Player.FetchFields();
            }
        }
        
        public void PlayerLoad()
        {
            if (_gamePush != null)
            {
                Debug.Log($"[GamePushTester][PlayerLoad]");
                GP_Player.Load();
            }
        }
        
        public void PaymentsFetch()
        {
            if (_gamePush != null)
            {
                Debug.Log($"[GamePushTester][PaymentsFetch]");
                GP_Payments.Fetch();
            }
        }
        public void FetchProductsExtern()
        {
            if (_gamePush != null)
            {
                Debug.Log($"[GamePushTester][FetchProductsExtern]");
                GamePushCloudProvider.GPFetchProductsExtern();
            }
        }
    }
}