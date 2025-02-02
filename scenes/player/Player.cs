using System;
using System.Collections.Generic;
using Godot;
using Sandbox.Classes;
using Sandbox.Globals.Extensions;

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
        var eMoveDirection = GetEMoveDirection();
        GD.Print($"{eMoveDirection}");

        switch (currentState)
        {
            case State.Idle:
                if (eMoveDirection != EMoveDirection.None)
                {
                    return State.Move;
                }

                break;
            case State.Move:
                if (eMoveDirection == EMoveDirection.None)
                {
                    return State.Idle;
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }

        keepCurrent = true;
        return currentState;
    }

    public void TickPhysics(State currentState, double delta)
    {
        switch (currentState)
        {
            case State.Idle:
                break;
            case State.Move:
                _handleMoveTick(delta);
                break;
        }
    }

    #endregion

    #region 移动状态

    #region MoveSpeed

    private float _moveSpeed = 300;

    [Export]
    public float MoveSpeed
    {
        get => _moveSpeed;
        set => SetMoveSpeed(value);
    }

    private async void SetMoveSpeed(float value)
    {
        await this.EnsureReadyAsync();
        _moveSpeed = value;
    }

    #endregion


    #region 移动方向相关

    private enum EMoveDirection
    {
        Right,
        RightDown,
        Down,
        LeftDown,
        Left,
        LeftUp,
        Up,
        RightUp,
        None,
    }

    private readonly Dictionary<EMoveDirection, Vector2> _moveDirectionMap = new()
    {
        { EMoveDirection.None, Vector2.Zero },
        { EMoveDirection.Right, Vector2.Right },
        { EMoveDirection.RightDown, new Vector2(1, 1).Normalized() },
        { EMoveDirection.Down, Vector2.Down },
        { EMoveDirection.LeftDown, new Vector2(-1, 1).Normalized() },
        { EMoveDirection.Left, Vector2.Left },
        { EMoveDirection.LeftUp, new Vector2(-1, -1).Normalized() },
        { EMoveDirection.Up, Vector2.Up },
        { EMoveDirection.RightUp, new Vector2(1, -1).Normalized() },
    };

    private static EMoveDirection GetEMoveDirection()
    {
        var moveDirectionInput = GetMoveDirectionInput();
        if (moveDirectionInput.Length() == 0)
            return EMoveDirection.None;

        var angle = moveDirectionInput.Angle();
        const float pi = (float)Math.PI;

        return angle switch
        {
            >= -1f / 8 * pi and < 1f / 8 * pi => EMoveDirection.Right,
            >= 1f / 8 * pi and < 3f / 8 * pi => EMoveDirection.RightDown,
            >= 3f / 8 * pi and < 5f / 8 * pi => EMoveDirection.Down,
            >= 5f / 8 * pi and < 7f / 8 * pi => EMoveDirection.LeftDown,
            >= 7f / 8 * pi or < -7f / 8 * pi => EMoveDirection.Left,
            >= -7f / 8 * pi and < -5f / 8 * pi => EMoveDirection.LeftUp,
            >= -5f / 8 * pi and < -3f / 8 * pi => EMoveDirection.Up,
            >= -3f / 8 * pi and < -1f / 8 * pi => EMoveDirection.RightUp,
            _ => EMoveDirection.None,
        };

        Vector2 GetMoveDirectionInput()
        {
            return Input.GetVector("move_left", "move_right", "move_up", "move_down");
        }
    }

    #endregion

    #region 移动Tick

    private void _handleMoveTick(double _)
    {
        var eMoveDirection = GetEMoveDirection();
        _handleMoveAnimation(eMoveDirection);
        var direction = _moveDirectionMap[eMoveDirection];
        Velocity = MoveSpeed * direction;
        MoveAndSlide();
    }

    private void _handleMoveAnimation(EMoveDirection eMoveDirection)
    {
        var animationName = eMoveDirection switch
        {
            EMoveDirection.Right or EMoveDirection.Left => "walk_right",
            EMoveDirection.RightDown or EMoveDirection.LeftDown => "walk_right_down",
            EMoveDirection.Down => "walk_down",
            EMoveDirection.Up => "walk_up",
            EMoveDirection.RightUp or EMoveDirection.LeftUp => "walk_right_up",
            _ => throw new ArgumentOutOfRangeException(nameof(eMoveDirection), "错误的移动方向")
        };
        AnimationPlayer.Play(animationName);
        Graphics.Scale = Graphics.Scale with { X = Math.Abs(Graphics.Scale.X) * (_isFaceLeft() ? -1 : 1) };
    }

    private bool _isFaceLeft()
    {
        return _moveDirectionMap[GetEMoveDirection()].X < 0;
    }

    #endregion

    #endregion

    #endregion

    #region Child

    [ExportGroup("ChildDontChange")]
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; } = null!;

    [Export] public Node2D Graphics { get; set; } = null!;

    #endregion
}