using Godot;
using Sandbox.Globals;

namespace Sandbox.MapNs;

public partial class Map : Node2D
{
    #region 生命周期

    public override void _Ready()
    {
        base._Ready();
        SetCameraLimit();
    }

    #endregion

    #region 相机边界

    private void SetCameraLimit()
    {
        Game.WorldReadyEvent += () =>
        {
            var camera = Game.Player?.Camera;
            if (camera == null) return;
            camera.RightLimit = RightLimit;
            camera.BottomLimit = BottomLimit;
            camera.LeftLimit = LeftLimit;
            camera.TopLimit = TopLimit;
        };
    }

    #endregion

    #region Child

    [ExportGroup("ChildDontChange")]
    [ExportSubgroup("Limits")]
    [Export]
    public Marker2D RightLimit { get; set; } = null!;

    [Export] public Marker2D BottomLimit { get; set; } = null!;
    [Export] public Marker2D LeftLimit { get; set; } = null!;
    [Export] public Marker2D TopLimit { get; set; } = null!;

    #endregion
}