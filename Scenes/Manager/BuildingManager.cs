using Game.UI;
using Godot;
using Game.Resources.Building;
using System.Runtime.CompilerServices;

namespace Game.Manager;

public partial class BuildingManager : Node
{
	[Export]
	private GridManager gridManager;

	[Export]
	private GameUI gameUI;

	[Export]
	private Node2D ySortRoot;

	[Export]
	private Node2D cursor;

	private int currentResourceCount;
	private int startingResourceCount = 4;
	private int currentlyUsedResourceCount;
	private buildingResource toPlaceBuildingResource;
	private Vector2I? hoveredGridCell;
	private int AvaliableResourceCount => (startingResourceCount + currentResourceCount) - currentlyUsedResourceCount;

	public override void _Ready()
	{
		gridManager.ResourceTileUpdated += OnResourceTileUpdated;
		gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (hoveredGridCell.HasValue && toPlaceBuildingResource != null && evt.IsActionPressed("left_click") && gridManager.IsTilePositionBuildable(hoveredGridCell.Value) && AvaliableResourceCount >= toPlaceBuildingResource.ResourceCost)
		{
			PlaceBuildingAtHoveredCellPosition();
			cursor.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		var gridPosition = gridManager.GetMouseGridCellPosition();
		cursor.GlobalPosition = gridPosition * 64;
		if (toPlaceBuildingResource != null && cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
		{
			hoveredGridCell = gridPosition;
			gridManager.ClearHighlightedTiles();
			gridManager.HighlightExpandedBuildableTiles(hoveredGridCell.Value, toPlaceBuildingResource.BuildableRadius);
			gridManager.HighlightResourceTiles(hoveredGridCell.Value, toPlaceBuildingResource.ResourceRadius);
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		if (!hoveredGridCell.HasValue)
		{
			return;
		}
		var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
		ySortRoot.AddChild(building);
		building.GlobalPosition = hoveredGridCell.Value * 64;
		hoveredGridCell = null;
		gridManager.ClearHighlightedTiles();
		currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
		GD.Print(AvaliableResourceCount);
	}

	private void OnResourceTileUpdated(int resourceCount)
	{
		currentResourceCount = resourceCount;
	}

	private void OnBuildingResourceSelected(buildingResource buildingResource)
	{
		toPlaceBuildingResource = buildingResource;
		cursor.Visible = true;
		gridManager.HighlightBuildableTiles();
	}
}

