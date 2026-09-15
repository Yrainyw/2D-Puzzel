using Godot;
using System;

namespace Game;

public partial class Main : Node2D
{
	private Sprite2D cursor;
	private PackedScene buildingScene;
	private Button placeBuildingButton;

	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://Scenes/Building/Building.tscn");
		cursor = GetNode<Sprite2D>("Cursor");
		placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
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
	}

	private void OnButtonPressed()
	{
		cursor.Visible = true;
	}
}
