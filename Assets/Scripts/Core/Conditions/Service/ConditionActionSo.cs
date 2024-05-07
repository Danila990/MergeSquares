using System;
using Core.Audio;
using Core.Windows;
using UnityEngine;
using Utils.Attributes;

namespace Core.Conditions.Service
{
    [Serializable]
    [CreateAssetMenu(fileName = "ConditionActionSo", menuName = "Conditions/ConditionActions")]
    public class ConditionActionSo : ScriptableObject
    {
        [SerializeField] private EConditionActionType type;

        [EnumConditionalHide(nameof(type), EConditionActionType.OpenPopup, true)]
        [SerializeField] private EPopupType windowId;
        [EnumConditionalHide(nameof(type), EConditionActionType.PlayMusic, true)]
        [SerializeField] private SoundSource startSound;
        [EnumConditionalHide(nameof(type), EConditionActionType.PlayMusic, true)]
        [SerializeField] private SoundSource stopSound;

        [SerializeField] private bool oneTime;
        
        [SerializeField] private ExpressionDesc expressionDesc;
        
        public ExpressionDesc ExpressionDesc => expressionDesc;
        public EConditionActionType Type => type;
        public EPopupType WindowId => windowId;
        public SoundSource StartSound => startSound;
        public SoundSource StopSound => stopSound;

        public bool OneTime => oneTime;
    }
}