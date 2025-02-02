using Godot;
using Sandbox.Classes;

namespace Sandbox;

public partial class Player : CharacterBody2D, IStateMachine<Player.State>
{
    #region 生命周期

    private Player()
    {
        _stateMachine = StateMachine<State>.Create(this);
    }

    #endregion

    #region 状态机

    private StateMachine<State> _stateMachine;

    #region State enum

    public enum State
    {
        Idle
    }

    #endregion

    #region IStateMachine<State> Members

    public void TransitionState(State fromState, State toState)
    {
    }

    public State GetNextState(State currentState, out bool keepCurrent)
    {
        keepCurrent = true;
        return currentState;
    }

    public void TickPhysics(State currentState, double delta)
    {
    }

    #endregion

    #endregion
}