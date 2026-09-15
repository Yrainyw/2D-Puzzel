using Godot;
using System;

namespace Game;

public partial class Main : Node2D
{
	private Sprite2D sprite;

	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Cursor");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var mouse_position = GetGlobalMousePosition();
		var grid_position = mouse_position / 64;

		grid_position = grid_position.Floor();
		sprite.GlobalPosition = grid_position * 64;
	}
}
