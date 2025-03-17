using UnityEngine;

namespace FiniteStateMachine
{
    public interface IHasState
    {
        public void At(IState from, IState to, IPredicate condition);
        public void Any(IState to, IPredicate condition);
    }
}