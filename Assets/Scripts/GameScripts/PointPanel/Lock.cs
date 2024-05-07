using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts.PointPanel
{
    public class Lock : MonoBehaviour
    {
        [SerializeField] private Sprite lockSprite;
        [SerializeField] private Sprite unlockSprite;
        [SerializeField] private Image lockImage;
        [SerializeField] private float waitTime;

        public void ShowUnlock(Action end)
        {
            lockImage.sprite = unlockSprite;
            lockImage.SetNativeSize();
            StartCoroutine(WaitTime(end));
        }

        public void ShowLock()
        {
            lockImage.sprite = lockSprite;
            lockImage.SetNativeSize();
        }
    
        IEnumerator WaitTime(Action end)
        {
            yield return new WaitForSeconds(waitTime);
            end();
        }
    }
}
