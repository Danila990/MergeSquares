using TMPro;
using UnityEngine;

namespace UI
{
    public class TextSetter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public void Set(string value) => text.text = value;
    }
}