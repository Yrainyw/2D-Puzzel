using Godot;
using Newtonsoft.Json;

namespace Game.Autoload;

public partial class SaveManager : Node
{
	public static SaveManager Instance { get; private set; }
	
	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			Instance = this;
		}
	}

    public override void _Ready()
    {
        // var saveData = new SaveData();

		// saveData.SaveLevelCompletion("random_id", true);

		// var dataString = JsonConvert.SerializeObject(saveData);
		var saveData = JsonConvert.DeserializeObject<SaveData>("{\"LevelCompletionStatus\" : {\"random_id\" : {\"IsCompleted\" : true}}}");

		foreach(var key in saveData.LevelCompletionStatus.Keys)
		{
			GD.Print(key);
			GD.Print(saveData.LevelCompletionStatus[key].IsCompleted);
		}

    }
}
