namespace Core.Conditions
{
    public class ConditionAppStart : ConditionBase
    {
        private ConditionBuilderContext _context;
        private bool _completed;

        public override bool Updatable => true;

        public ConditionAppStart(ConditionBuilderContext context)
        {
            _context = context;
        }

        protected override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            if(_context.saveService.IsAppStarted && !_completed)
            {
                _completed = true;
                MarkChanged();
            }
        }

        public override bool IsTrue => _completed;
    }
}