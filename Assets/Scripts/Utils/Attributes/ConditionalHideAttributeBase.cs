using System;
using UnityEditor;
using UnityEngine;

namespace Utils.Attributes
{
    [AttributeUsage(ValidOn)]
    public abstract class ConditionalHideAttributeBase : PropertyAttribute
    {
        protected const AttributeTargets ValidOn = AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct;
        
        #if UNITY_EDITOR
        protected ConditionalHideAttributeBase(Predicate<SerializedProperty> predicate, string conditionalSourceField, bool hideInInspector, bool isInverse = false)
        {
            _predicate = predicate;
            _isInverse = isInverse;
            
            ConditionalSourceField = conditionalSourceField;
            HideInInspector = hideInInspector;
        }

        private readonly Predicate<SerializedProperty> _predicate;
        private readonly bool _isInverse;

        public string ConditionalSourceField
        {
            get;
        }

        public bool HideInInspector
        {
            get;
        }

        public bool IsFit(SerializedProperty property)
        {
            var isFit = _predicate.Invoke(property);
            if (_isInverse)
            {
                isFit = !isFit;
            }

            return isFit;
        }
        #endif
    }
}