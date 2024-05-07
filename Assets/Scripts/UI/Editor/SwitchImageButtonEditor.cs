using UnityEditor;

namespace UI.Editor
{
    [CustomEditor(typeof(SwitchImageButton))]
    public class SwitchImageButtonEditor : UnityEditor.UI.ButtonEditor
    {
        private SerializedProperty imageProperty;
        private SerializedProperty onSpriteProperty;
        private SerializedProperty offSpriteProperty;

        protected override void OnEnable()
        {
             base.OnEnable();
            
             imageProperty = serializedObject.FindProperty("additionalImage");
             onSpriteProperty = serializedObject.FindProperty("onSprite");
             offSpriteProperty = serializedObject.FindProperty("offSprite");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(imageProperty);
            EditorGUILayout.PropertyField(onSpriteProperty);
            EditorGUILayout.PropertyField(offSpriteProperty);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}

