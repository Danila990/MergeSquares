using Core.Repositories.Editor;
using GoogleSheetsToUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Editor;

namespace Core.Localization.Editor
{
    [CustomEditor(typeof(LocalizationRepository))]
    public class LocalizationRepositoryEditor : AbstractRepositoryEditor
    {
        private LocalizationRepository _localizationRepository;

        protected override void Enable()
        {
            _localizationRepository = (LocalizationRepository) target;
        }
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "Download", UpdateItems);
            UIElementsEditorHelper.AddButton(container, "Read from csv", ReadItems);
            UIElementsEditorHelper.AddButton(container, "DownloadAdditions", UpdateAdditionalItems);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }

        public void UpdateAdditionalItems()
        {
            foreach (var worksheet in _localizationRepository.AdditionalWorksheets)
            {
                SpreadsheetManager.ReadPublicSpreadsheet(
                    new GSTU_Search(_localizationRepository.AssociatedSheet, worksheet), ss => UpdateMethod(ss, worksheet));
            }
            Debug.Log($"[LocalizationRepositoryEditor][UpdateItems] Finished!");
        }

        private void UpdateMethod(GstuSpreadSheet ss, string worksheetName)
        {
            _localizationRepository.UpdateRepository(ss, false, worksheetName);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}