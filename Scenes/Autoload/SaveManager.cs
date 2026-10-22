using System;
using Game.Resources.Level;
using Godot;
using Newtonsoft.Json;

namespace Game.Autoload;

public partial class SaveManager : Node // 管理游戏存档的读取和保存
{
	private const string SFX_BUS_NAME = "SFX";
	private const string MUSIC_BUS_NAME = "Music";

	public static SaveManager Instance { get; private set; } // 全局访问 SaveManager
	private static SaveData saveData = new(); // 存储当前游戏数据
	private static readonly string SAVE_FILE_PATH = "user://save.json"; // 存档文件路径

	// 游戏启动时读取并恢复存档
	public override void _Ready()
	{
		Instance = this;
		LoadSaveData();
		ApplyAudioSettings();
		ApplyWindowSettings();
	}

	// 查询指定关卡是否通关
	public static bool IsLevelCompleted(string levelId)
	{
		saveData.LevelCompletionStatus.TryGetValue(levelId, out var data);
		return data?.IsCompleted == true;
	}

	// 保存关卡通关状态
	public static void SaveLevelCompletion(LevelDefinitionResources levelDefinitionResources)
	{
		saveData.SaveLevelCompletion(levelDefinitionResources.Id, true);
		WriteSaveData();
	}

	// 保存音量状态
	public static void SaveVolume(string busName, float volume)
	{
		saveData.SaveVolume(busName, volume);
		WriteSaveData();
	}

	// 获取存档中的音量状态
	public static float GetVolume(string busName)
	{
		return saveData.GetVolume(busName);
	}

	// 保存全屏状态
	public static void SaveIsFullScreen(bool isFullScreen)
	{
		saveData.SaveIsFullScreen(isFullScreen);
		WriteSaveData();
	}

	// 获取存档中的全屏状态
	public static bool GetIsFullScreen()
	{
		return saveData.IsFullScreen;
	}

	// 将存档数据转换为 JSON 并写入文件
	private static void WriteSaveData()
	{
		var dataString = JsonConvert.SerializeObject(saveData);

		using var saveFile = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Write);
		saveFile.StoreLine(dataString);
	}

	// 从文件读取存档数据
	private static void LoadSaveData()
	{
		if (!FileAccess.FileExists(SAVE_FILE_PATH)) // 没有存档文件就直接返回
		{
			return;
		}

		// 读取存档文件内容
		using var saveFile = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Read);
		var dataString = saveFile.GetLine();

		try
		{
			saveData = JsonConvert.DeserializeObject<SaveData>(dataString); // 将 JSON 转换为 SaveData 对象
		}
		catch(Exception)
		{
			GD.PushWarning("Save file Json was corrupted."); // 存档损坏时输出警告
		}
	}

	// 恢复保存的音量设置
	private static void ApplyAudioSettings()
	{
		OptionsHelper.SetBusVolumnPercent(MUSIC_BUS_NAME, GetVolume(MUSIC_BUS_NAME));
		OptionsHelper.SetBusVolumnPercent(SFX_BUS_NAME, GetVolume(SFX_BUS_NAME));
	}

	// 恢复保存的窗口模式
	private static void ApplyWindowSettings()
	{
		if (OptionsHelper.IsFullScreen() != GetIsFullScreen())
		{
			OptionsHelper.ToggleWindowMode();
		}
	}
}
