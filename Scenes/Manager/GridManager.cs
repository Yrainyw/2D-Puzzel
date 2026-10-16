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
	private HashSet<Vector2I> validBuildableAttackTiles = new();
	private HashSet<Vector2I> allTilesInBuildingRadius = new();
	private HashSet<Vector2I> collectedResourceTiles = new();
	private HashSet<Vector2I> occupiedTiles = new();
	private HashSet<Vector2I> goblinOccupiedTiles = new();
	private HashSet<Vector2I> attackTiles = new();

	[Export] private TileMapLayer highlightTilemapLayer;
	[Export] private TileMapLayer baseTerrainTilemapLayer;

	private List<TileMapLayer> allTilemapLayers = new();
	private Dictionary<TileMapLayer, ElevationLayer> tileMapLayerToElevationLayer = new();
	private Dictionary<BuildingComponent, HashSet<Vector2I>> buildingToBuildableTiles = new();

	public override void _Ready()
	{
		GameEvents.Instance.Connect(GameEvents.SignalName.BuildingPlaced, Callable.From<BuildingComponent>(OnBuildingPlaced));
		GameEvents.Instance.Connect(GameEvents.SignalName.BuildingDestroyed, Callable.From<BuildingComponent>(OnBuildingDestroyed));
		GameEvents.Instance.Connect(GameEvents.SignalName.BuildingEnabled, Callable.From<BuildingComponent>(OnBuildingEnabled));
		GameEvents.Instance.Connect(GameEvents.SignalName.BuildingDisabled, Callable.From<BuildingComponent>(OnBuildingDisabled));
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
		var validTiles = GetValidTilesInRadius(tileArea, radius).ToHashSet().Except(validBuildableAttackTiles).Except(buildingAreaTiles);
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

	public bool CanDestroyBuilding(BuildingComponent toDestroyBuildingComponent)
	{
		if (toDestroyBuildingComponent.BuildingResource.BuildableRadius > 0)
		{
			var dependentBuildings = BuildingComponent.GetValidBuildingComponents(this).Where((buildingComponent) => {
				if (buildingComponent == toDestroyBuildingComponent) return false;

				if (buildingComponent.BuildingResource.IsBase) return false;

				var anyTilesInRadius = buildingComponent.GetTileArea().ToTiles()
					.Any((tilePosition) => buildingToBuildableTiles[toDestroyBuildingComponent].Contains(tilePosition));
				return anyTilesInRadius;
			});

			var allBuildingsStillValid = dependentBuildings.All((dependentBuilding) => {
				var tilesForBuilding = dependentBuilding.GetTileArea().ToTiles();
				return tilesForBuilding.All((tilePosition) => {
					var tileIsInSet = buildingToBuildableTiles.Keys
						.Where((key) => key != toDestroyBuildingComponent && key != dependentBuilding) 
						.Any((buildingComponent) => buildingToBuildableTiles[buildingComponent].Contains(tilePosition));
					return tileIsInSet;
				});
			});

			if (!allBuildingsStillValid)
			{
				return false;
			}
			return IsBuildingNetworkConnected(toDestroyBuildingComponent);
		}
		return true;
	}

	private bool IsBuildingNetworkConnected(BuildingComponent toDestroyBuildingComponent)
	{
		var baseBuilding = BuildingComponent.GetValidBuildingComponents(this).First((buildingComponent) => buildingComponent.BuildingResource.IsBase);
		var visitedBuildings = new HashSet<BuildingComponent>();

		VisitAllConnectedBuildings(baseBuilding, toDestroyBuildingComponent, visitedBuildings);

		var totalBuildingsToVisit = BuildingComponent.GetValidBuildingComponents(this)
			.Count((buildingComponent) => 
			{
				return buildingComponent != toDestroyBuildingComponent && buildingComponent.BuildingResource.BuildableRadius > 0;
			});
		return totalBuildingsToVisit == visitedBuildings.Count;
	}

	private void VisitAllConnectedBuildings(BuildingComponent rootBuilding, BuildingComponent excludeBuilding, HashSet<BuildingComponent> visitedBuildings)
	{
		var dependentBuildings = BuildingComponent.GetValidBuildingComponents(this).Where((buildingComponent) =>
		{
			if (buildingComponent.BuildingResource.BuildableRadius == 0) return false;

			if (visitedBuildings.Contains(buildingComponent)) return false;

			var anyTilesInRadius = buildingComponent.GetTileArea().ToTiles()
				.Any((tilePosition) => buildingToBuildableTiles[rootBuilding].Contains(tilePosition));
			return buildingComponent != excludeBuilding && anyTilesInRadius;
		}).ToList();

		visitedBuildings.UnionWith(dependentBuildings);
		
		foreach (var dependentBuilding in dependentBuildings)
		{
			VisitAllConnectedBuildings(dependentBuilding, excludeBuilding, visitedBuildings);
		}
	}

	private HashSet<Vector2I> GetBuildableTileSet(bool isAttackTiles = false)
	{
		return isAttackTiles ? validBuildableAttackTiles : validBuildableTiles;
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

		if (buildingComponent.IsDisabled) return;

		var tileArea = buildingComponent.GetTileArea();

		if (buildingComponent.BuildingResource.IsDangerBuilding()){
			var tilesInRadius = GetValidTilesInRadius(tileArea, buildingComponent.BuildingResource.DangerRadius).ToHashSet();

			tilesInRadius.ExceptWith(occupiedTiles);
			goblinOccupiedTiles.UnionWith(tilesInRadius);
		}
	}

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPositions());
		
		var tileArea = buildingComponent.GetTileArea();

		if(buildingComponent.BuildingResource.BuildableRadius > 0)
		{
			var allTiles = GetTilesInRadius(tileArea, buildingComponent.BuildingResource.BuildableRadius, (_) => true);

			allTilesInBuildingRadius.UnionWith(allTiles);

			var validTiles = GetValidTilesInRadius(tileArea, buildingComponent.BuildingResource.BuildableRadius);

			buildingToBuildableTiles[buildingComponent] = validTiles.ToHashSet();
			validBuildableTiles.UnionWith(validTiles);
		}

		validBuildableTiles.ExceptWith(occupiedTiles);
		validBuildableAttackTiles.UnionWith(validBuildableTiles);
		validBuildableTiles.ExceptWith(goblinOccupiedTiles);
		EmitSignal(SignalName.GridStateUpdated);
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var tileArea = buildingComponent.GetTileArea();
		var resourceTiles = GetResourceTilesInRadius(tileArea, buildingComponent.BuildingResource.ResourceRadius);
		var oldResourceTilesCount = collectedResourceTiles.Count;

		collectedResourceTiles.UnionWith(resourceTiles);

		if (oldResourceTilesCount != collectedResourceTiles.Count)
		{
			EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);
		}

		EmitSignal(SignalName.GridStateUpdated);
	}

	private void UpdateAttackTiles(BuildingComponent buildingComponent)
	{
		if (!buildingComponent.BuildingResource.IsAttackBuilding())
		{
			return;
		}

		var tileArea = buildingComponent.GetTileArea();
		var newAttackTiles = GetTilesInRadius(tileArea, buildingComponent.BuildingResource.AttackRadius, (_) => true).ToHashSet(); 

		attackTiles.UnionWith(newAttackTiles);
	}

	private void RecalculateGrid()
	{
		occupiedTiles.Clear();
		validBuildableTiles.Clear();
		validBuildableAttackTiles.Clear();
		allTilesInBuildingRadius.Clear();
		collectedResourceTiles.Clear();
		goblinOccupiedTiles.Clear();
		attackTiles.Clear();
		buildingToBuildableTiles.Clear();

		var buildingComponents = BuildingComponent.GetValidBuildingComponents(this);

		foreach (var buildingComponent in buildingComponents)
		{
			UpdateBuildingComponentGridState(buildingComponent);
		}

		CheckGoblinCampDestruction();
		EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);
		EmitSignal(SignalName.GridStateUpdated);
	}

	public void RecalculateGoblinOccupiedTiles()
	{
		goblinOccupiedTiles.Clear();
		
		var dangerBuildings = BuildingComponent.GetDangerBuildingComponents(this);

		foreach (var building in dangerBuildings)
		{
			UpdateGoblinOccupiedTiles(building);
		}
	}

	public void CheckGoblinCampDestruction()
	{
		var dangerBuildings = BuildingComponent.GetDangerBuildingComponents(this);

		foreach (var building in dangerBuildings)
		{
			var tileArea = building.GetTileArea();
			var IsInsideAttackTile = tileArea.ToTiles().Any((tilePosition) => attackTiles.Contains(tilePosition));

			if (IsInsideAttackTile)
			{
				building.Disable();
			}
			else
			{
				building.Enable();
			}
		}
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

	private void UpdateBuildingComponentGridState(BuildingComponent buildingComponent)
	{
		UpdateGoblinOccupiedTiles(buildingComponent);
		UpdateValidBuildableTiles(buildingComponent);
		UpdateCollectedResourceTiles(buildingComponent);
		UpdateAttackTiles(buildingComponent);
		
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateBuildingComponentGridState(buildingComponent);
		CheckGoblinCampDestruction();
	}

	private void OnBuildingDestroyed(BuildingComponent buildingComponent)
	{
		RecalculateGrid();
	}

	private void OnBuildingEnabled(BuildingComponent buildingComponent)
	{
		UpdateBuildingComponentGridState(buildingComponent);
	}

	private void OnBuildingDisabled(BuildingComponent buildingComponent)
	{
		RecalculateGrid();
	}
}