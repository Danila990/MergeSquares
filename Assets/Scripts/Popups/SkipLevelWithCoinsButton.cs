using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public class SkipLevelWithCoinsButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsCount;
    [SerializeField] private Button button;

    public void SetButtonState(int cost, bool state)
    {
        coinsCount.text = cost.ToString();
        button.interactable = state;
    }
}
