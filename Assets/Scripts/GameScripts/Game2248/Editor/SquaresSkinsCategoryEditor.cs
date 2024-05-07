using UnityEditor;
using UnityEngine.UIElements;
using Utils.Editor;

namespace GameScripts.MergeSquares.Shop.Editor
{
    [CustomEditor(typeof(SquaresSkinsCategory))]
    public class SquaresSkinsCategoryEditor : UnityEditor.Editor
    {
        private SquaresSkinsCategory _category;

        private void OnEnable()
        {
            _category = (SquaresSkinsCategory) target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "CreateSkinCells", CreateSkinCells);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }
        
        private void CreateSkinCells()
        {
            _category.CreateSkinCells();
            EditorUtility.SetDirty(target);
        }
    }
}