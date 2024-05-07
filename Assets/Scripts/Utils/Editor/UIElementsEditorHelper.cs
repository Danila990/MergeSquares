using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Utils.Editor
{
    public static class UIElementsEditorHelper
    {
        public static void AddButton(VisualElement container, string text, Action clicked)
        {
            Button button = new Button {text = text};
            button.style.width = container.contentRect.width;
            button.style.height = 30;
            button.style.alignSelf = new StyleEnum<Align>(Align.Center);
            button.clickable.clicked += clicked;
            container.Add(button);
        }
        
        public static void FillDefaultInspector(VisualElement container, SerializedObject serializedObject, bool hideScript = false, EventCallback<SerializedPropertyChangeEvent> callback = null)
        {
            SerializedProperty property = serializedObject.GetIterator();
            if (property.NextVisible(true)) // Expand first child.
            {
                do
                {
                    if (property.propertyPath == "m_Script" && hideScript)
                    {
                        continue;
                    }
                    var field = new PropertyField(property);
                    field.name = "PropertyField:" + property.propertyPath;
                    field.RegisterValueChangeCallback(callback);
     
                    if (property.propertyPath == "m_Script" && serializedObject.targetObject != null)
                    {
                        field.SetEnabled(false);
                    }
     
                    container.Add(field);
                }
                while (property.NextVisible(false));
            }
        }
    }
}