using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderMultiplierTrigger : MonoBehaviour
{
    [SerializeField] private SquareSlider squareSlider;
    [SerializeField] private int multiplier = 1;
    private void OnTriggerEnter2D(Collider2D other)
    {
        squareSlider.SetMultiplier(multiplier);
    }
}
