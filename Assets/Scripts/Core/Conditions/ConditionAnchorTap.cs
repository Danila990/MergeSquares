using Core.Anchors;
using Tutorial;

namespace Core.Conditions {
	public class ConditionAnchorTap : ConditionBase {
		private readonly Anchor _anchor;
        private bool _completed;

		public ConditionAnchorTap( Anchor anchor ) {
			_anchor = anchor;
            _anchor.Clicked += OnAnchorTap;
        }

		public override void Dispose() {
            _anchor.Clicked -= OnAnchorTap;
			base.Dispose();
		}

		private void OnAnchorTap() {
			_completed = true;
			MarkChanged();
		}

		public override bool IsTrue => _completed;
	}
}