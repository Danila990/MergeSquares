using TMPro;
using UnityEngine;

namespace GameScripts.PointPanel
{
    public class LevelCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textNum;

        public void SetLevel(int level)
        {
            textNum.text = level.ToString();
        }
    }
}
