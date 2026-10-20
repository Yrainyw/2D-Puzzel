using System;
using System.Data.Common;
using Game.Resources.Level;
using Godot;
using Newtonsoft.Json;

namespace Game.Autoload;

public partial class SaveManager : Node
{
	public static SaveManager Instance { get; private set; }
	private static SaveData saveData = new();
	private static readonly string SAVE_FILE_PATH = "user://save.json";

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			Instance = this;
			LoadSaveData();
		}
	}

	public static void SaveLevelCompletion(LevelDefinitionResources levelDefinitionResources)
	{
		saveData.SaveLevelCompletion(levelDefinitionResources.Id, true);
		WriteSaveData();
	}

	private static void WriteSaveData()
	{
		var dataString = JsonConvert.SerializeObject(saveData);

		using var saveFile = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Write);
		saveFile.StoreLine(dataString);
	}

	private static void LoadSaveData()
	{
		if (!FileAccess.FileExists(SAVE_FILE_PATH))
		{
			return;
		}

		using var saveFile = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Read);
		var dataString = saveFile.GetLine();

		try
		{
			saveData = JsonConvert.DeserializeObject<SaveData>(dataString);
		}
		catch(Exception)
		{
			GD.PushWarning("Save file Json was corrupted.");
		}
	}
}
