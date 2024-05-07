using System.Collections.Generic;
using Core.Executor.Commands;

namespace Core.Executor
{
    public class Executor : Sequence
    {
        public static readonly List<Executor> DEBUG_EXECUTORS_LIST = new List<Executor>();
        public readonly string DebugName;
        public bool StartIfNull => startIfNull;

        public Executor(string debugName, bool startIfNull = true)
        {
            DebugName = debugName;
            this.startIfNull = startIfNull;
            DEBUG_EXECUTORS_LIST.Add(this);
        }

        public void Start()
        {
            Ensure();
        }

        public void Dispose()
        {
            DEBUG_EXECUTORS_LIST.Remove(this);
        }

        public override string ToString()
        {
            return DebugName + ":" + DebugReport(Commands.Utils.Delimiter + Commands.Utils.Space);
        }
    }
}