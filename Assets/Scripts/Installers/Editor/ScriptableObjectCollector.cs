using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using Utils.Editor;

namespace Installers.Editor
{
    public class ScriptableObjectCollector<T> : UnityEditor.Editor where T : ScriptableObject
    {
        private ICollectable<T> _data;

        private void OnEnable()
        {
            _data = (ICollectable<T>) target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "Download sounds", SetCollectedItems);
            UIElementsEditorHelper.AddButton(container, "Update all repositories", UpdateRepositories);
            UIElementsEditorHelper.AddButton(container, "Reset repositories update", ResetRepositoriesUpdate);
            UIElementsEditorHelper.AddButton(container, "Update all Repositories from csv", UpdateRepositoriesFromCsv);
            UIElementsEditorHelper.AddButton(container, "Save all assets", SaveAllAssets);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }

        private void SetCollectedItems()
        {
            _data.ResetData();
            foreach (var rootFolder in _data.GetRootFolders())
            {
                _data.SetData(ScriptableObjectHelpers.GetAllInstances<T>(rootFolder));
            }

            SaveAllAssets();
        }

        private void UpdateRepositories()
        {
            if(_data is ScriptableInstaller installer)
            {
                installer.UpdateRepositories();
            }
        }
        
        private void ResetRepositoriesUpdate()
        {
            if(_data is ScriptableInstaller installer)
            {
                installer.ResetUpdate();
            }
        }
        
        private void UpdateRepositoriesFromCsv()
        {
            if(_data is ScriptableInstaller installer)
            {
                installer.UpdateRepositoriesFromCsv();
            }
        }
        
        private void SaveAllAssets()
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}