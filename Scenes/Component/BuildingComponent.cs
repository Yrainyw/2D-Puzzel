using Godot;
using Game.Autoload;
using Game.Resources.Building;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Linq;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{
	[Export(PropertyHint.File, "*.tres")]
	private string buildingResourcePath;

	public buildingResource buildingResource { get; private set; }
	private HashSet<Vector2I> occupiedTiles = new();

	public override void _Ready()
	{
		if (buildingResourcePath != null)
		{
			buildingResource = GD.Load<buildingResource>(buildingResourcePath);
		}
		
		AddToGroup(nameof(BuildingComponent));
		Callable.From(Initialize).CallDeferred();
	}

	public Vector2I GetGridCellPosition()
	{
		var gridPosition = GlobalPosition / 64;

		gridPosition = gridPosition.Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}

	public HashSet<Vector2I> GetOccupiedCellPositions()
	{
		return occupiedTiles.ToHashSet();
	}

	public bool IsTileInBuildingArea(Vector2I tilePosition)
	{
		return occupiedTiles.Contains(tilePosition);
	}

	public void Destroy()
	{
		GameEvents.EmitBuildingDestroyed(this);
		Owner.QueueFree();
	}

	private void CalculateOccupiedCellPositions()
	{
		var gridPosition = GetGridCellPosition();
		for (int x = gridPosition.X; x < gridPosition.X + buildingResource.Dimensions.X; x++)
		{
			for (int y = gridPosition.Y; y < gridPosition.Y + buildingResource.Dimensions.Y; y++)
			{
				occupiedTiles.Add(new Vector2I(x, y));
			}
		}
	}

	private void Initialize()
	{
		CalculateOccupiedCellPositions();
		GameEvents.EmitBuildingPlaced(this);
	}
}
