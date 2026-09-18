using Godot;
using Game.Autoload;
using Game.Resources.Building;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{
	[Export(PropertyHint.File, "*.tres")]
	public string buildingResourcePath;

	public buildingResource buildingResource { get; private set; }

	public override void _Ready()
	{
		if (buildingResourcePath != null)
		{
			buildingResource = GD.Load<buildingResource>(buildingResourcePath);
		}
		
		AddToGroup(nameof(BuildingComponent));
		Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();
	}

	public Vector2I GetGridCellPosition()
	{
		var gridPosition = GlobalPosition / 64;

		gridPosition = gridPosition.Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}
}
