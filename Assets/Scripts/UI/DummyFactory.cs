using UI;
using UnityEngine;

namespace MergeBoard.UI
{
    public class DummyFactory : MonoBehaviour
    {
        [SerializeField] private Dummy dummyPrefab;

        public RectTransform dummyRoot;
        public Dummy Create()
        {
            return Instantiate(dummyPrefab, dummyRoot);
        }
    }
}