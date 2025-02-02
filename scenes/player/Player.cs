using System;
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
        Idle,
        Move,
    }

    #endregion

    #region IStateMachine<State> Members

    public void TransitionState(State fromState, State toState)
    {
        GD.Print($"{Name}: {fromState} => {toState}");
    }

    public State GetNextState(State currentState, out bool keepCurrent)
    {
        keepCurrent = false;
        var moveDirection = GetMoveDirection();

        switch (currentState)
        {
            case State.Idle:
                if (moveDirection.Length() != 0)
                {
                    return State.Move;
                }

                break;
            case State.Move:
                if (moveDirection.Length() == 0)
                {
                    return State.Idle;
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }

        keepCurrent = true;
        return currentState;

        Vector2 GetMoveDirection()
        {
            return Input.GetVector("move_left", "move_right", "move_up", "move_down");
        }
    }

    public void TickPhysics(State currentState, double delta)
    {
    }

    #endregion

    #endregion
}