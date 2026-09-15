using Godot;
using System;

namespace Game;

public partial class Main : Node2D
{
	private Sprite2D sprite;
	private PackedScene buildingScene;

	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://Scenes/Building/Building.tscn");
		sprite = GetNode<Sprite2D>("Cursor");
	}

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt.IsActionPressed("left_click"))
		{
			PlaceBuildingAtMousePosition();
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var grid_position = GetMouseGridCellPosition();
		sprite.GlobalPosition = grid_position * 64;
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
}
