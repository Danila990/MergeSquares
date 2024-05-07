using UnityEditor;
using UnityEngine.UIElements;
using Utils.Editor;

namespace GameScripts.Game2248.Tasks.Editor
{
    [CustomEditor(typeof(TaskService))]
    public class TaskServiceEditor : UnityEditor.Editor
    {
        private TaskService _database;
        
        private void OnEnable()
        {
            _database = (TaskService) target;
        }


        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "AddStat", () => _database.AddStat(ETaskDataType.LevelCompleteCount, 1));
            UIElementsEditorHelper.AddButton(container, "ResetDaily", _database.ResetDaily);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }
    }
}