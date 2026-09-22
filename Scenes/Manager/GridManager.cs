using Godot;
using System;
using System.Collections.Generic;
using Game.Component;
using System.Linq;
using Game.Autoload;

namespace Game.Manager;

public partial class GridManager : Node
{
	private const string IS_BUILDABLE = "is_buildable";
	private const string IS_WOOD = "is_wood";

	[Signal]
	public delegate void ResourceTileUpdatedEventHandler(int collectedTiles);

	private HashSet<Vector2I> validBuildableTiles = new();
	private HashSet<Vector2I> collectedResourceTiles = new();
	private HashSet<Vector2I> occupiedTiles = new();

	[Export]
	private TileMapLayer highlightTilemapLayer;

	[Export]
	private TileMapLayer baseTerrainTilemapLayer;

	private List<TileMapLayer> allTilemapLayers;

	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
		GameEvents.Instance.BuildingDestroyed += OnBuildingDestroyed;
		allTilemapLayers = GetAllTilemapLayers(baseTerrainTilemapLayer);
	}

	public bool TileHasCustomData(Vector2I tilePosition, string dataName)
	{
		foreach (var layer in allTilemapLayers)
		{
			var customData = layer.GetCellTileData(tilePosition);

			if (customData == null) continue;
			return (bool)customData.GetCustomData(dataName);
		}
		return false;
	}

	public bool IsTilePositionBuildable(Vector2I tilePosition)
	{
		return validBuildableTiles.Contains(tilePosition);
	}

	public void HighlightBuildableTiles()
	{
		foreach (var tilePosition in validBuildableTiles)
		{
			highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HighlightExpandedBuildableTiles(Vector2I rootCell, int radius)
	{
		var validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
		var expandedTiles = validTiles.Except(validBuildableTiles).Except(occupiedTiles);
		var atlasCoords = new Vector2I(1, 0);

		foreach (var tilePosition in expandedTiles)
		{
			highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void HighlightResourceTiles(Vector2I rootCell, int radius)
	{
		var resourceTiles = GetResourceTilesInRadius(rootCell, radius);
		var atlasCoords = new Vector2I(1, 0);

		foreach (var tilePosition in resourceTiles)
		{
			highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void ClearHighlightedTiles()
	{
		highlightTilemapLayer.Clear();
	}

	public Vector2I GetMouseGridCellPosition()
	{
		var mousePosition = highlightTilemapLayer.GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;

		gridPosition = gridPosition.Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}

	private List<TileMapLayer> GetAllTilemapLayers(TileMapLayer rootTileMapLayer)
	{
		var result = new List<TileMapLayer>();
		var children = rootTileMapLayer.GetChildren();

		children.Reverse();

		foreach (var child in children){
			if (child is TileMapLayer childLayer)
			{
				result.AddRange(GetAllTilemapLayers(childLayer));
			}
		}

		result.Add(rootTileMapLayer);
		return result;
	}

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		occupiedTiles.Add(buildingComponent.GetGridCellPosition());
		
		var rootCell = buildingComponent.GetGridCellPosition();
		var validTiles = GetValidTilesInRadius(rootCell, buildingComponent.buildingResource.BuildableRadius);

		validBuildableTiles.UnionWith(validTiles);
		validBuildableTiles.ExceptWith(occupiedTiles);
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var resourceTiles = GetResourceTilesInRadius(rootCell, buildingComponent.buildingResource.ResourceRadius);
		var oldResourceTilesCount = collectedResourceTiles.Count;

		collectedResourceTiles.UnionWith(resourceTiles);

		if (oldResourceTilesCount != collectedResourceTiles.Count)
		{
			EmitSignal(SignalName.ResourceTileUpdated, collectedResourceTiles.Count);
		}
	}

	private void RecalculateGrid(BuildingComponent excludeBuildingComponent)
	{
		occupiedTiles.Clear();
		validBuildableTiles.Clear();
		collectedResourceTiles.Clear();

		var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
			.Where((buildingComponent) => buildingComponent != excludeBuildingComponent);

		foreach (var buildingComponent in buildingComponents)
		{
			UpdateValidBuildableTiles(buildingComponent);
			UpdateCollectedResourceTiles(buildingComponent);
		}
		
		EmitSignal(SignalName.ResourceTileUpdated, collectedResourceTiles.Count);
	}

	private List<Vector2I> GetTilesInRadius(Vector2I rootCell, int radius, Func<Vector2I, bool> filterFn)
	{
		var result = new List<Vector2I>();

		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var tilePosition = new Vector2I(x, y);
				if (!filterFn(tilePosition))
				{
					continue;
				}
				result.Add(tilePosition);
			}
		}
		return result;
	}

	private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(rootCell,radius, (tilePosition) =>
		{
			return TileHasCustomData(tilePosition, IS_BUILDABLE);
		});
	}

	private List<Vector2I> GetResourceTilesInRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(rootCell,radius, (tilePosition) =>
		{
			return TileHasCustomData(tilePosition, IS_WOOD);
		});
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateValidBuildableTiles(buildingComponent);
		UpdateCollectedResourceTiles(buildingComponent);
	}

	private void OnBuildingDestroyed(BuildingComponent buildingComponent)
	{
		RecalculateGrid(buildingComponent);
	}
}