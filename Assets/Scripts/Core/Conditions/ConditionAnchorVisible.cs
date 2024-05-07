using Core.Anchors;
using Tutorial;

namespace Core.Conditions
{
    public class ConditionAnchorVisible : ConditionBase
    {
        private readonly Anchor _anchor;
        private bool _enabled;

        public ConditionAnchorVisible(Anchor anchor)
        {
            _anchor = anchor;
            _anchor.StateChanged += OnAnchorChanged;
        }

        public override void Dispose()
        {
            _anchor.StateChanged -= OnAnchorChanged;
            base.Dispose();
        }

        private void OnAnchorChanged(bool enabled)
        {
            _enabled = enabled;
            MarkChanged();
        }

        public override bool IsTrue => _enabled;
    }
}