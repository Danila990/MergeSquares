using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeadboarsFlyScores : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Action OnFlyEnd = () => { };

    public void Init(int scores, Action callback)
    {
        text.text = scores.ToString();
    }
}
