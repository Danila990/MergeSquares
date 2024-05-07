using System;
using System.Collections.Generic;

namespace Tutorial.Models
{
    [Serializable]
    public abstract class TutorialDescBase<T>
    {
        public string name;
        public bool lockSaves = true;
        public bool enabled = true;
        public bool forceStart;
        public virtual IReadOnlyList<T> Steps => new List<T>();
    }
}