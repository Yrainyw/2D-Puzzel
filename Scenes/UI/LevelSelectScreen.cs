using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class LevelSelectScreen : MarginContainer
{
	[Signal]
	public delegate void BackPressedEventHandler();
	
	[Export] private PackedScene LevelSelectSectionScene;

	private GridContainer gridContainer;
	private Button backButton;

	public override void _Ready()
	{
		gridContainer = GetNode<GridContainer>("%GridContainer");
		backButton = GetNode<Button>("BackButton");

		var levelDefinitions = LevelManager.GetLevelDefinitions();

		for (var i = 0; i < levelDefinitions.Length; i++)
		{
			var levelDefinition = levelDefinitions[i];
			var levelSelectSection = LevelSelectSectionScene.Instantiate<LevelSelectSection>();

			gridContainer.AddChild(levelSelectSection);
			levelSelectSection.SetLevelDefinition(levelDefinition);
			levelSelectSection.SetLevelIndex(i);
			levelSelectSection.LevelSelected += OnLevelSelected;
		}

		backButton.Pressed += OnBackButtonPressed;
	}

	private void OnLevelSelected(int levelIndex)
	{
		LevelManager.Instance.ChangeToLevel(levelIndex);
	}

	private void OnBackButtonPressed()
	{
		EmitSignal(SignalName.BackPressed);
	}
}
