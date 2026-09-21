using Godot;
using Game.Manager;
using Game.Resources.Building;
using Game.UI;

namespace Game;

public partial class Main : Node
{
	private GridManager gridManager;
	private Sprite2D cursor;
	private Node2D ySortRoot;
	private Vector2I? hoveredGridCell;
	private buildingResource toPlaceBuildingResource;
	private GameUI gameUI;


	public override void _Ready()
	{
		gridManager = GetNode<GridManager>("Grid Manager");
		cursor = GetNode<Sprite2D>("Cursor");
		ySortRoot = GetNode<Node2D>("YSortRoot");
		gameUI = GetNode<GameUI>("GameUI");

		cursor.Visible = false;
		gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
		gridManager.ResourceTileUpdated += OnResourceTileUpdated;
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (hoveredGridCell.HasValue && evt.IsActionPressed("left_click") && gridManager.IsTilePositionBuildable(hoveredGridCell.Value))
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
	}

	private void OnBuildingResourceSelected(buildingResource buildingResource)
	{
		toPlaceBuildingResource = buildingResource;
		cursor.Visible = true;
		gridManager.HighlightBuildableTiles();
	}

	private void OnResourceTileUpdated(int resourCount)
	{
		GD.Print(resourCount);
	}
}