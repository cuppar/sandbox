using Godot;
using Sandbox.Globals;

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

    private void _onPlayerDetectAreaAreaExited(Area2D area2D)
    {
        var player = Game.Player;
        if (player == null) return;
        var playerGraphicsArea = player.GraphicsArea;
        if (area2D != playerGraphicsArea) return;


        // todo 抽象成area2d的扩展方法
        var playerTop = float.MaxValue;
        foreach (var child in playerGraphicsArea.GetChildren())
        {
            if (child is not CollisionShape2D collisionShape2D) continue;
            var top = (collisionShape2D.Shape.GetRect().Position * collisionShape2D.GlobalScale +
                       collisionShape2D.GlobalPosition).Y;
            if (top < playerTop)
                playerTop = top;
        }

        var playerDetectAreaBottom = float.MinValue;
        foreach (var child in PlayerDetectArea.GetChildren())
        {
            if (child is not CollisionShape2D collisionShape2D) continue;
            var bottom = (collisionShape2D.Shape.GetRect().End * collisionShape2D.GlobalScale +
                          collisionShape2D.GlobalPosition).Y;
            if (bottom > playerDetectAreaBottom)
                playerDetectAreaBottom = bottom;
        }

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