using Godot;

namespace Game.Autoload;

public partial class OptionsHelper : Node
{
	public static void SetBusVolumnPercent(string busName, float volumnPercent)
	{
		var busIndex = AudioServer.GetBusIndex(busName);

		AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(volumnPercent));
	}

	public static float GetBusVolumnPercent(string busName)
	{
		var busIndex = AudioServer.GetBusIndex(busName);
		return Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
	}

	public static void ToggleWindowMode()
	{
		if (IsFullScreen())
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
		}
	}

	public static bool IsFullScreen()
	{
		return DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen;
	}
}
