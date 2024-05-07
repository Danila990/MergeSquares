using System;
using UnityEngine;

namespace Utils.ClickableList
{
    public abstract class AClickableItem<TParams, TEvent> : MonoBehaviour where TEvent : AClickableItem<TParams, TEvent>
    {
        public Action<TEvent> ButtonClicked;
        public Action<TEvent> InfoButtonClicked;

        protected TParams _initParams;

        public virtual void Init(TParams initParams)
        {
            _initParams = initParams;

            if(!IsParamsNull())
            {
                InitItem(initParams);
            }
        }

        public void OnButtonClicked()
        {
            if (IsClickable())
            {
                ButtonClicked?.Invoke(this as TEvent);
            }
        }

        public void OnInfoButtonClicked()
        {
            InfoButtonClicked?.Invoke(this as TEvent);
        }

        protected abstract void InitItem(TParams initParams);

        protected virtual bool IsClickable()
        {
            return !IsParamsNull();
        }

        protected bool IsParamsNull()
        {
            if(default(TParams) == null)
            {
                return _initParams == null;
            }

            return false;
        }
    }
}
