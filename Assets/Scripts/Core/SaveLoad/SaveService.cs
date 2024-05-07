using System;
using System.Collections.Generic;
using CloudServices;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;
using PlayerPrefs = UnityEngine.PlayerPrefs;

namespace Core.SaveLoad
{

    [Serializable]
    public class LoadContext
    {
        public TimeSpan playerOfflineTime;
    }

    [Serializable]
    public abstract class AllSaveDataElem
    {
        public string key;

        public AllSaveDataElem(string key)
        {
            this.key = key;
        }
    }

    [Serializable]
    public class AllSaveDataStrElem : AllSaveDataElem
    {
        public string data;

        public AllSaveDataStrElem(string key, string data) : base(key)
        {
            this.data = data;
        }
    }

    [Serializable]
    public class AllSaveDataIntElem : AllSaveDataElem
    {
        public int data;

        public AllSaveDataIntElem(string key, int data) : base(key)
        {
            this.data = data;
        }
    }

    [Serializable]
    public class GameSaveData
    {
        public long saveTime;
        public string saveId;
        public List<AllSaveDataStrElem> strData = new();
        public List<AllSaveDataIntElem> intData = new();

        public GameSaveData()
        {
            saveId = Guid.NewGuid().ToString();
        }

        public void SetString(string key, string data)
        {
            var oldData = GetStringData(key);
            if (oldData != null)
            {
                oldData.data = data;
            }
            else
            {
                strData.Add(new AllSaveDataStrElem(key, data));
            }
        }

        public void SetInt(string key, int data)
        {
            var oldData = GetIntData(key);
            if (oldData != null)
            {
                oldData.data = data;
            }
            else
            {
                intData.Add(new AllSaveDataIntElem(key, data));
            }
        }

        public string GetString(string key, string defaultData)
        {
            foreach (var data in strData)
            {
                if (data.key == key)
                {
                    return data.data;
                }
            }

            return defaultData;
        }

        public int GetInt(string key, int defaultData)
        {
            foreach (var data in intData)
            {
                if (data.key == key)
                {
                    return data.data;
                }
            }

            return defaultData;
        }

        public bool HasKeyInt(string key)
        {
            foreach (var data in intData)
            {
                if (data.key == key)
                {
                    return true;
                }
            }

            return false;
        }
        
        public AllSaveDataStrElem GetStringData(string key)
        {
            foreach (var data in strData)
            {
                if (data.key == key)
                {
                    return data;
                }
            }

            return null;
        }

        public AllSaveDataIntElem GetIntData(string key)
        {
            foreach (var data in intData)
            {
                if (data.key == key)
                {
                    return data;
                }
            }

            return null;
        }
        
        public override string ToString() => JsonUtility.ToJson(this, true);
    }

    public class SaveService : MonoBehaviour
    {
        [SerializeField] private bool verbose;
        [SerializeField] private bool needWatch;
        [SerializeField] private string defaultSaveKey;
        [SerializeField] private string timestampSaveKey;
        [SerializeField] private string privacyPolicySaveKey;
        [SerializeField] private List<Saver> savers;

        public event Action<LoadContext> LoadFinished = context => { };
        public bool SaveLocked { get; set; }
        public IReadOnlyList<string> SaveKeys => _saveKeysHolder.Data.keys;
        public string CurrentKey => _currentKey;
        public bool IsGameNew => _newGame;
        public bool IsAppStarted => _appStarted;
        
        public string SaveId => string.IsNullOrEmpty(_saveData.saveId) ? "old saves" : _saveData.saveId;

        private const string SaveKeysHolderKey = "SaveKeysHolderKey";
        private const string CurrentSaveKey = "DefaultKey";
        private const string AllDataKey = "AllData";
        private const string Service = "SaveService";

        private SaveKeysHolder _saveKeysHolder;
        private GameSaveData _saveData = new();

        private float _prevSaveTime;
        private bool _needSave;
        private bool _needForceSave;
        private string _currentKey;
        private bool _newGame;
        private bool _appStarted;
        private bool _privacyPolicyAgreed;
        private Watcher _watcher;
        
        private CloudService _cloudService;

        [Inject]
        public void Construct(CloudService cloudService)
        {
            _cloudService = cloudService;
        }
        
        private void Start()
        {
            _watcher = new(Service, needWatch);
            _saveKeysHolder = new SaveKeysHolder(SaveKeysHolderKey, new List<string> {defaultSaveKey});
            _currentKey = PlayerPrefs.GetString(CurrentSaveKey, defaultSaveKey);

            foreach (var saver in savers)
            {
                saver.SaveNeeded += OnSaveNeeded;
            }

            ExternalLoad();
        }

        private void OnDestroy()
        {
            foreach (var saver in savers)
            {
                saver.SaveNeeded -= OnSaveNeeded;
            }
        }

        private void Update()
        {
            if (_needSave && !SaveLocked /*&& _prevSaveTime + saveCooldown < Time.time*/)
            {
                _prevSaveTime = Time.time;
                _needSave = false;
                if (_needForceSave)
                {
                    _needForceSave = false;
                    ForceSave();
                }
                else
                {
                    Save();
                }
            }
        }
        
        public void SetNeedWatch(bool value)
        {
            needWatch = value;
        }
        
        public void SetVerbose(bool value)
        {
            verbose = value;
        }
        
        [UsedImplicitly]
        public void ClearAllSavesDebug()
        {
            _cloudService.CloudProvider.ClearSaves(PlayerPrefs.DeleteAll);
        }

        public void SetAgreementState(bool termsOfUse)
        {
            if (termsOfUse)
            {
                // _termsOfUseAgreed = true;
            }
            else
            {
                _privacyPolicyAgreed = true;
            }

            ForceSave();
        }

        public bool GetAgreementState(bool termsOfUse)
        {
            if (termsOfUse)
            {
                // return _termsOfUseAgreed;
                return true;
            }

            return _privacyPolicyAgreed;
        }

        public void ForceSave()
        {
            Save(true);
        }

        /// <summary>
        /// Creates new save controller to hold and use data.
        /// </summary>
        /// <param name="key">Key of created controller</param>
        /// <param name="empty">If true - creates controller from zero progress. Creates controller from current progress otherwise</param>
        public void CreateSave(string key, bool empty = false)
        {
            _saveKeysHolder.AddKey(key);
            SaveTo(key, empty);
        }

        public void ClearSave()
        {
            _saveData = new();
            ExternalSave(true);
            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// Saves current progress to save with specified key.
        /// </summary>
        /// <param name="key">Specified key</param>
        /// <param name="empty">Create empty save</param>
        /// <param name="force">Force save</param>
        public void SaveTo(string key = null, bool empty = false, bool force = false)
        {
            _watcher.StartWatch("SaveTo");
            key = String.IsNullOrEmpty(key) ? defaultSaveKey : key;
            foreach (var saver in savers)
            {
                if (verbose)
                {
                    var saveKey = key + saver.Key;
                    var dataToSave = empty ? "" : saver.DataSaved?.Invoke();
                    _saveData.SetString(saveKey, dataToSave);
                    print($"[SaveService][SaveTo] key: {saveKey} data: {dataToSave}");
                }
                else
                {
                    _saveData.SetString(key + saver.Key, empty ? "" : saver.DataSaved?.Invoke());
                }
            }

            _saveData.saveTime = Timestamp.GetTicks();
            _saveData.SetString(key + timestampSaveKey, Timestamp.GetStringTicks());
            _saveData.SetInt(key + privacyPolicySaveKey, _privacyPolicyAgreed ? 1 : 0);
            // game.SetInt(key + termsOfUseSaveKey, _termsOfUseAgreed ? 1 : 0);
            ExternalSave(force);
            _watcher.StopWatch("SaveTo");
        }

        /// <summary>
        /// Removes data about save with specified key.
        /// </summary>
        /// <param name="key">Specified key</param>
        public void RemoveSaveFrom(string key)
        {
            key = String.IsNullOrEmpty(key) ? defaultSaveKey : key;
            _saveKeysHolder.RemoveKey(key);
            foreach (var saver in savers)
            {
                PlayerPrefs.DeleteKey(key + saver.Key);
            }

            PlayerPrefs.DeleteKey(key + timestampSaveKey);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Finds save controller by specified key and applies it to current progress.
        /// </summary>
        /// <param name="key">Key of target save controller</param>
        /// <param name="force">If true, creates new save if can't find already existing</param>
        /// <param name="empty">If true, creates save from zero progress when force is true</param>
        /// <returns>True if load is exist. False otherwise</returns>
        public bool TryChooseSave(string key, bool force = false, bool empty = false)
        {
            if (_saveKeysHolder.IsSaveExist(key))
            {
                PlayerPrefs.SetString(CurrentSaveKey, key);
                PlayerPrefs.Save();
                return true;
            }

            if (force)
            {
                CreateSave(key, empty);
                PlayerPrefs.SetString(CurrentSaveKey, key);
                PlayerPrefs.Save();
                return true;
            }

            return false;
        }

        private void OnSaveNeeded(bool force)
        {
            _needSave = true;
            _needForceSave = force;
        }

        private void Save(bool force = false)
        {
            SaveTo(null, false, force);
        }

        private void ExternalSave(bool force = false)
        {
            _cloudService.CloudProvider.StartSave(_saveData.ToString(), saveData =>
            {
                PlayerPrefs.SetString(AllDataKey, saveData);
                PlayerPrefs.Save();
            }, force);
        }

        private void ExternalLoad()
        {
            _cloudService.CloudProvider.Loaded += OnLoaded;
            _cloudService.CloudProvider.StartLoad(() => PlayerPrefs.GetString(AllDataKey, ""));
        }

        private void OnLoaded(string json, string localJson)
        {
            _watcher.StartWatch("OnLoaded");
            _saveData = ChooseLast(JsonUtility.FromJson<GameSaveData>(json), JsonUtility.FromJson<GameSaveData>(localJson));

            _privacyPolicyAgreed = _saveData.GetInt(_currentKey + privacyPolicySaveKey, 0) == 1;
            // _termsOfUseAgreed = _saveData.GetInt(_currentKey + termsOfUseSaveKey, 0) == 1;

            var timestampKey = _currentKey + timestampSaveKey;
            if (!_saveData.HasKeyInt(timestampKey))
            {
                _newGame = true;
            }

            var timestamp = _saveData.GetString(timestampKey, "0");

            var context = new LoadContext {playerOfflineTime = Timestamp.CalculateTimeDiff(timestamp)};
            foreach (var saver in savers)
            {
                if (verbose)
                {
                    var loadKey = _currentKey + saver.Key;
                    var data = _saveData.GetString(_currentKey + saver.Key, "");
                    saver.DataLoaded?.Invoke(data, context);
                    print($"[SaveService][Load] key: {loadKey} data: {data}");
                }
                else
                {
                    var data = _saveData.GetString(_currentKey + saver.Key, "");
                    saver.DataLoaded?.Invoke(data, context);
                }
            }

            foreach (var saver in savers)
            {
                saver.DataLoadFinished?.Invoke(context);
            }

            LoadFinished.Invoke(context);
            _watcher.StopWatch("OnLoaded");
            if (verbose)
            {
                Debug.Log($"[SaveService][OnLoaded] Time since app started: {Time.realtimeSinceStartup} s");
            }
            _appStarted = true;
        }

        private GameSaveData ChooseLast(GameSaveData data, GameSaveData localData)
        {
            if (data == null && localData == null)
            {
                return new();
            }
            
            if (data == null)
            {
                return localData;
            }
            
            if (localData == null)
            {
                return data;
            }
            Debug.Log($"[SaveService][ChooseLast] choose cloud save: {data.saveTime >= localData.saveTime}");

            return data.saveTime >= localData.saveTime ? data : localData;

        }

#if UNITY_EDITOR
        [MenuItem("Tools/Reset save data/Reset")]
#endif
        public static void ResetAllSaveData()
        {
            PlayerPrefs.DeleteAll();
        }
        
#if UNITY_EDITOR
        [MenuItem("Tools/Reset save data/View prefs")]
#endif
        public static void ViewPrefs()
        {
            var json = PlayerPrefs.GetString(AllDataKey, "");
            Debug.Log($"[SaveService][ViewPrefs] prefs: {json}");
        }
    }
}