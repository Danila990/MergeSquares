using System;
using System.Collections.Generic;
using System.Linq;
using Core.Repositories;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Core.Localization
{
    [Serializable]
    public class LocalizeSaveData
    {
        public string fieldName;
        public string language;
        public string value;
    }

    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "Repositories/Localization")]
    public class LocalizationRepository : AbstractRepository<List<LocalizeModel>>, ISerializationCallbackReceiver
    {
        public override string AssociatedSheet => "1jKqJPTqTvimIKQEwgCV281ejgnCJFsjvVp_FyxBrzCM";
        public override string AssociatedWorksheet => "Data";
        public IReadOnlyList<string> AdditionalWorksheets => additionalWorksheets;

        [SerializeField] private List<string> additionalWorksheets = new();
        [SerializeField] private List<LocalizeSaveData> localizeSave = new();
        [SerializeField] private List<string> languageList = new();

        public event Action LanguageChanged = () => { };

        private string _currentLanguage = "Default";
        public Dictionary<string, Dictionary<string, string>> _languages = new(); // lang, value

        public List<string> GetLanguagesList() => new(languageList);

        public void SetLanguage(string language)
        {
            if (languageList.Contains(language))
            {
                _currentLanguage = language;
                LanguageChanged.Invoke();
            }
        }

        public string GetTextInCurrentLocale(string fieldName)
        {
            var res = "NULL";
            if (_languages.TryGetValue(_currentLanguage, out var l))
            {
                if (l.TryGetValue(fieldName, out var value))
                {
                    res = value;
                }
            }
            return res;
        }

        public override void UpdateRepository(GstuSpreadSheet spreadSheet)
        {
            UpdateRepository(spreadSheet, true);
        }

        public void UpdateRepository(GstuSpreadSheet spreadSheet, bool newData, string spreadSheetName = "")
        {
            if(newData)
            {
                data = new List<LocalizeModel>();
            }
            foreach (var cell in spreadSheet.columns["Key"])
            {
                if (cell.value == "Key")
                    continue;
                
                if (cell.value.StartsWith("//"))
                    continue;
                
                if (cell.value.Trim() == string.Empty)
                    continue;

                if (cell.value == "--")
                    break;

                var row = spreadSheet.rows[cell.value];
                var model = new LocalizeModel {fieldName = cell.value};
                foreach (var rowCell in row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    if (rowCell.columnId == "Key")
                        continue;

                    if (!model.values.ContainsKey(rowCell.columnId))
                    {
                        model.values.Add(rowCell.columnId, rowCell.value);
                    }
                }

                if(data.Find(m => m.fieldName == model.fieldName) == null)
                {
                    data.Add(model);
                }
                else
                {
                    Debug.LogWarning($"[LocalizationRepository][UpdateRepository] Got same key: {model.fieldName} in list: {spreadSheetName}");
                }
            }
        }

        public void OnBeforeSerialize()
        {
            localizeSave.Clear();
            languageList.Clear();
            foreach (var model in data)
            {
                foreach (var modelValue in model.values)
                {
                    localizeSave.Add(new LocalizeSaveData()
                    {
                        fieldName = model.fieldName,
                        language = modelValue.Key,
                        value = modelValue.Value
                    });

                    if (!languageList.Contains(modelValue.Key) && modelValue.Key != "Default")
                    {
                        languageList.Add(modelValue.Key);
                    }
                }
            }
        }

        public void OnAfterDeserialize()
        {
            data = new List<LocalizeModel>();
            foreach (var l in languageList)
            {
                _languages.Add(l, new());
            }
            foreach (var save in localizeSave)
            {
                var model = data.GetBy(value => value.fieldName == save.fieldName);
                if (model != default)
                {
                    if (!model.values.ContainsKey(save.language))
                    {
                        model.values.Add(save.language, save.value);
                    }
                }
                else
                {
                    var newModel = new LocalizeModel {fieldName = save.fieldName};
                    newModel.values.Add(save.language, save.value);
                    data.Add(newModel);
                }

                if (!_languages[save.language].ContainsKey(save.fieldName))
                {
                    _languages[save.language].Add(save.fieldName, save.value);
                }
            }
        }
    }
}