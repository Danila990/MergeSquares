using UnityEngine;
using Utils;
using Zenject;

namespace Core.Conditions.View
{
    public class ConditionalView : MonoBehaviour
    {
        public ConditionDesc[] conditions;
        public string expression = "A";
        public UnityEventBool onChanged;

        private ConditionBase _conditionOrNull;

        private ConditionBuilder _conditionBuilder;

        [Inject]
        public void Construct(ConditionBuilder conditionBuilder)
        {
            _conditionBuilder = conditionBuilder;
            _conditionOrNull = _conditionBuilder.CreateCondition(expression, conditions, OnConditionUpdated);
        }

        private void Start()
        {
            if (_conditionOrNull != null)
            {
                OnConditionUpdated(_conditionOrNull.IsTrue);
            }
        }

        private void OnDestroy()
        {
            if (_conditionOrNull != null)
            {
                _conditionOrNull.Dispose();
                _conditionOrNull = null;
            }
        }

        private void OnConditionUpdated(bool completed)
        {
            onChanged.Invoke(completed);
        }
    }
}