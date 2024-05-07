using CloudServices;
using Purchases;
using Zenject;

namespace GameScripts.MergeSquares.Shop
{
    public class SquaresPurchaseProvider : PurchaseProviderBase
    {
        private GridManager _gridManager;
        
        [Inject]
        public void Construct(GridManager gridManager)
        {
            _gridManager = gridManager;
        }
        
        public override bool TryCheckAlreadyBought(PurchaseSo so)
        {
            if (so.type == Purchases.EPurchaseType.GameType && so.purchaseExt is PurchaseExtSo purchaseExt)
            {
                switch (purchaseExt.Type)
                {
                    case EPurchaseType.Skin:
                        return (_gridManager.OpenedSkins.Find(s => s.skinType == purchaseExt.SkinType) != null);
                }
            }

            return false;
        }

        public override void Purchase(PurchaseSo so, CloudPurchase purchase)
        {
            if (so.type == Purchases.EPurchaseType.GameType && so.purchaseExt is PurchaseExtSo purchaseExt)
            {
                switch (purchaseExt.Type)
                {
                    case EPurchaseType.Skin:
                        _gridManager.OpenSkin(purchaseExt.SkinType);
                        break;
                }
            }
        }
    }
}