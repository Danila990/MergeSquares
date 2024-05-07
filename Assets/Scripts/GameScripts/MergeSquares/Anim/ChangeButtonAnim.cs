using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeButtonAnim : MonoBehaviour
{
    private Vector2 startScale;


    private void Awake()
    {
        startScale = transform.localScale;
    }

    public void ChangeOn()
    {
        transform.localScale = new Vector2(startScale.x * 1.2f, startScale.y *1.2f);
    }

    public void ChangeOff()
    {
       transform.localScale = startScale;
    }

}
