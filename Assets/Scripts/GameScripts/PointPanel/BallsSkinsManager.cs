using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.PointPanel
{
    [Serializable]
    public enum ESkinPointId
    {
        Default = 0,
        Spots = 1,
        Stars = 2
    }
    
    public enum EHatPointId
    {
        None = 0,
        Glasses = 1,
        Cat = 2,
        Bow = 3,
        Cap = 4,
        Hat = 5,
    }

    [Serializable]
    public class BallSkin
    {
        public Sprite icon;
        public bool isHat;
        public EHatPointId hatId;
        public Vector3 hatOffset = Vector3.zero;
        public ESkinPointId skinId;
        public int openCost = 250;
        
        public override bool Equals(object obj)
        {
            BallSkin ballSkin = obj as BallSkin;
            if (ballSkin == null)
                return false;

            return (icon == ballSkin.icon)
                   && (skinId == ballSkin.skinId)
                   && (isHat == ballSkin.isHat);
        }
    }

    public class BallsSkinsManager: MonoBehaviour
    {
        [SerializeField] private List<BallSkin> skins;

        public List<BallSkin> Skins => skins;
    }
}