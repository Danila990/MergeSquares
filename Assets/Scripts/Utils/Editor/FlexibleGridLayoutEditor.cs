using UnityEditor;
using UnityEngine.UIElements;

namespace Utils.Editor
{
    [CustomEditor(typeof(FlexibleGridLayout))]
    public class FlexibleGridLayoutEditor : UnityEditor.Editor
    {
        private FlexibleGridLayout _grid;

        private void OnEnable()
        {
            _grid = (FlexibleGridLayout) target;
        }
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "Set editor size", SetSize);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }
        
        private void SetSize()
        {
            _grid.SetSize();
            EditorUtility.SetDirty(target);
        }
    }
}