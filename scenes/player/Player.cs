using System;
using System.Collections.Generic;
using Godot;
using Sandbox.Classes;
using Sandbox.Globals.Extensions;

namespace Sandbox;

public partial class Player : CharacterBody2D, IStateMachine<Player.State>
{
    #region 相机

    // 让相机成为Player的属性，方便全局设置相机边界等
    [Export] public Camera? Camera { get; set; }

    #endregion

    #region 玩家朝向

    // 初始值必须为Right,与贴图默认方向一致
    private EFaceDirection _lastFrameEFaceDirection = EFaceDirection.Right;

    #region CurrentFaceDirection

    private EFaceDirection _currentEFaceDirection = EFaceDirection.Right;

    [Export]
    public EFaceDirection CurrentEFaceDirection
    {
        get => _currentEFaceDirection;
        set => SetCurrentEFaceDirection(value);
    }

    private async void SetCurrentEFaceDirection(EFaceDirection value)
    {
        await this.EnsureReadyAsync();
        _currentEFaceDirection = value;
    }

    #endregion

    public enum EFaceDirection
    {
        Right,
        Down,
        Left,
        Up,
    }

    private void _resolveFlipH()
    {
        if (IsHFaceDirectionChange())
            FlipH();

        return;

        bool IsHFaceDirectionChange()
        {
            return IsFaceLeft(_lastFrameEFaceDirection) != IsFaceLeft(CurrentEFaceDirection);
        }

        bool IsFaceLeft(EFaceDirection aEFaceDirection)
        {
            return aEFaceDirection == EFaceDirection.Left;
        }

        void FlipH()
        {
            Scale = Scale with { X = -Scale.X };
        }
    }

    #endregion

    #region 生命周期

    private Player()
    {
        _stateMachine = StateMachine<State>.Create(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _resolveFlipH();
        _lastFrameEFaceDirection = CurrentEFaceDirection;
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
        switch (toState)
        {
            case State.Idle:
                _handleTransitionToIdle();
                break;
            case State.Move:
                _handleTransitionToMove();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(toState), toState, null);
        }
    }

    public State GetNextState(State currentState, out bool keepCurrent)
    {
        keepCurrent = false;
        var eMoveDirection = GetEMoveDirection();
        GD.Print($"玩家移动方向: {eMoveDirection}");

        switch (currentState)
        {
            case State.Idle:
                if (eMoveDirection != EMoveDirection.None)
                {
                    _handleTransitionOutIdle();
                    return State.Move;
                }

                break;
            case State.Move:
                if (eMoveDirection == EMoveDirection.None)
                {
                    _handleTransitionOutMove();
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
                _handleIdleTick(delta);
                break;
            case State.Move:
                _handleMoveTick(delta);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
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
        SetFaceDirection();
        HandleMoveAnimation();
        HandleMove();

        return;

        void SetFaceDirection()
        {
            CurrentEFaceDirection = eMoveDirection switch
            {
                EMoveDirection.Right or EMoveDirection.RightDown or EMoveDirection.RightUp => EFaceDirection.Right,
                EMoveDirection.Left or EMoveDirection.LeftDown or EMoveDirection.LeftUp => EFaceDirection.Left,
                EMoveDirection.Down => EFaceDirection.Down,
                EMoveDirection.Up => EFaceDirection.Up,
                _ => throw new ArgumentOutOfRangeException(nameof(eMoveDirection), eMoveDirection, "错误的移动方向")
            };
        }

        void HandleMoveAnimation()
        {
            var animationName = eMoveDirection switch
            {
                EMoveDirection.Right or EMoveDirection.Left => "walk_right",
                EMoveDirection.RightDown or EMoveDirection.LeftDown => "walk_right_down",
                EMoveDirection.Down => "walk_down",
                EMoveDirection.Up => "walk_up",
                EMoveDirection.RightUp or EMoveDirection.LeftUp => "walk_right_up",
                _ => throw new ArgumentOutOfRangeException(nameof(eMoveDirection), eMoveDirection, "错误的移动方向")
            };
            AnimationPlayer.Play(animationName);
        }

        void HandleMove()
        {
            var direction = _moveDirectionMap[eMoveDirection];
            Velocity = MoveSpeed * direction;
            MoveAndSlide();
        }
    }

    #endregion

    #region 状态转移

    private void _handleTransitionToMove()
    {
        WalkSFX.Play();
    }

    private void _handleTransitionOutMove()
    {
        WalkSFX.Stop();
    }

    #endregion

    #endregion

    #region idle状态

    #region 状态转移

    private void _handleTransitionToIdle()
    {
    }

    private void _handleTransitionOutIdle()
    {
    }

    #endregion

    #region idle Tick

    private void _handleIdleTick(double _)
    {
        HandleIdleAnimation();

        return;

        void HandleIdleAnimation()
        {
            var animationName = CurrentEFaceDirection switch
            {
                EFaceDirection.Right or EFaceDirection.Left => "idle_right",
                EFaceDirection.Down => "idle_down",
                EFaceDirection.Up => "idle_up",
                _ => throw new ArgumentOutOfRangeException()
            };
            AnimationPlayer.Play(animationName);
        }
    }

    #endregion

    #endregion

    #endregion

    #region Child

    [ExportGroup("ChildDontChange")]
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; } = null!;

    [ExportSubgroup("SFX")] [Export] public AudioStreamPlayer2D WalkSFX { get; set; } = null!;

    #endregion
}