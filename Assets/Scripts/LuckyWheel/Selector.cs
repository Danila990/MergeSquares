using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    [SerializeField] private Animator wheelSpinAnimator;

    public void Roll(int resultSector)
    {
        wheelSpinAnimator.SetInteger("Sector", resultSector);
        wheelSpinAnimator.SetTrigger("Rotation");
    }
    
    public void OnRotationEnd() {
        wheelSpinAnimator.SetTrigger("Reset");
    }
}
