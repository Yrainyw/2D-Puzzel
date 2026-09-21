using Godot;

namespace Game.UI;

public partial class GameUI : MarginContainer
{
	[Signal]
	public delegate void PlaceTowerButtonPressedEventHandler();

	[Signal]
	public delegate void PlaceVillageButtonPressedEventHandler();
	
	private Button placeTowerButton;
	private Button placeVillageButton;

	public override void _Ready()
	{
		placeTowerButton = GetNode<Button>("%PlaceTowerButton");
		placeVillageButton = GetNode<Button>("%PlaceVillageButton");

		placeTowerButton.Pressed += OnPlaceTowerButtonPressed;
		placeVillageButton.Pressed += OnPlaceVillageButtonPressed;
	}

	private void OnPlaceTowerButtonPressed()
	{
		EmitSignal(SignalName.PlaceTowerButtonPressed);
	}

	private void OnPlaceVillageButtonPressed()
	{
		EmitSignal(SignalName.PlaceVillageButtonPressed);
	}
}
