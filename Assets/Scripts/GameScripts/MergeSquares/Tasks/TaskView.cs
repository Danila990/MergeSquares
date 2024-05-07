using System;
using Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace GameScripts.MergeSquares.Tasks
{
    public class TaskView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rewardAmount;
        [SerializeField] private LocalizeUi taskDesc;
        [SerializeField] private Button claimActiveButton;
        [SerializeField] private Button claimButton;
        [SerializeField] private SlicedFilledImage bar;
        [SerializeField] private GameObject overlay;

        public Action<TaskData> Claimed = data => {};
        
        private TaskData _taskData;

        public void OnClick()
        {
            Claimed.Invoke(_taskData);
        }

        public void Init(TaskData taskData)
        {
            _taskData = taskData;
            UpdateView();
        }

        public void OnChanged(TaskData data)
        {
            if (data == _taskData)
            {
                UpdateView();
            }
        }

        private void UpdateView()
        {
            if (_taskData.model.rewards.Count > 0)
            {
                rewardAmount.text = _taskData.model.rewards[0].value.ToString();
            }
            
            overlay.SetActive(_taskData.claimed);

            taskDesc.SetLocalizationKey(_taskData.model.DescKey, new[] {_taskData.value.ToString(), _taskData.model.targetValue.ToString()});
            claimButton.gameObject.SetActive(_taskData.value < _taskData.model.targetValue);
            claimActiveButton.gameObject.SetActive(_taskData.value >= _taskData.model.targetValue);
            bar.fillAmount = (float)_taskData.value / _taskData.model.targetValue;
        }
    }
}