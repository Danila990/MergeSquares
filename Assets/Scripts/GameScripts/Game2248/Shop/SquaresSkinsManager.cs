using System;
using GameScripts.MergeSquares.Shop;
using UnityEngine;

namespace GameScripts.Game2248.Shop
{
    [Serializable]
    public class SquaresSkin : SquaresSkin<ESquareSkin>{};
    
    [CreateAssetMenu(fileName = "SquaresSkinsManager2248", menuName = "Squares/SquaresSkinsManager2248")]
    public class SquaresSkinsManager : SquaresSkinsManagerBase<SquaresSkin, ESquareSkin> {}
}