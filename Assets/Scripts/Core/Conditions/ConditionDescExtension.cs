using UnityEngine;

namespace Core.Conditions
{
    public abstract class ConditionDescExtension : ScriptableObject
    {
        public abstract ConditionBase CreateCondition(ConditionBuilderContext context);
    }
}