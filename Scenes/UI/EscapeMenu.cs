using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class EscapeMenu : CanvasLayer
{
	private readonly StringName ESCAPE_ACTION = "escape";

	[Export(PropertyHint.File, "*.tscn")] private string mainMenuScenePath;
	[Export] private PackedScene optionsMenuScene;
	
	private Button resumeButton;
	private Button optionsButton;
	private Button quitButton;
	private MarginContainer marginContainer;
	
	public override void _Ready()
	{
		resumeButton = GetNode<Button>("%ResumeButton");
		optionsButton = GetNode<Button>("%OptionsButton");
		quitButton = GetNode<Button>("%QuitButton");
		marginContainer = GetNode<MarginContainer>("MarginContainer");

		AudioHelpers.RegisterButtons(new Button[] { resumeButton, optionsButton, quitButton });

		resumeButton.Pressed += OnResumeButtonPressed;
		optionsButton.Pressed += OnOptionsButtonPressed;
		quitButton.Pressed += OnQuitButtonPressed;
	}

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt.IsActionPressed(ESCAPE_ACTION))
		{
			QueueFree();
			GetViewport().SetInputAsHandled();
		}
    }

	private void OnResumeButtonPressed()
	{
		 QueueFree();
	}

	private void OnOptionsButtonPressed()
	{
		marginContainer.Visible = false;
		
		var optionsMenu = optionsMenuScene.Instantiate<OptionsMenu>();

		AddChild(optionsMenu);
		optionsMenu.DonePressed += () => { OnOptionsDonePressed(optionsMenu); };
	}

	private void OnQuitButtonPressed()
	{
		GetTree().ChangeSceneToFile(mainMenuScenePath);
	}

	private void OnOptionsDonePressed(OptionsMenu optionsMenu)
	{
		marginContainer.Visible = true;
		optionsMenu.QueueFree();
	}
}
