using Game.UI;
using Godot;
using Game.Resources.Building;
using Game.Building;
using System;
using Game.Component;
using System.Linq;

namespace Game.Manager;

public partial class BuildingManager : Node
{
	private readonly StringName ACTION_LEFT_CLICK = "left_click"; 
	private readonly StringName ACTION_CANCEL = "cancel";
	private readonly StringName ACTION_RIGHT_CLICK = "right_click";

	[Export]
	private int startingResourceCount = 4;
	
	[Export]
	private GridManager gridManager;

	[Export]
	private GameUI gameUI;

	[Export]
	private Node2D ySortRoot;

	[Export]
	private PackedScene buildingGhostScene;

	private enum State 
	{
		Normal, 
		PlacingBuilding
	}

	private int currentResourceCount;
	private int currentlyUsedResourceCount;
	private buildingResource toPlaceBuildingResource;
	private Vector2I hoveredGridCell;
	private BuildingGhost buildingGhost;
	private State currentState;
	private int AvaliableResourceCount => (startingResourceCount + currentResourceCount) - currentlyUsedResourceCount;

	public override void _Ready()
	{
		gridManager.ResourceTileUpdated += OnResourceTileUpdated;
		gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		switch (currentState)
		{
			case State.Normal:
			if (evt.IsActionPressed(ACTION_RIGHT_CLICK))
				{
					DestroyBuildingAtHoveredCellPosition();
				}
				break;
			case State.PlacingBuilding:
				if (evt.IsActionPressed(ACTION_CANCEL))
				{
					ChangeState(State.Normal);
				}
				else if (
					toPlaceBuildingResource != null && 
					evt.IsActionPressed(ACTION_LEFT_CLICK) && 
					IsBuildingPlaceableAtTile(hoveredGridCell))
				{
					PlaceBuildingAtHoveredCellPosition();
				}
				break;
			default:
				break;
		}
	}

	public override void _Process(double delta)
	{	
		var gridPosition = gridManager.GetMouseGridCellPosition();

		if (hoveredGridCell != gridPosition)
		{
			hoveredGridCell = gridPosition;	
			UpdateHoveredGridCell();		
		}

		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				buildingGhost.GlobalPosition = gridPosition * 64;
				break;
		}
	}

	private void UpdateGridDisplay()
	{
		gridManager.ClearHighlightedTiles();
		gridManager.HighlightBuildableTiles();

		if (IsBuildingPlaceableAtTile(hoveredGridCell))
		{
			gridManager.HighlightExpandedBuildableTiles(hoveredGridCell, toPlaceBuildingResource.BuildableRadius);
			gridManager.HighlightResourceTiles(hoveredGridCell, toPlaceBuildingResource.ResourceRadius);	
			buildingGhost.SetValid();
		}
		else
		{
			buildingGhost.SetInValid();
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();

		ySortRoot.AddChild(building);
		building.GlobalPosition = hoveredGridCell * 64;
		currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
		ChangeState(State.Normal);
	}

	private void DestroyBuildingAtHoveredCellPosition()
	{
		var buildingComponent = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
		.FirstOrDefault((buildingComponent) => buildingComponent.GetGridCellPosition() == hoveredGridCell);

		if (buildingComponent == null) return;

		currentlyUsedResourceCount -= buildingComponent.buildingResource.ResourceCost;
		buildingComponent.Destroy();
		GD.Print(AvaliableResourceCount);
	}

	private void ClearBuildingGhost()
	{
		gridManager.ClearHighlightedTiles();

		if (IsInstanceValid(buildingGhost))
		{
			buildingGhost.QueueFree();
		}

		buildingGhost = null;
	}

	private bool IsBuildingPlaceableAtTile(Vector2I tilePosition)
	{
		return gridManager.IsTilePositionBuildable(tilePosition) && AvaliableResourceCount >= toPlaceBuildingResource.ResourceCost;
	}

	private void UpdateHoveredGridCell()
	{
		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				UpdateGridDisplay();
				break;
		}
	}

	private void ChangeState(State toState)
	{
		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				ClearBuildingGhost();
				toPlaceBuildingResource = null;
				break;
		}

		currentState = toState;

		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
				ySortRoot.AddChild(buildingGhost);
				break;
		}
	}

	private void OnResourceTileUpdated(int resourceCount)
	{
		currentResourceCount = resourceCount;
	}

	private void OnBuildingResourceSelected(buildingResource buildingResource)
	{
		ChangeState(State.PlacingBuilding);

		var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
		
		buildingGhost.AddChild(buildingSprite);
		toPlaceBuildingResource = buildingResource;
		UpdateGridDisplay();
	}
}

