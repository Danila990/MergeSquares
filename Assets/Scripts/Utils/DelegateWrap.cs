using System;

namespace Utils
{
    public class DelegateWrap<T> where T : Delegate
    {
        public T Delegate;
    }
}