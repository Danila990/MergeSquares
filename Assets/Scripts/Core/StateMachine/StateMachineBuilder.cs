using System;

namespace Core.StateMachine
{
    /// <summary>
    /// Entry point for fluent API for constructing states.
    /// </summary>
    public class StateMachineBuilder<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// Root level state.
        /// </summary>
        private readonly State<TEnum> rootState;

        /// <summary>
        /// Entry point for constructing new state machines.
        /// </summary>
        public StateMachineBuilder()
        {
            rootState = new State<TEnum>();
        }

        /// <summary>
        /// Create a new state of a specified type with a specified name and add it as a
        /// child of the root state.
        /// </summary>
        /// <typeparam name="T">Type of the state to add</typeparam>
        /// <param name="stateName">Name for the new state</param>
        /// <returns>Builder used to configure the new state</returns>
        public IStateBuilder<T, StateMachineBuilder<TEnum>, TEnum> State<T>(TEnum state)
            where T : AbstractState<TEnum>, new()
        {
            return new StateBuilder<T, StateMachineBuilder<TEnum>, TEnum>(this, rootState, state);
        }

        /// <summary>
        /// Create a new state with a specified name and add it as a
        /// child of the root state.
        /// </summary>
        /// <param name="stateName">Name for the new state</param>
        /// <returns>Builder used to configure the new state</returns>
        public IStateBuilder<State<TEnum>, StateMachineBuilder<TEnum>, TEnum> State(TEnum state)
        {
            return new StateBuilder<State<TEnum>, StateMachineBuilder<TEnum>, TEnum>(this, rootState, state);
        }

        /// <summary>
        /// Return the root state once everything has been set up.
        /// </summary>
        public IState<TEnum> Build()
        {
            return rootState;
        }
    }
}