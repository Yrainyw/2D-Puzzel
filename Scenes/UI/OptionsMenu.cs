using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class OptionsMenu : CanvasLayer // 游戏选项菜单
{
	private const string SFX_BUS_NAME = "SFX";
	private const string MUSIC_BUS_NAME = "Music";

	[Signal]
	public delegate void DonePressedEventHandler();
	
	private Button sfxDownButton;
	private Button sfxUpButton;
	private Label sfxLabel;

	private Button musicDownButton;
	private Button musicUpButton;
	private Label musicLabel;

	private Button windowButton;
	private Button doneButton;

	// 初始化按钮并绑定点击事件
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

		AudioHelpers.RegisterButtons(new Button[] { sfxUpButton, sfxDownButton, musicUpButton, musicDownButton, windowButton, doneButton }); // 注册按钮音效并更新界面
		UpdateDisplay();

		// 绑定音量增减按钮
		sfxUpButton.Pressed += () => { ChangeBusVolumn(SFX_BUS_NAME,.1f); };
		sfxDownButton.Pressed += () => { ChangeBusVolumn(SFX_BUS_NAME, -.1f); };
		musicUpButton.Pressed += () => { ChangeBusVolumn(MUSIC_BUS_NAME, .1f); };
		musicDownButton.Pressed += () => { ChangeBusVolumn(MUSIC_BUS_NAME, -.1f); };

		// 绑定窗口切换和返回按钮
		windowButton.Pressed += OnWindowButtonPressed;
		doneButton.Pressed += OnDoneButtonPressed;
	}

	// 更新音量数字和窗口模式显示
	private void UpdateDisplay()
	{
		sfxLabel.Text = Mathf.Round(OptionsHelper.GetBusVolumnPercent(SFX_BUS_NAME) * 10).ToString();
		musicLabel.Text = Mathf.Round(OptionsHelper.GetBusVolumnPercent(MUSIC_BUS_NAME) * 10).ToString();
		windowButton.Text = OptionsHelper.IsFullScreen() ? "Fullscreen" : "Windowed";
	}

	// 调整音量并保存设置
	private void ChangeBusVolumn(string busName, float change)
	{
		var busVolumePercent = OptionsHelper.GetBusVolumnPercent(busName);

		busVolumePercent = Mathf.Clamp(busVolumePercent + change, 0, 1); // 限制音量在 0～1
		OptionsHelper.SetBusVolumnPercent(busName, busVolumePercent); // 修改实际音量
		SaveManager.SaveVolume(busName, busVolumePercent); // 保存音量到文件
		UpdateDisplay(); // 更新界面
	}

	// 切换全屏或窗口模式
	private void OnWindowButtonPressed()
	{
		OptionsHelper.ToggleWindowMode();
		SaveManager.SaveIsFullScreen(OptionsHelper.IsFullScreen());
		UpdateDisplay();
	}

	// 关闭选项菜单
	private void OnDoneButtonPressed()
	{
		EmitSignal(SignalName.DonePressed);
	}
}
