using Godot;

namespace Game.Autoload;

public partial class OptionsHelper : Node // 管理游戏音量和窗口模式
{
	// 设置指定音频总线的音量
	public static void SetBusVolumnPercent(string busName, float volumnPercent)
	{
		var busIndex = AudioServer.GetBusIndex(busName);

		AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(volumnPercent)); // 将音量转换为分贝并应用
	}

	// 获取指定音频总线的音量
	public static float GetBusVolumnPercent(string busName)
	{
		var busIndex = AudioServer.GetBusIndex(busName);
		return Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex)); // 将分贝转换为 0～1 的音量值
	}

	// 切换全屏和窗口模式
	public static void ToggleWindowMode()
	{
		if (IsFullScreen())
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); // 窗口
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen); // 全屏
		}
	}

	// 判断当前是否为全屏模式
	public static bool IsFullScreen()
	{
		return DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen;
	}
}
