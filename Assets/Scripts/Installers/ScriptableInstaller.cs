using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Audio;
using Core.Localization;
using Core.Repositories;
using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using Levels.Repositories;
using Rewards;
using Rewards.Models;
using UnityEditor;
using UnityEngine;
using Utils;
using Zenject;

namespace Installers
{
    public class RepositoryState
    {
        public bool needUpdate;
        public IAbstractRepository repository;

        public List<RepositoryState> deps;
        public bool HaveDeps => deps != null && deps.Count > 0;

        public RepositoryState(IAbstractRepository repository)
        {
            this.repository = repository;
        }

        public void AddDep(RepositoryState dep)
        {
            if (deps == null)
            {
                deps = new List<RepositoryState>();
            }

            deps.Add(dep);
        }
    }

    [CreateAssetMenu(fileName = "ScriptableInstaller", menuName = "Installers/ScriptableInstaller")]
    public class ScriptableInstaller : ScriptableObjectInstaller<ScriptableInstaller>, ICollectable<SoundSource>
    {
        [SerializeField] private LocalizationRepository localizationRepository;
        [SerializeField] private List<string> foldersToScan;
        [SerializeField] private List<SoundSource> soundSources = new List<SoundSource>();
        [Space]
        [SerializeField] private List<ScriptableObject> _scriptableRepositories;

        private List<RepositoryState> _repositoryStates = new List<RepositoryState>();
        private bool _updateStarted;

        public override void InstallBindings()
        {
            Container.Bind<LocalizationRepository>().FromScriptableObject(localizationRepository).AsSingle();

            foreach (var soundSource in soundSources)
            {
                Container.QueueForInject(soundSource);
            }

            foreach(var repository in _scriptableRepositories)
            {
                Container.BindInterfacesAndSelfTo(repository.GetType()).FromScriptableObject(repository).AsSingle();
            }

            GameSignalsInstaller.Install(Container);
        }

        public void ResetData()
        {
            soundSources.Clear();
        }
        
        public void SetData(List<SoundSource> data)
        {
            soundSources.AddRange(data);
        }

        public List<string> GetRootFolders()
        {
            return foldersToScan;
        }

        public List<RepositoryState> CollectRepositoryStates()
        {
            var repositoryStates = new List<RepositoryState>();
            foreach (var repository in _scriptableRepositories)
            {
                if(repository is not IAbstractRepository abstractRepository)
                {
                    continue;
                }

                repositoryStates.Add(new RepositoryState(abstractRepository));
            }

            return repositoryStates;
        }
        
        public void UpdateRepositories()
        {
#if UNITY_EDITOR
            if (!_updateStarted)
            {
                _updateStarted = true;
                Debug.Log($"[ScriptableInstaller][UpdateRepositories] Update started!");

                _repositoryStates = CollectRepositoryStates();

                foreach (var repositoryState in _repositoryStates)
                {
                    StartUpdateRepository(repositoryState);
                }

                EditorCoroutineRunner.StartCoroutine(FinishUpdate());
            }
#endif
        }

        public void ResetUpdate()
        {
#if UNITY_EDITOR
            _updateStarted = false;
#endif
        }
        
        public void UpdateRepositoriesFromCsv()
        {
#if UNITY_EDITOR
            if (!_updateStarted)
            {
                _updateStarted = true;
                Debug.Log($"[ScriptableInstaller][UpdateRepositories] Update started!");

                _repositoryStates = CollectRepositoryStates();

                foreach (var repositoryState in _repositoryStates)
                {
                    StartUpdateRepositoryFromCsv(repositoryState);
                }

                EditorCoroutineRunner.StartCoroutine(FinishUpdate());
            }
#endif
        }

        private T GetScriptableRepository<T>() where T : ScriptableObject
        {
            return _scriptableRepositories.Find(x => x is T) as T;
        }

        private void StartUpdateRepository(RepositoryState repositoryState)
        {
            repositoryState.needUpdate = true;
            SpreadsheetManager.ReadPublicSpreadsheet(
                new GSTU_Search(
                    repositoryState.repository.AssociatedSheet,
                    repositoryState.repository.AssociatedWorksheet
                ),
                ss =>
                {
                    repositoryState.needUpdate = false;
                    repositoryState.repository.UpdateRepository(ss);
                    UpdateDependencies(repositoryState);
                    SaveAsset((ScriptableObject) repositoryState.repository);
                }
            );
        }
        
        private void StartUpdateRepositoryFromCsv(RepositoryState repositoryState)
        {
            repositoryState.needUpdate = true;
            StreamReader reader = new StreamReader(CsvFileWriter.GetPath(repositoryState.repository.AssociatedWorksheet));
            var ss = GstuSpreadSheet.CreateSheetFromString(reader.ReadToEnd());
            repositoryState.needUpdate = false;
            repositoryState.repository.UpdateRepository(ss);
            UpdateDependencies(repositoryState);
            SaveAsset((ScriptableObject) repositoryState.repository);
        }

        private void SaveAsset(Object target)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
#endif
        }
        
        private void UpdateDependencies(RepositoryState repositoryState)
        {
            if (repositoryState.HaveDeps)
            {
                var deps = new List<RepositoryState>(repositoryState.deps);
                // remove all for scan below
                repositoryState.deps.Clear();
                foreach (var repositoryStateDep in deps)
                {
                    var contains = false;
                    foreach (var state in _repositoryStates)
                    {
                        if (state.HaveDeps && state.deps.Contains(repositoryStateDep))
                        {
                            contains = true;
                            break;
                        }
                    }

                    if (!contains)
                    {
                        _repositoryStates.Add(repositoryStateDep);
                        StartUpdateRepository(repositoryStateDep);
                    }
                }
            }
        }

        private IEnumerator FinishUpdate()
        {
            var needUpdate = true;
            while (needUpdate)
            {
                needUpdate = false;
                foreach (var repositoryState in _repositoryStates)
                {
                    if (repositoryState.needUpdate)
                    {
                        needUpdate = true;
                        break;
                    }
                }

                yield return null;
            }

            var resourceRepository = GetScriptableRepository<ResourceRepository>();
            if (resourceRepository)
            {
                resourceRepository.DownloadImages();
                SaveAsset(resourceRepository);
            }

            _updateStarted = false;

            Debug.Log($"[ScriptableInstaller][FinishUpdate] Update finished!");
            Debug.Log($"<color=lime>Start test!</color>");

            var playerLevelRepository = GetScriptableRepository<PlayerLevelRepository>();
            if (playerLevelRepository)
            {
                Debug.Log($"<color=aqua>Check player level rewards...</color>");
                foreach (var playerLevelModel in playerLevelRepository.GetData())
                {
                    TestRewards($"In level {playerLevelModel.id}", playerLevelModel.rewards);
                }
            }

            Debug.Log($"<color=lime>End test!</color>");
        }

        private void TestRewards(string logPrefix, List<RewardModel> rewardModels)
        {
            var rewardRepository = GetScriptableRepository<RewardRepository>();
            if (!rewardRepository)
            {
                Debug.Log($"<color=red> Error: Reward repository not found!</color>");
                return;
            }

            foreach (var model in rewardModels)
            {
                var resourceModel = rewardRepository.GetById(model.id);
                if (resourceModel == null)
                {
                    Debug.Log(
                        $"{logPrefix} cant find reward resource with id: <color=red>{model.id}</color> in rewardRepository");
                }
            }
        }
        
        private void TestRewards(string logPrefix, List<RangeStatRewardModel> rewardModels)
        {
            var rewardRepository = GetScriptableRepository<RewardRepository>();
            if (!rewardRepository)
            {
                Debug.Log($"<color=red> Error: Reward repository not found!</color>");
                return;
            }

            foreach (var model in rewardModels)
            {
                var resourceModel = rewardRepository.GetById(model.baseReward.id);
                if (resourceModel == null)
                {
                    Debug.Log(
                        $"{logPrefix} cant find reward resource with id: <color=red>{model.baseReward.id}</color> in rewardRepository");
                    continue;
                }

                if(resourceModel.rewardType != ERewardType.GameStat)
                {
                    Debug.Log(
                        $"{logPrefix} cant find reward resource with id: <color=red>{model.baseReward.id}</color> and stat type in rewardRepository");
                }
            }
        }
        
        private void TestRewards(string logPrefix, List<RangeUnitRewardModel> rewardModels)
        {
            var rewardRepository = GetScriptableRepository<RewardRepository>();
            if (!rewardRepository)
            {
                Debug.Log($"<color=red> Error: Reward repository not found!</color>");
                return;
            }

            foreach (var model in rewardModels)
            {
                foreach (var unitRewardModel in model.rewards)
                {
                    var resourceModel = rewardRepository.GetById(unitRewardModel.baseReward.id);
                    if (resourceModel == null)
                    {
                        Debug.Log(
                            $"{logPrefix} cant find reward resource with id: <color=red>{unitRewardModel.baseReward.id}</color> in rewardRepository");
                        continue;
                    }

                    switch (resourceModel.rewardType)
                    {
                        default:
                            Debug.Log(
                                $"{logPrefix} cant find reward resource with id: <color=red>{unitRewardModel.baseReward.id}</color> and unit type in rewardRepository");
                            break;
                    }
                }
            }
        }
    }
}