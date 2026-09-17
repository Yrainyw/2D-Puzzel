using Godot;
using System;

namespace Game;

public partial class Main : Node2D
{
	private Sprite2D cursor;
	private PackedScene buildingScene;
	private Button placeBuildingButton;
	private TileMapLayer highlightTilemapLayer;
	private Vector2? hoveredGridCell;

	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://Scenes/Building/Building.tscn");
		cursor = GetNode<Sprite2D>("Cursor");
		placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
		highlightTilemapLayer = GetNode<TileMapLayer>("HighlightTileMapLayer");
		cursor.Visible = false;
		placeBuildingButton.Pressed += OnButtonPressed;
	}

    public override void _UnhandledInput(InputEvent evt)
    {
        if (cursor.Visible && evt.IsActionPressed("left_click"))
		{
			PlaceBuildingAtMousePosition();
			cursor.Visible = false;
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var grid_position = GetMouseGridCellPosition();

		cursor.GlobalPosition = grid_position * 64;

		if (cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != grid_position))
		{
			hoveredGridCell = grid_position;
			UpdateHighlightTilemapLayer();
		}
	}

	private Vector2 GetMouseGridCellPosition()
	{
		var mouse_position = GetGlobalMousePosition();
		var grid_position = mouse_position / 64;

		grid_position = grid_position.Floor();
		return grid_position;
	}

	private void PlaceBuildingAtMousePosition()
	{
		var building = buildingScene.Instantiate<Node2D>();

		AddChild(building);

		var grid_position = GetMouseGridCellPosition();

		building.GlobalPosition = grid_position * 64;
		hoveredGridCell = null;
		UpdateHighlightTilemapLayer();
	}

	private void UpdateHighlightTilemapLayer()
	{
		highlightTilemapLayer.Clear();
		
		if (!hoveredGridCell.HasValue)
		{
			return;
		}

		for (var x  = hoveredGridCell.Value.X - 3; x <= hoveredGridCell.Value.X + 3; x++)
		{
			for (var y = hoveredGridCell.Value.Y - 3; y <= hoveredGridCell.Value.Y + 3; y++)
			{
				highlightTilemapLayer.SetCell(new Vector2I((int)x, (int)y), 0, Vector2I.Zero);
			}
		}
	}

	private void OnButtonPressed()
	{
		cursor.Visible = true;
	}
}
