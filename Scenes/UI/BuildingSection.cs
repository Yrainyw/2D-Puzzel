using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class BuildingSection : PanelContainer
{
	[Signal]
	public delegate void SelectedButtonPressedEventHandler();

	private Label titleLabel;
	private Button selectButton;

	public override void _Ready()
	{
		titleLabel = GetNode<Label>("%Label");
		selectButton = GetNode<Button>("%Button");
		selectButton.Pressed += OnSelectButtonPressed;
	}

	public void SetBuildingResource(buildingResource buildingResource)
	{
		titleLabel.Text = buildingResource.DisplayName;
		selectButton.Text = $"Select (Cost: {buildingResource.ResourceCost})";
	}

	private void OnSelectButtonPressed()
	{
		EmitSignal(SignalName.SelectedButtonPressed);
	}
}
