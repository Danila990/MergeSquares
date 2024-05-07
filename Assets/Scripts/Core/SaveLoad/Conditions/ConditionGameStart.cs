using Core.Conditions;

namespace Core.SaveLoad.Conditions
{
    public class ConditionGameStart : ConditionBase
    {
        private bool _isTermsOfUse;
        private bool _completed;

        public override bool Updatable => true;

        public ConditionGameStart(bool isTermsOfUse, ConditionBuilderContext context)
        {
            _isTermsOfUse = isTermsOfUse;
            _completed = context.saveService.IsGameNew && !context.saveService.GetAgreementState(isTermsOfUse);
        }

        protected override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            if(_completed)
            {
                MarkChanged();
            }
        }

        public override bool IsTrue => _completed;
    }
}