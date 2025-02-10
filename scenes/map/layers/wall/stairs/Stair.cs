using System;
using System.Threading.Tasks;
using Godot;
using Sandbox.Globals;
using Sandbox.Globals.Extensions;

namespace Sandbox.MapNs.LayersNs.WallNs;

public partial class Stair : Node2D
{
    private const int LayersZIndexStep = 10;

    public override void _Ready()
    {
        base._Ready();
        PlayerDetectArea.AreaEntered += _onPlayerDetectAreaAreaEntered;
        PlayerDetectArea.AreaExited += _onPlayerDetectAreaAreaExited;
    }

    private void _onPlayerDetectAreaAreaEntered(Area2D area2D)
    {
        var player = Game.Player;
        if (player == null) return;
        var playerGraphicsArea = player.GraphicsArea;
        if (area2D != playerGraphicsArea) return;

        if (player.ZIndex == ZIndex)
        {
            player.ZIndex += LayersZIndexStep;
        }
    }

    private async void _onPlayerDetectAreaAreaExited(Area2D area2D)
    {
        var player = Game.Player;
        if (player == null) return;
        var playerGraphicsArea = player.GraphicsArea;
        if (area2D != playerGraphicsArea) return;

        // 因为player使用scale进行翻转，所以scale可能为负值，需要对scale取绝对值进行计算
        var playerTop = playerGraphicsArea.GetCollisionChildRect().Position.Y *
                        Math.Abs(playerGraphicsArea.GlobalScale.Y) +
                        playerGraphicsArea.GlobalPosition.Y;

        var playerDetectAreaBottom =
            PlayerDetectArea.GetCollisionChildRect().End.Y *
            Math.Abs(PlayerDetectArea.GlobalScale.Y) +
            PlayerDetectArea.GlobalPosition.Y;

        var isGoDownStair = playerTop > playerDetectAreaBottom;

        if (isGoDownStair)
        {
            player.ZIndex = ZIndex;
        }
    }


    #region Child

    [ExportGroup("ChildDontChange")]
    [Export]
    public Area2D PlayerDetectArea { get; set; } = null!;

    #endregion
}