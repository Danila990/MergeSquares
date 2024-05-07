using Game.UI;

namespace UI
{
    public class HoldButton : HoldButtonBase
    {
        public void StartHolding()
        {
            ToggleHolding(true);
        }
        
        public void FinishHolding()
        {
            ToggleHolding(false);
        }
    }
}