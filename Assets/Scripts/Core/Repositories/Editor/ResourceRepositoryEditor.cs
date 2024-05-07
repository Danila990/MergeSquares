using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Utils;
using Utils.Editor;

namespace Core.Repositories.Editor
{
    [CustomEditor(typeof(ResourceRepository))]
    public class ResourceRepositoryEditor : UnityEditor.Editor
    {
        private ResourceRepository _target;

        private void OnEnable()
        {
            _target = (ResourceRepository) target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "Download", UpdateItems);
            UIElementsEditorHelper.AddButton(container, "UpdateImages", UpdateImages);
            UIElementsEditorHelper.AddButton(container, "DeleteImages", DeleteImages);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }

        private void UpdateItems()
        {
            _target.DownloadImages();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
        
        private void UpdateImages()
        {
            _target.UpdateImages();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
        private void DeleteImages()
        {
            _target.DeleteImages();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}