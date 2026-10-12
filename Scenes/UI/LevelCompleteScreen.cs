using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class LevelCompleteScreen : CanvasLayer
{
	private Button nextLevelButton;
	
	public override void _Ready()
	{
		nextLevelButton = GetNode<Button>("%NextLevelButton");
		nextLevelButton.Pressed += OnNextLevelButtonPressed;
	}
	
	private void OnNextLevelButtonPressed()
	{
		LevelManager.Instance.ChangeToNextLevel();
	}

}
