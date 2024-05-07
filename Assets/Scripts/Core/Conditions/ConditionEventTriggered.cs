namespace Core.Conditions {
	public struct EvScreenTap { }

    public class ConditionEventTriggered : ConditionBase {
        protected bool _completed;

        public override bool IsTrue => _completed;

        public void OnEvent() {
            _completed = true;
            MarkChanged();
        }
    }
    
	public class ConditionEventTriggered<T> : ConditionEventTriggered {
        public void OnEvent( T ev ) {
			_completed = true;
			MarkChanged();
		}
	}
}