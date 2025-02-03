using Godot;
using Sandbox.Globals;

namespace Sandbox.WorldNs;

public partial class World : Node2D
{
    public override void _Ready()
    {
        base._Ready();
        Game.Player = Player;
        Game.OnWorldReadyEvent();
    }

    #region Child

    [ExportGroup("ChildDontChange")]
    [Export]
    public Player Player { get; set; } = null!;

    #endregion
}