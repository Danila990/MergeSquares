using System;
using Core.Executor.Conditions;
using UnityEngine.Events;
using Utils;

namespace Core.Conditions
{
    public abstract class ConditionBase : ICondition
    {
        public abstract bool IsTrue { get; }

        private UnityAction<bool> Changed { get; set; }
        private DelegateWrap<Action<float>> Updated { get; set; }
        private bool _updateSet;

        public void Init(UnityAction<bool> changed)
        {
            Changed = changed;
        }

        public virtual void Dispose()
        {
            Changed = null;
            if (_updateSet)
            {
                Updated.Delegate -= OnUpdate;
                _updateSet = false;
            }
        }

        public void InitUpdate(DelegateWrap<Action<float>> updated)
        {
            if (!_updateSet)
            {
                Updated = updated;
                Updated.Delegate += OnUpdate;
                _updateSet = true;
            }
        }

        protected virtual void OnUpdate(float dt) {}

        public virtual bool Updatable => false;

        protected void MarkChanged()
        {
            if (Changed != null)
            {
                Changed.Invoke(IsTrue);
            }
        }
    }
}