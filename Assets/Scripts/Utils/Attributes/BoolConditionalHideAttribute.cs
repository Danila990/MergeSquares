using System;
using UnityEditor;

namespace Utils.Attributes
{
    [AttributeUsage(ValidOn)]
    public class BoolConditionalHideAttribute : ConditionalHideAttributeBase
    {
        #if UNITY_EDITOR
        public BoolConditionalHideAttribute(string conditionalSourceField, bool inverse = false,  bool hideInInspector = false) : base(CreatePredicate(inverse), conditionalSourceField, hideInInspector)
        {
            
        }
        #else
        public BoolConditionalHideAttribute(string conditionalSourceField, bool inverse = false,  bool hideInInspector = false)
        {
            
        }
        #endif
       
        #if UNITY_EDITOR
        private static Predicate<SerializedProperty> CreatePredicate(bool inverse)
        {
            return IsFit;
            
            bool IsFit(SerializedProperty property)
            {
                return property.boolValue != inverse;
            }
        }
        #endif
    }
}