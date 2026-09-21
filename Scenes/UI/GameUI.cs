using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUI : MarginContainer
{
	[Signal]
	public delegate void BuildingResourceSelectedEventHandler(buildingResource buildingResource);

	private HBoxContainer hBoxContainer;

	[Export]
	private buildingResource[] buildingResources;
	
	private Button placeTowerButton;
	private Button placeVillageButton;

	public override void _Ready()
	{
		hBoxContainer = GetNode<HBoxContainer>("HBoxContainer");
		CreateBuildingButtons();
	}

	private void CreateBuildingButtons()
	{
		foreach (var buildingResource in buildingResources)
		{
			var buildingButton = new Button();
			buildingButton.Text = $"Place {buildingResource.DisplayName}";
			hBoxContainer.AddChild(buildingButton);
			buildingButton.Pressed += () =>
			{
				EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
			};
		}
	}
}
