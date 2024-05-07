using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class GridInstaller : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup gridLayoutGroup;

        public void ChangeGridLayout(int rawCount)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = rawCount;
            
            var itemSize = CalculateItemSize(rawCount);
            gridLayoutGroup.cellSize = itemSize;

            var spacing = new Vector2(itemSize.x / 3 - 1, itemSize.y / 2);
            gridLayoutGroup.spacing = spacing;
        }

        private Vector2 CalculateItemSize(int rawCount)
        {
            var groupSize = GetComponent<RectTransform>().rect.size.x - (gridLayoutGroup.padding.left + gridLayoutGroup.padding.right);
            var itemSize = Vector2.one * groupSize / (2 * rawCount - 1);
            itemSize += Vector2.one * itemSize.x / 3;
            return itemSize;
        }
    }
}