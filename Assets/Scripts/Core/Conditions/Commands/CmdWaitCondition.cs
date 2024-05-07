using System;
using Core.Executor.Commands;
using UnityEngine;
using Zenject;

namespace Core.Conditions.Commands {
	public class CmdWaitCondition : ICommand {
        private readonly string _expression;
        private readonly ConditionDesc[] _conditionDescs;
        private ConditionBase _conditionOrNull;
        private Action<ICommand> _onFinish;
        private readonly ExpressionDesc _desc;
        private ConditionBuilder _conditionBuilder;
        
        public CmdWaitCondition( ExpressionDesc desc, ConditionBuilder conditionBuilder ) {
            _desc = desc;
            _conditionBuilder = conditionBuilder;
        }
        
        public CmdWaitCondition( ConditionBase condition )
        {
            _conditionOrNull = condition;
            if ( _conditionOrNull != null ) {
                condition.Init(OnConditionChanged);
                OnConditionChanged( _conditionOrNull.IsTrue );
            }
        }

        public void Start( Action<ICommand> onFinish ) {
            _onFinish = onFinish;
            if(_conditionOrNull == null)
            {
                // _conditionOrNull = _conditionBuilder.CreateCondition(_desc, OnConditionChanged);
                
                _conditionOrNull = _conditionBuilder.CreateCondition(_expression, _conditionDescs, null);

                if (_conditionOrNull != null)
                {
                    OnConditionChanged(_conditionOrNull.IsTrue);
                }
            }
        }
        
        public void Update(float dt) {}

        private void OnConditionChanged( bool completed ) {
            if ( completed ) {
                Dispose();
                _onFinish?.Invoke( this );
            }
        }

        private void Dispose() {
            if ( _conditionOrNull != null ) {
                _conditionOrNull.Dispose();
                _conditionOrNull = null;
            }
        }
	}
}