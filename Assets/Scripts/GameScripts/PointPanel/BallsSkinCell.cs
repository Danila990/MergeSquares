using Core.Anchors;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.PointPanel
{
    public class BallsSkinCell : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Sprite lockSprite;
        [SerializeField] private Anchor anchor;
        [SerializeField] private Image frame;
        [SerializeField] private Sprite usingFrame;
        [SerializeField] private Sprite nextFrame;

        public BallSkin Skin => _ballSkin;
        
        private BallSkin _ballSkin;
        private bool locked = false;
    
        private global::GameScripts.PointPanel.PointPanel _pointPanel;

        [Inject]
        private void Construct(global::GameScripts.PointPanel.PointPanel pointPanel)
        {
            _pointPanel = pointPanel;
        }

        public void OnClick()
        {
            if (!locked)
            {
                _pointPanel.SetNewSkin(_ballSkin);
            }
        }

        public void Show(BallSkin ballSkin, int num)
        {
            this._ballSkin = ballSkin;
            anchor.Id += num.ToString();
        }

        public void Unlock()
        {
            locked = false;
            SetSprite(_ballSkin.icon);
        }

        public void Lock()
        {
            locked = true;
            SetSprite(lockSprite);
        }

        public void SetFrameNext()
        {
            frame.gameObject.SetActive(true);
            frame.sprite = nextFrame;
            frame.SetNativeSize();
        }

        public void SetFrameUsing()
        {
            frame.gameObject.SetActive(true);
            frame.sprite = usingFrame;
            frame.SetNativeSize();
        }

        public void ClearFrame()
        {
            frame.gameObject.SetActive(false);
        }

        private void SetSprite(Sprite sprite)
        {
            icon.sprite = sprite;
            icon.SetNativeSize();
        }
    }
}
