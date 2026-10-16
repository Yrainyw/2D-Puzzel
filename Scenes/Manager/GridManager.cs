using Godot;
using System;
using System.Collections.Generic;
using Game.Component;
using System.Linq;
using Game.Autoload;
using Game.Level.Util;

namespace Game.Manager;

public partial class GridManager : Node
{
	private const string IS_BUILDABLE = "is_buildable";
	private const string IS_WOOD = "is_wood";
	private const string IS_IGNORED = "is_ignored";

	[Signal]
	public delegate void ResourceTilesUpdatedEventHandler(int collectedTiles);

	[Signal]
	public delegate void GridStateUpdatedEventHandler();

	private HashSet<Vector2I> validBuildableTiles = new();
	private HashSet<Vector2I> validAttackTiles = new();
	private HashSet<Vector2I> allTilesInBuildingRadius = new();
	private HashSet<Vector2I> collectedResourceTiles = new();
	private HashSet<Vector2I> occupiedTiles = new();
	private HashSet<Vector2I> goblinOccupiedTiles = new();

	[Export] private TileMapLayer highlightTilemapLayer;
	[Export] private TileMapLayer baseTerrainTilemapLayer;

	private List<TileMapLayer> allTilemapLayers = new();
	private Dictionary<TileMapLayer, ElevationLayer> tileMapLayerToElevationLayer = new();

	public override void _Ready()
	{
		GameEvents.Instance.Connect(GameEvents.SignalName.BuildingPlaced, Callable.From<BuildingComponent>(OnBuildingPlaced));
		GameEvents.Instance.Connect(GameEvents.SignalName.BuildingDestroyed, Callable.From<BuildingComponent>(OnBuildingDestroyed));
		allTilemapLayers = GetAllTilemapLayers(baseTerrainTilemapLayer);
		MapTileMapLayersToElevationLayers();	
	}

	public (TileMapLayer, bool) GetTileCustomData (Vector2I tilePosition, string dataName)
	{
		foreach (var layer in allTilemapLayers)
		{
			var customData = layer.GetCellTileData(tilePosition);

			if (customData == null || (bool)customData.GetCustomData(IS_IGNORED)) continue;
			return (layer, (bool)customData.GetCustomData(dataName));
		}
		return (null, false);
	}

	public bool IsTilePositionInAnyBuildingRadius(Vector2I tilePosition)
	{
		return allTilesInBuildingRadius.Contains(tilePosition);
	}

	public bool IsTileAreaBuildable(Rect2I tileArea, bool isAttackTiles = false)
	{
		var tiles = tileArea.ToTiles();

		if (tiles.Count == 0) return false;

		(TileMapLayer firstTileMapLayer, _) = GetTileCustomData(tiles[0], IS_BUILDABLE);

		var targetElevationLayer = firstTileMapLayer != null ? tileMapLayerToElevationLayer[firstTileMapLayer] : null;
		var tileSetToCheck = GetBuildableTileSet(isAttackTiles);

		if (isAttackTiles)
		{
			tileSetToCheck = tileSetToCheck.Except(occupiedTiles).ToHashSet();
		}

		return tiles.All((tilePosition) => {
			(TileMapLayer tileMapLayer, bool isBuildable) = GetTileCustomData(tilePosition, IS_BUILDABLE);

			var elevationLayer = tileMapLayer != null ? tileMapLayerToElevationLayer[tileMapLayer] : null;
			return isBuildable && tileSetToCheck.Contains(tilePosition) && elevationLayer == targetElevationLayer;
		});
	}

	public void HighlightGoblinOccupiedTiles()
	{
		var atlasCoords = new Vector2I(2, 0);

		foreach (var tilePosition in goblinOccupiedTiles)
		{
			highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void HighlightBuildableTiles(bool isAttackTiles = false)
	{
		foreach (var tilePosition in GetBuildableTileSet(isAttackTiles))
		{
			highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HighlightExpandedBuildableTiles(Rect2I tileArea, int radius)
	{
		var validTiles = GetValidTilesInRadius(tileArea, radius).ToHashSet();
		var expandedTiles = validTiles.Except(validBuildableTiles).Except(occupiedTiles);
		var atlasCoords = new Vector2I(1, 0);

		foreach (var tilePosition in expandedTiles)
		{
			highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void HighlightAttackTiles(Rect2I tileArea, int radius)
	{
		var buildingAreaTiles = tileArea.ToTiles();
		var validTiles = GetValidTilesInRadius(tileArea, radius).ToHashSet().Except(validAttackTiles).Except(buildingAreaTiles);
		var atlasCoords = new Vector2I(1, 0);

		foreach (var tilePosition in validTiles)
		{
			highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void HighlightResourceTiles(Rect2I tileArea, int radius)
	{
		var resourceTiles = GetResourceTilesInRadius(tileArea, radius);
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

	public Vector2I GetMouseGridCellPositionWithDimensionOffset(Vector2 dimensions)
	{
		var mouseGridPosition = highlightTilemapLayer.GetGlobalMousePosition() / 64;

		mouseGridPosition -= dimensions / 2;
		mouseGridPosition = mouseGridPosition.Round();
		return new Vector2I((int) mouseGridPosition.X, (int) mouseGridPosition.Y);
	}

	public Vector2I GetMouseGridCellPosition()
	{
		var mousePosition = highlightTilemapLayer.GetGlobalMousePosition();
		return ConvertWorldPositionToTilePosition(mousePosition);
	}

	public Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
	{
		var tilePosition = worldPosition / 64;
		
		tilePosition = tilePosition.Floor();
		return new Vector2I((int)tilePosition.X, (int)tilePosition.Y);
	}

	private HashSet<Vector2I> GetBuildableTileSet(bool isAttackTiles = false)
	{
		return isAttackTiles ? validAttackTiles : validBuildableTiles;
	}

	private List<TileMapLayer> GetAllTilemapLayers(Node2D rootNode)
	{
		var result = new List<TileMapLayer>();
		var children = rootNode.GetChildren();

		children.Reverse();

		foreach (var child in children){
			if (child is Node2D childNode)
			{
				result.AddRange(GetAllTilemapLayers(childNode));
			}
		}

		if (rootNode is TileMapLayer tileMapLayer)
		{
			result.Add(tileMapLayer);
		}
		return result;
	}

	private void MapTileMapLayersToElevationLayers()
	{
		foreach(var layer in allTilemapLayers)
		{
			ElevationLayer elevationLayer;
			Node startNode = layer;
			do
			{
				var parent = startNode.GetParent();

				elevationLayer = parent as ElevationLayer;
				startNode = parent;
			} while (elevationLayer == null && startNode != null);

		tileMapLayerToElevationLayer[layer] = elevationLayer;
		}
	}

	private void UpdateGoblinOccupiedTiles(BuildingComponent buildingComponent)
	{
		occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPositions());

		var rootCell = buildingComponent.GetGridCellPosition();
		var tileArea = new Rect2I(rootCell, buildingComponent.BuildingResource.Dimensions);

		if (buildingComponent.BuildingResource.DangerRadius > 0){
			var tilesInRadius = GetValidTilesInRadius(tileArea, buildingComponent.BuildingResource.DangerRadius).ToHashSet();

			tilesInRadius.ExceptWith(occupiedTiles);
			goblinOccupiedTiles.UnionWith(tilesInRadius);
		}
	}

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPositions());
		
		var rootCell = buildingComponent.GetGridCellPosition();
		var tileArea = new Rect2I(rootCell, buildingComponent.BuildingResource.Dimensions);
		var validTiles = GetValidTilesInRadius(tileArea, buildingComponent.BuildingResource.BuildableRadius);
		var allTiles = GetTilesInRadius(tileArea, buildingComponent.BuildingResource.BuildableRadius, (_) => true);

		allTilesInBuildingRadius.UnionWith(allTiles);
		validBuildableTiles.UnionWith(validTiles);
		validBuildableTiles.ExceptWith(occupiedTiles);
		validAttackTiles.UnionWith(validBuildableTiles);
		validBuildableTiles.ExceptWith(goblinOccupiedTiles);
		EmitSignal(SignalName.GridStateUpdated);
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var tileArea = new Rect2I(rootCell, buildingComponent.BuildingResource.Dimensions);
		var resourceTiles = GetResourceTilesInRadius(tileArea, buildingComponent.BuildingResource.ResourceRadius);
		var oldResourceTilesCount = collectedResourceTiles.Count;

		collectedResourceTiles.UnionWith(resourceTiles);

		if (oldResourceTilesCount != collectedResourceTiles.Count)
		{
			EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);
		}

		EmitSignal(SignalName.GridStateUpdated);
	}

	private void RecalculateGrid()
	{
		occupiedTiles.Clear();
		validBuildableTiles.Clear();
		validAttackTiles.Clear();
		allTilesInBuildingRadius.Clear();
		collectedResourceTiles.Clear();
		goblinOccupiedTiles.Clear();

		var buildingComponents = BuildingComponent.GetValidBuildingComponents(this);

		foreach (var buildingComponent in buildingComponents)
		{
			UpdateGoblinOccupiedTiles(buildingComponent);
			UpdateValidBuildableTiles(buildingComponent);
			UpdateCollectedResourceTiles(buildingComponent);
		}

		EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);
		EmitSignal(SignalName.GridStateUpdated);
	}

	private bool IsTileInsideCircle(Vector2 centerPosition, Vector2 tilePosition, float radius)
	{
		var distanceX = centerPosition.X - (tilePosition.X + .5);
		var distanceY = centerPosition.Y - (tilePosition.Y + .5);
		var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
		return distanceSquared <= radius * radius;
	}

	private List<Vector2I> GetTilesInRadius(Rect2I tileArea, int radius, Func<Vector2I, bool> filterFn)
	{
		var result = new List<Vector2I>();
		var tileAreaF = tileArea.ToRect2F();
		var tileAreaCenter = tileAreaF.GetCenter();
		var radiusMod = Mathf.Max(tileAreaF.Size.X, tileAreaF.Size.Y) / 2;

		for (var x = tileArea.Position.X - radius; x < tileArea.End.X + radius; x++)
		{
			for (var y = tileArea.Position.Y - radius; y < tileArea.End.Y + radius; y++)
			{
				var tilePosition = new Vector2I(x, y);

				if (!IsTileInsideCircle(tileAreaCenter, tilePosition, radius + radiusMod) || !filterFn(tilePosition))
				{
					continue;
				}
				result.Add(tilePosition);
			}
		}
		return result;
	}

	private List<Vector2I> GetValidTilesInRadius(Rect2I tileArea, int radius)
	{
		return GetTilesInRadius(tileArea,radius, (tilePosition) =>
		{
			return GetTileCustomData(tilePosition, IS_BUILDABLE).Item2;
		});
	}

	private List<Vector2I> GetResourceTilesInRadius(Rect2I tileArea, int radius)
	{
		return GetTilesInRadius(tileArea,radius, (tilePosition) =>
		{
			return GetTileCustomData(tilePosition, IS_WOOD).Item2;
		});
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateGoblinOccupiedTiles(buildingComponent);
		UpdateValidBuildableTiles(buildingComponent);
		UpdateCollectedResourceTiles(buildingComponent);
	}

	private void OnBuildingDestroyed(BuildingComponent buildingComponent)
	{
		RecalculateGrid();
	}
}