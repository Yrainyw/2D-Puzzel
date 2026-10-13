using System.Linq;
using Game.Resources.Level;
using Godot;

namespace Game.Autoload;

public partial class LevelManager : Node
{
	public static LevelManager Instance { get; private set; }
	
	[Export] private LevelDefinitionResources[] levelDefinitions;

	private int currentLevelIndex;

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			Instance = this;
		}
	}

	public static LevelDefinitionResources[] GetLevelDefinitions()
	{
		return Instance.levelDefinitions.ToArray();
	}

	public void ChangeToLevel(int levelIndex)
	{
		if (levelIndex >= levelDefinitions.Length || levelIndex < 0) return;

		currentLevelIndex = levelIndex;

		var levelDefinition = levelDefinitions[currentLevelIndex];

		GetTree().ChangeSceneToFile(levelDefinition.LevelScenePath);
	}

	public void ChangeToNextLevel()
	{
		ChangeToLevel(currentLevelIndex + 1);
	}
}
