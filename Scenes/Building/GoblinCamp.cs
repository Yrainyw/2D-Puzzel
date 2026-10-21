using Game.Component;
using Godot;

namespace Game.Building;

public partial class GoblinCamp : Node2D
{
	[Export] private BuildingComponent buildingComponent;
	[Export] private AnimatedSprite2D fireAnimatedSprite2D;
	[Export] private AnimatedSprite2D animatedSprite2D;
	
	public override void _Ready()
	{
		fireAnimatedSprite2D.Visible = false;
		buildingComponent.Disabled += OnDisabled;
		buildingComponent.Enabled += OnEnabled;
	}

	private void OnDisabled()
	{
		animatedSprite2D.Play("destroyed");
		fireAnimatedSprite2D.Visible = true;
	}

	private void OnEnabled()
	{
		animatedSprite2D.Play("default");
		fireAnimatedSprite2D.Visible = false;
	}
}
