using Game.Resources.Level;
using Godot;

namespace Game.UI;

public partial class LevelSelectSection : PanelContainer
{
	[Signal]
	public delegate void LevelSelectedEventHandler(int levelIndex);
	
	private Button button;
	private Label resourceCountLabel;
	private Label levelNumberLabel;
	private int levelIndex;

	public override void _Ready()
	{
		button = GetNode<Button>("%Button");
		resourceCountLabel = GetNode<Label>("%ResourceCountLabel");
		levelNumberLabel = GetNode<Label>("%LevelNumberLabel");

		button.Pressed += OnButtonPressed;
	}

	public void SetLevelDefinition(LevelDefinitionResources levelDefinition)
	{
		resourceCountLabel.Text = levelDefinition.StartingResourceCount.ToString();
	}

	public void SetLevelIndex(int index)
	{
		levelIndex = index;
		levelNumberLabel.Text = $"Level {index + 1}";
	}

	private void OnButtonPressed()
	{
		EmitSignal(SignalName.LevelSelected, levelIndex);
	}
}
