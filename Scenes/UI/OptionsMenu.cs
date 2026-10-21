using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class OptionsMenu : CanvasLayer
{
	private const string SFX_BUS_NAME = "SFX";
	private const string MUSIC_BUS_NAME = "Music";
	
	private Button sfxDownButton;
	private Button sfxUpButton;
	private Label sfxLabel;

	private Button musicDownButton;
	private Button musicUpButton;
	private Label musicLabel;

	private Button windowButton;
	private Button doneButton;

	public override void _Ready()
	{
		sfxDownButton = GetNode<Button>("%SFXDownButton");
		sfxUpButton = GetNode<Button>("%SFXUpButton");
		sfxLabel = GetNode<Label>("%SFXLabel");

		musicDownButton = GetNode<Button>("%MusicDownButton");
		musicUpButton = GetNode<Button>("%MusicUpButton");
		musicLabel = GetNode<Label>("%MusicLabel");

		windowButton = GetNode<Button>("%WindowButton");
		doneButton = GetNode<Button>("%DoneButton");

		AudioHelpers.RegisterButtons(new Button[] { sfxUpButton, sfxDownButton, musicUpButton, musicDownButton, windowButton, doneButton });
		UpdateDisplay();

		sfxUpButton.Pressed += () => { ChangeBusVolumn(SFX_BUS_NAME,.1f); };
		sfxDownButton.Pressed += () => { ChangeBusVolumn(SFX_BUS_NAME, -.1f); };
		musicUpButton.Pressed += () => { ChangeBusVolumn(MUSIC_BUS_NAME, .1f); };
		musicDownButton.Pressed += () => { ChangeBusVolumn(MUSIC_BUS_NAME, -.1f); };

		windowButton.Pressed += OnWindowButtonPressed;
	}

	private void UpdateDisplay()
	{
		sfxLabel.Text = Mathf.Round(OptionsHelper.GetBusVolumnPercent(SFX_BUS_NAME) * 10).ToString();
		musicLabel.Text = Mathf.Round(OptionsHelper.GetBusVolumnPercent(MUSIC_BUS_NAME) * 10).ToString();
		windowButton.Text = OptionsHelper.IsFullScreen() ? "Fullscreen" : "Windowed";
	}

	private void ChangeBusVolumn(string busName, float change)
	{
		var busVolumnPercent = OptionsHelper.GetBusVolumnPercent(busName);

		busVolumnPercent = Mathf.Clamp(busVolumnPercent + change, 0, 1);
		OptionsHelper.SetBusVolumnPercent(busName, busVolumnPercent);
		UpdateDisplay();
	}

	private void OnWindowButtonPressed()
	{
		OptionsHelper.ToggleWindowMode();
		UpdateDisplay();
	}
}
