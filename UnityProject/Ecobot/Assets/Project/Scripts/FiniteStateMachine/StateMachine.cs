using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace FiniteStateMachine
{
    public class StateMachine
    {
        private StateNode _current;
        private Dictionary<Type, StateNode> _nodes = new();
        private HashSet<ITransition> _anyTransitions = new();

        public ReactiveProperty<IState> CurrentState { get; private set; } = new ();
        public IState PreviousState { get; private set; }

        public void Update()
        {
            var transition = GetTransition();
            if (transition != null)
            {
                ChangeState(transition.To);
            }

            CurrentState.Value?.Update();
        }

        public void FixedUpdate()
        {
            CurrentState.Value?.FixedUpdate();
        }

        public void SetState(IState state)
        {
            if (state == CurrentState.Value) return;

            PreviousState = CurrentState.Value;
            _current = _nodes[state.GetType()];
            CurrentState.Value = _current.State;  

            CurrentState.Value?.OnEnter();
        }

        private void ChangeState(IState state)
        {
            if (state == CurrentState.Value) return;

            PreviousState = CurrentState.Value;
            CurrentState.Value?.OnExit();

            _current = _nodes[state.GetType()];
            CurrentState.Value = _current.State;
            CurrentState.Value?.OnEnter();
        }

        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }

        public void AddAnyTransition(IState to, IPredicate condition)
        {
            _anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
        }

        private StateNode GetOrAddNode(IState state)
        {
            var node = _nodes.GetValueOrDefault(state.GetType());

            if (node == null)
            {
                node = new StateNode(state);
                _nodes.Add(state.GetType(), node);
            }

            return node;
        }

        private ITransition GetTransition()
        {
            foreach (var transition in _anyTransitions)
            {
                if (transition.Condition.Evaluate()) return transition;
            }

            foreach (var transition in _current.Transitions)
            {
                if (transition.Condition.Evaluate()) return transition;
            }

            return null;
        }
        
        public void ReturnToPreviousMode()
        {
            if (PreviousState != null)
            {
                SetState(PreviousState);
            }
            else
            {
                Debug.LogWarning("No previous state to return to.");
            }
        }

        private class StateNode
        {
            public IState State { get; }
            public HashSet<ITransition> Transitions { get; }

            public StateNode(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            public void AddTransition(IState to, IPredicate condition)
            {
                Transitions.Add(new Transition(to, condition));
            }
        }
    }
}