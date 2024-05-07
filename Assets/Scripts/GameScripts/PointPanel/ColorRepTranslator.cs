using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.PointPanel
{
    public class ColorRepTranslator : MonoBehaviour
    {
        [SerializeField] private PointsRepository pointRepository;

        public Dictionary<EPointId, PointVariations> AllPointsList => _allPointsList;
    
        private Dictionary<EPointId, PointVariations> _allPointsList = new Dictionary<EPointId, PointVariations>();
    
        private void Awake()
        {
            foreach (var variation in pointRepository.pointVariationsList)
            {
                _allPointsList.Add(variation.id, variation);
            }
        }
    }
}
