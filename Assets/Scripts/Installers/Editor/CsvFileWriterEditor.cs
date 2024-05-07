using UnityEditor;
using UnityEngine.UIElements;
using Utils;
using Utils.Editor;

namespace Installers.Editor
{
    [CustomEditor(typeof(CsvFileWriter))]
    public class CsvFileWriterEditor : UnityEditor.Editor
    {
        private CsvFileWriter _writer;

        private void OnEnable()
        {
            _writer = (CsvFileWriter) target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "Write repositories to csv files", WriteAll);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }
        
        private void WriteAll()
        {
            _writer.WriteFiles();
        }
    }
}
