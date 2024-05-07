using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.StateMachine
{
    /// <summary>
    /// Non-generic state interface.
    /// </summary>
    public interface IState<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// Parent state, or null if this is the root level state.
        /// </summary>
        IState<TEnum> Parent { get; set; }

        /// <summary>
        /// Change to the state with the specified name.
        /// </summary>
        void ChangeState(TEnum enumState, bool needEnter = true);

        /// <summary>
        /// Push another state above the current one, so that popping it will return to the
        /// current state.
        /// </summary>
        void PushState(TEnum enumState);

        /// <summary>
        /// Exit out of the current state and enter whatever state is below it in the stack.
        /// </summary>
        void PopState();

        /// <summary>
        /// Update this state and its children with a specified delta time.
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Triggered when we enter the state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Triggered when we exit the state.
        /// </summary>
        void Exit();

        /// <summary>
        /// Trigger an event on this state or one of its children.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        void TriggerEvent(string name);

        /// <summary>
        /// Triggered when and event occurs. Executes the event's action if the 
        /// current state is at the top of the stack, otherwise triggers it on 
        /// the next state down.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        /// <param name="eventArgs">Arguments to send to the event</param>
        void TriggerEvent(string name, EventArgs eventArgs);

        /// <summary>
        /// Set log debug info
        /// </summary>
        bool Verbose { get; set; }

        /// <summary>
        /// Set log prefix
        /// </summary>
        string Prefix { get; set; }
    }

    /// <summary>
    /// State with a specified handler type.
    /// </summary>
    public abstract class AbstractState<TEnum> : IState<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// Action called when we enter the state.
        /// </summary>
        private Action onEnter;

        /// <summary>
        /// Action called when the state gets updated.
        /// </summary>
        private Action<float> onUpdate;

        /// <summary>
        /// Action called when we exit the state.
        /// </summary>
        private Action onExit;

        private readonly IList<Condition> conditions = new List<Condition>();

        /// <summary>
        /// Set log debug info
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Set log prefix
        /// </summary>
        public string Prefix { get; set; } = "";
        
        /// <summary>
        /// Parent state, or null if this is the root level state.
        /// </summary>
        public IState<TEnum> Parent { get; set; }

        /// <summary>
        /// Stack of active child states.
        /// </summary>
        private readonly Stack<IState<TEnum>> activeChildren = new Stack<IState<TEnum>>();

        /// <summary>
        /// Dictionary of all children (active and inactive), and their names.
        /// </summary>
        private readonly IDictionary<TEnum, IState<TEnum>> children = new Dictionary<TEnum, IState<TEnum>>();

        /// <summary>
        /// Dictionary of all actions associated with this state.
        /// </summary>
        private readonly IDictionary<string, Action<EventArgs>> events = new Dictionary<string, Action<EventArgs>>();

        /// <summary>
        /// Pops the current state from the stack and pushes the specified one on.
        /// </summary>
        public void ChangeState(TEnum enumState, bool needEnter = true)
        {
            // Try to find the specified state.
            IState<TEnum> newState;
            if (!children.TryGetValue(enumState, out newState))
            {
                throw new ApplicationException("Tried to change to state \"" + enumState +
                                               "\", but it is not in the list of children.");
            }

            // Exit and pop the current state
            if (activeChildren.Count > 0)
            {
                activeChildren.Pop().Exit();
            }

            if (Verbose)
            {
                Debug.Log($"[{Prefix}][AbstractState][ChangeState] Change state to: {enumState}");
            }
            
            // Activate the new state
            activeChildren.Push(newState);
            if (needEnter)
            {
                if (Verbose)
                {
                    Debug.Log($"[{Prefix}][AbstractState][ChangeState] Enter to state: {enumState}");
                }
                newState.Enter();
            }
        }

        /// <summary>
        /// Push another state from the existing dictionary of children to the top of the state stack.
        /// </summary>
        public void PushState(TEnum enumState)
        {
            // Find the new state and add it
            IState<TEnum> newState;
            if (!children.TryGetValue(enumState, out newState))
            {
                throw new ApplicationException("Tried to change to state \"" + enumState +
                                               "\", but it is not in the list of children.");
            }

            activeChildren.Push(newState);
            newState.Enter();
        }

        /// <summary>
        /// Remove the current state from the active state stack and activate the state immediately beneath it.
        /// </summary>
        public void PopState()
        {
            // Exit and pop the current state
            if (activeChildren.Count > 0)
            {
                activeChildren.Pop().Exit();
            }
            else
            {
                throw new ApplicationException("PopState called on state with no active children to pop.");
            }
        }

        /// <summary>
        /// Update this state and its children with a specified delta time.
        /// </summary>
        public void Update(float deltaTime)
        {
            // Only update the child at the end of the tree
            if (activeChildren.Count > 0)
            {
                activeChildren.Peek().Update(deltaTime);
                return;
            }

            if (onUpdate != null)
            {
                onUpdate(deltaTime);
            }

            // Update conditions
            for (var i = 0; i < conditions.Count; i++)
            {
                if (conditions[i].Predicate())
                {
                    if (Verbose)
                    {
                        Debug.Log($"[{Prefix}][AbstractState][Update] Condition passed");
                    }
                    conditions[i].Action();
                }
            }
        }

        /// <summary>
        /// Create a new state as a child of the current state.
        /// </summary>
        public void AddChild(IState<TEnum> newState, TEnum enumState)
        {
            try
            {
                children.Add(enumState, newState);
                newState.Parent = this;
            }
            catch (ArgumentException)
            {
                throw new ApplicationException("State with name \"" + enumState +
                                               "\" already exists in list of children.");
            }
        }

        /// <summary>
        /// Data structure for associating a condition with an action.
        /// </summary>
        private struct Condition
        {
            public Func<bool> Predicate;
            public Action Action;
        }

        /// <summary>
        /// Set an action to be called when the state is updated an a specified 
        /// predicate is true.
        /// </summary>
        public void SetCondition(Func<bool> predicate, Action action)
        {
            conditions.Add(new Condition()
            {
                Predicate = predicate,
                Action = action
            });
        }

        /// <summary>
        /// Action triggered on entering the state.
        /// </summary>
        public void SetEnterAction(Action onEnter)
        {
            this.onEnter = onEnter;
        }

        /// <summary>
        /// Action triggered on exiting the state.
        /// </summary>
        public void SetExitAction(Action onExit)
        {
            this.onExit = onExit;
        }

        /// <summary>
        /// Action which passes the current state object and the delta time since the 
        /// last update to a function.
        /// </summary>
        public void SetUpdateAction(Action<float> onUpdate)
        {
            this.onUpdate = onUpdate;
        }

        /// <summary>
        /// Sets an action to be associated with an identifier that can later be used
        /// to trigger it.
        /// Convenience method that uses default event args intended for events that 
        /// don't need any arguments.
        /// </summary>
        public void SetEvent(string identifier, Action<EventArgs> eventTriggeredAction)
        {
            SetEvent<EventArgs>(identifier, eventTriggeredAction);
        }

        /// <summary>
        /// Sets an action to be associated with an identifier that can later be used
        /// to trigger it.
        /// </summary>
        public void SetEvent<TEvent>(string identifier, Action<TEvent> eventTriggeredAction)
            where TEvent : EventArgs
        {
            events.Add(identifier, args => eventTriggeredAction(CheckEventArgs<TEvent>(identifier, args)));
        }

        /// <summary>
        /// Cast the specified EventArgs to a specified type, throwing a descriptive exception if this fails.
        /// </summary>
        private static TEvent CheckEventArgs<TEvent>(string identifier, EventArgs args)
            where TEvent : EventArgs
        {
            try
            {
                return (TEvent) args;
            }
            catch (InvalidCastException ex)
            {
                throw new ApplicationException("Could not invoke event \"" + identifier + "\" with argument of type " +
                                               args.GetType().Name + ". Expected " + typeof(TEvent).Name, ex);
            }
        }

        /// <summary>
        /// Triggered when we enter the state.
        /// </summary>
        public void Enter()
        {
            if (onEnter != null)
            {
                onEnter();
            }
        }

        /// <summary>
        /// Triggered when we exit the state.
        /// </summary>
        public void Exit()
        {
            if (onExit != null)
            {
                if (Verbose)
                {
                    Debug.Log($"[{Prefix}][AbstractState][ChangeState] Exit from state");
                }
                onExit();
            }

            while (activeChildren.Count > 0)
            {
                activeChildren.Pop().Exit();
            }
        }

        /// <summary>
        /// Triggered when and event occurs. Executes the event's action if the 
        /// current state is at the top of the stack, otherwise triggers it on 
        /// the next state down.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        public void TriggerEvent(string name)
        {
            TriggerEvent(name, EventArgs.Empty);
        }

        /// <summary>
        /// Triggered when and event occurs. Executes the event's action if the 
        /// current state is at the top of the stack, otherwise triggers it on 
        /// the next state down.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        /// <param name="eventArgs">Arguments to send to the event</param>
        public void TriggerEvent(string name, EventArgs eventArgs)
        {
            // Only update the child at the end of the tree
            if (activeChildren.Count > 0)
            {
                activeChildren.Peek().TriggerEvent(name, eventArgs);
                return;
            }

            Action<EventArgs> myEvent;
            if (events.TryGetValue(name, out myEvent))
            {
                if (Verbose)
                {
                    Debug.Log($"[{Prefix}][AbstractState][TriggerEvent] event name: {name}");
                }
                myEvent(eventArgs);
            }
        }
    }

    /// <summary>
    /// State with no extra functionality used for root of state hierarchy.
    /// </summary>
    public class State<TEnum> : AbstractState<TEnum> where TEnum : Enum
    {
    }
}