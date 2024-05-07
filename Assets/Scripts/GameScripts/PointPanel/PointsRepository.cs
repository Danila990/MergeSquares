using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.PointPanel
{
    public enum EPointId
    {
        None = 0,
        Blue = 1,
        Brown = 2,
        GreenDark = 3,
        Dark = 4,
        Pink = 5,
        Orange = 6,
        Red = 7,
        Turquoise = 8,
        Violet = 9,
        White = 10,
        Yellow = 11
    }
    
    [Serializable]
    public class SkinPointBase
    {
        public Sprite sprite;
    }

    [Serializable]
    public class SkinPoint : SkinPointBase
    {
        public ESkinPointId skinPointId;
    }
    
    [Serializable]
    public class HatPoint : SkinPointBase
    {
        public EHatPointId hatPointId;
    }

    [Serializable]
    public class PointVariations
    {
        public EPointId id;
        public List<SkinPoint> sprites;
        public List<HatPoint> hats;
        public Sprite highlight;
    }

    [CreateAssetMenu(fileName = "PointsRepository", menuName = "Repositories/PointsRepository")]
    public class PointsRepository : ScriptableObject
    {
        public List<PointVariations> pointVariationsList = new List<PointVariations>();
    }
}