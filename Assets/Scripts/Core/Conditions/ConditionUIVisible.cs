namespace Core.Conditions {
	public class ConditionUIVisible : ConditionBase {
		// private readonly UIID id;
		//
		// public ConditionUIVisible( UIID id ) {
		// 	this.id = id;
		// 	App.The.Events.Subscribe<EvUIWidgetStateChanged>( OnUIWidgetChanged );
		// }
		//
		// public override void Dispose() {
		// 	App.The.Events.Unsubscribe<EvUIWidgetStateChanged>( OnUIWidgetChanged );
		// 	base.Dispose();
		// }
		//
		// public override bool IsTrue => App.The.Events.SyncBroadcast( new SyncUIState( id ) ).isVisible;
		//
		// private void OnUIWidgetChanged( EvUIWidgetStateChanged ev ) {
		// 	if ( ev.id == id ) {
		// 		MarkChanged();
		// 	}
		// }
        public override bool IsTrue => false;
    }
}