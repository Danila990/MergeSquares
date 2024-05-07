using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shop.Repositories
{
    [Serializable]
    public class ShopCategoriesData
    {
        public List<ShopCategoryData> categories = new List<ShopCategoryData>();
    }

    [Serializable]
    public class ShopCategoryData
    {
        public EShopCategory category;
        public List<ShopPanelData> panels = new List<ShopPanelData>();
    }

    [Serializable]
    public class ShopPanelData
    {
        public string unitId;
        public int maxCount;
        public int hardCurrencyPrice;
    }

    [CreateAssetMenu(fileName = "ShopDataRepository", menuName = "Repositories/ShopDataRepository")]
    public class ShopDataRepository : ScriptableObject
    {
        [SerializeField] private ShopCategoriesData categoriesData;

        public ShopPanelData GetPanelDataByIndex(EShopCategory category, int index) => 
            GetCategoryData(category).panels[index];
        
        public ShopPanelData GetPanelDataById(EShopCategory category, string id)
        {
            var categoryData = GetCategoryData(category);
            return categoryData.panels.Find(panelData => panelData.unitId == id);
        }
        
        public ShopCategoryData GetCategoryData(EShopCategory requiredCategory) =>
            categoriesData.categories.Find(category => category.category == requiredCategory);
    }
}

