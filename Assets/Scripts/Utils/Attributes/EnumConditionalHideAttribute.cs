using System;
using UnityEditor;

namespace Utils.Attributes
{
    // DO NOT USE IT WITH FLAG ENUMS!
    [AttributeUsage(ValidOn)]
    public class EnumConditionalHideAttribute : ConditionalHideAttributeBase
    {
        #if UNITY_EDITOR
        public EnumConditionalHideAttribute(string conditionalSourceField, object value, bool hideInInspector = false, bool isInverse = false) : base(CreatePredicate(value), conditionalSourceField, hideInInspector, isInverse)
        {
            
        }
        #else
        public EnumConditionalHideAttribute(string conditionalSourceField, object value, bool hideInInspector = false)
        {
            
        }
        #endif
        
        #if UNITY_EDITOR
        private static Predicate<SerializedProperty> CreatePredicate(object value)
        {
            return IsFit;

            bool IsFit(SerializedProperty property)
            {
                var names = property.enumNames;
                var index = property.enumValueIndex;

                var self = names[index];
                var other = value.ToString();
                
                return self == other;
            }
        }
        #endif
    }
}