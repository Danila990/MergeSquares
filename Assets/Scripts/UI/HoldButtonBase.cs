using System;
using System.Linq;
using UI.Interfaces;
using UnityEngine;

namespace Game.UI
{
    public abstract class HoldButtonBase : MonoBehaviour, IHoldButton
    {
        public event Action<float> HoldPerformed = _ => { };
        public event Action<bool> ToggleHold = _ => { };
        public event Action<float> InertiaPerformed = _ => { };

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private AnimationCurve inertiaCurve;


        private bool _isHolding = false;
        private bool _isInertia = false;
        private float _holdingDuration = 0f;
        private float _inertiaDuration = 0f;
        private float _prevValue = 0;

        public float HoldingDuration => _holdingDuration;

        public void SetProgress(float value)
        {
            _holdingDuration = value;
        }

        public void StopInertia()
        {
            _isInertia = false;
        }
        public void Release()
        {
            _isHolding = false;
        }

        protected void ToggleHolding(bool hold)
        {
            _inertiaDuration = 0f;
            _isHolding = hold;
            _isInertia = !hold;
            ToggleHold.Invoke(hold);
        }
        
        private void Update()
        {
            if (_isHolding)
            {
                _holdingDuration += Time.deltaTime;
                var progress = curve.Evaluate(_holdingDuration);
                HoldPerformed.Invoke(progress);
            }
            else if (_isInertia)
            {
                _inertiaDuration += Time.deltaTime;
                var newValue = inertiaCurve.Evaluate(_inertiaDuration);
                _holdingDuration += newValue;
                var progress = curve.Evaluate(_holdingDuration);
                InertiaPerformed.Invoke(progress);
                if (newValue == _prevValue)
                {
                    _isInertia = false;
                }
                else
                {
                    _prevValue = newValue;
                }
            }
        }
    }
}
