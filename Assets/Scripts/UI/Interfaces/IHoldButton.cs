using System;

namespace UI.Interfaces
{
    public interface IHoldButton
    {
        event Action<float> HoldPerformed;
    }
}