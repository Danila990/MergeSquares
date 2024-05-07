using Core.Conditions;
using UnityEngine;

namespace Core.SaveLoad.Conditions
{
    public enum ESaveLoadType
    {
        GameStart,
    }

    [CreateAssetMenu(fileName = "SaveLoadDesc", menuName = "Conditions/SaveLoadDesc")]
    public class SaveLoadDesc : ConditionDescExtension
    {
        [SerializeField] private ESaveLoadType type;
        [SerializeField] private bool isTermsOfUse;
        
        public override ConditionBase CreateCondition(ConditionBuilderContext context)
        {
            ConditionBase res = new ConditionTrue();
            switch (type)
            {
                case ESaveLoadType.GameStart:
                    res = new ConditionGameStart(isTermsOfUse, context);
                    break;
            }

            return res;
        }
    }
}