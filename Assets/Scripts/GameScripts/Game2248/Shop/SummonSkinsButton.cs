using System;
using System.Collections;
using System.Collections.Generic;
using Core.Localization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SummonSkinsButton : MonoBehaviour
{
    [SerializeField] private LocalizeUi countText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button button;

    public Action<SummonSkinsButton> Clicked = (button) => { };

    public int Count => _count;
    public int Cost => _cost;

    private int _count;
    private int _cost;

    public Button Button => button;

    public void Init(int count, int cost)
    {
        _count = count;
        _cost = cost;
        // countText.text = count.ToString();
        costText.text = cost.ToString();
        button.onClick.AddListener(OnClick);
    }
    
    private void Start()
    {
        countText.UpdateArgs(new []{_count.ToString()});
    }

    private void OnClick()
    {
        Clicked.Invoke(this);
    }
}
