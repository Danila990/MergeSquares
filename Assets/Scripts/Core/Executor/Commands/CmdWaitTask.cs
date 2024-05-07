using System;
using System.Threading.Tasks;

namespace Core.Executor.Commands
{
    public class CmdWaitTask : ICommand
    {
        private Action<ICommand> _onFinish;
        private readonly Func<Task> _action;

        public CmdWaitTask(Func<Task> action)
        {
            _action = action;
        }

        public void Start(Action<ICommand> onFinish)
        {
            _onFinish = onFinish;
            var task = _action.Invoke();
            var awaiter = task.GetAwaiter();
            awaiter.OnCompleted(OnCompleted);
        }

        public void Update(float dt) {}

        protected virtual void OnCompleted()
        {
            _onFinish(this);
        }
    }

    public class CmdWaitTask1<T> : ICommand
    {
        private Action<ICommand> _onFinish;
        private readonly Func<T, Task> _action;
        private readonly T _arg;

        public CmdWaitTask1(Func<T, Task> action, T arg)
        {
            _action = action;
            _arg = arg;
        }

        public void Start(Action<ICommand> onFinish)
        {
            _onFinish = onFinish;
            var task = _action.Invoke(_arg);
            var awaiter = task.GetAwaiter();
            awaiter.OnCompleted(OnCompleted);
        }
        
        public void Update(float dt) {}

        protected virtual void OnCompleted()
        {
            _onFinish(this);
        }
    }

    public class CmdWaitTask2<T1, T2> : ICommand
    {
        private Action<ICommand> _onFinish;
        private readonly Func<T1, T2, Task> _action;
        private readonly T1 _arg1;
        private readonly T2 _arg2;

        public CmdWaitTask2(Func<T1, T2, Task> action, T1 arg1, T2 arg2)
        {
            _action = action;
            _arg1 = arg1;
            _arg2 = arg2;
        }

        public void Start(Action<ICommand> onFinish)
        {
            _onFinish = onFinish;
            var task = _action.Invoke(_arg1, _arg2);
            var awaiter = task.GetAwaiter();
            awaiter.OnCompleted(OnCompleted);
        }
        
        public void Update(float dt) {}

        protected virtual void OnCompleted()
        {
            _onFinish(this);
        }
    }
}