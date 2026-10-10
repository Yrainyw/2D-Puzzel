using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{
	[Signal]
	public delegate void BuildingResourceSelectedEventHandler(buildingResource buildingResource);

	private VBoxContainer BuildingSectionContainer;

	[Export]
	private buildingResource[] buildingResources;

	[Export]
	private PackedScene buildingSectionScene;
	
	private Button placeTowerButton;
	private Button placeVillageButton;

	public override void _Ready()
	{
		BuildingSectionContainer = GetNode<VBoxContainer>("%BuildingSectionContainer");
		CreateBuildingSections();
	}

	private void CreateBuildingSections()
	{
		foreach (var buildingResource in buildingResources)
		{
			var buildingSection = buildingSectionScene.Instantiate<BuildingSection>();
			BuildingSectionContainer.AddChild(buildingSection);
			buildingSection.SetBuildingResource(buildingResource);

			buildingSection.SelectedButtonPressed += () =>
			{
				EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
			};
		}
	}
}
