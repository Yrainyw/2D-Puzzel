using Godot;
using Game.UI;

namespace Game.Autoload;

public partial class ScreenTransitionManager : Node
{
	private readonly PackedScene screenTransitionScene = GD.Load<PackedScene>("res://scenes/UI/ScreenTransition.tscn");

	public async void TransitionToScene(string scenePath)
	{
		var screenTransition = screenTransitionScene.Instantiate<ScreenTransition>();
		AddChild(screenTransition);

		// 等待 fade_in 完全黑屏
		await ToSignal(screenTransition, ScreenTransition.SignalName.ScreenCovered);
		
		// 切换到新关卡
		var result = GetTree().ChangeSceneToFile(scenePath);

		if (result != Error.Ok)
		{
			screenTransition.QueueFree();
			return;
		}

		// 等待新关卡初始化完成
		await ToSignal(GetTree(), SceneTree.SignalName.SceneChanged);

		// 播放 fade_out，显示新关卡
		var animationPlayer = screenTransition.GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.Play("fade_out");

		await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

		screenTransition.QueueFree();
	}
}