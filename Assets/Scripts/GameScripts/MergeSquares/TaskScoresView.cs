using GameScripts.MergeSquares.Models;
using GameStats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameScripts.MergeSquares.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.MergeSquares
{
    [Serializable]
    public class TaskSprite
    {
        public ETaskType type;
        public Sprite sprite;
    }

    public class TaskScoresView : MonoBehaviour
    {
        [SerializeField] private List<TaskSprite> tasksSprites;
        [SerializeField] private Image taskImage;

        [SerializeField] private UnitView unitViewCurrent;
        [SerializeField] private UnitView unitViewTarget;
        [SerializeField] private TextMeshProUGUI textCurrent;
        [SerializeField] private TextMeshProUGUI textTarget;

        [Header("Animation")]
        [SerializeField] private float animationDuration;
        [SerializeField] private AnimationCurve scaleAnimationCurve;

        private TaskModel _task;

        public ETaskType TaskType => _task.type;

        public void Init(TaskModel task, int currentValue = 0)
        {
            _task = task;
            InitScoresUI(task, currentValue);
        }

        public void UpdateScore(int value)
        {
            if (_task.type == ETaskType.GetCellWithValue)
            {
                unitViewCurrent.Init(value);
                unitViewCurrent.Animator.AnimateScalePingPong();
            }
        }

        public void UpdateSkin(ESquareSkin skin)
        {
            if (unitViewCurrent.isActiveAndEnabled)
            {
                unitViewCurrent.SetSkin(skin);
                unitViewTarget.SetSkin(skin);
            }
        }

        public void AnimateWin(Action callback = null)
        {
            StartCoroutine(AnimateScalePulse(callback));
        }

        private void InitScoresUI(TaskModel task, int currentValue)
        {
            taskImage.sprite = tasksSprites.Where(t => t.type == TaskType).FirstOrDefault().sprite;
            taskImage.SetNativeSize();


            switch (TaskType)
            {
                case ETaskType.GetCellWithValue:
                    SetUseViewsInsteadText(true);
                    unitViewCurrent.Init(currentValue == 0 ? 2 : currentValue);
                    unitViewTarget.Init(task.value);
                    break;
                case ETaskType.Endless:
                    SetUseViewsInsteadText(false);
                    textTarget.gameObject.SetActive(false);
                    break;
                default:
                    SetUseViewsInsteadText(false);
                    break;
            }
        }

        private void SetUseViewsInsteadText(bool useViews)
        {
            unitViewCurrent.gameObject.SetActive(useViews);
            unitViewTarget.gameObject.SetActive(useViews);

            textCurrent.gameObject.SetActive(!useViews);
            textTarget.gameObject.SetActive(!useViews);
        }

        private IEnumerator AnimateScalePulse(Action callback)
        {
            for (float t = 0; t < animationDuration; t += Time.deltaTime)
            {
                transform.localScale = Vector3.one * scaleAnimationCurve.Evaluate(t / animationDuration);
                yield return null;
            }
            callback?.Invoke();
        }
    }
}
