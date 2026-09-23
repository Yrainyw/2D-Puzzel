using Godot;
using Game.Autoload;
using Game.Resources.Building;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;

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

	public List<Vector2I> GetOccupiedCellPositions()
	{
		var result = new List<Vector2I>();
		var gridPosition = GetGridCellPosition();

		for (int x = gridPosition.X; x < gridPosition.X + buildingResource.Dimensions.X; x++)
		{
			for (int y = gridPosition.Y; y < gridPosition.Y + buildingResource.Dimensions.Y; y++)
			{
				result.Add(new Vector2I(x, y));
			}
		}
		return result;
	}

	public void Destroy()
	{
		GameEvents.EmitBuildingDestroyed(this);
		Owner.QueueFree();
	}
}
