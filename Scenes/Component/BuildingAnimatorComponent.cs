using System.Linq;
using Godot;

namespace Game.Component;

public partial class BuildingAnimatorComponent : Node2D
{
	[Signal]
	public delegate void DestroyAnimationFinishedEventHandler();
	
	private Tween activeTween;
	private Node2D animationRootNode;
	
	public override void _Ready()
	{
		SetUpNodes();
	}

	public void PlayInAnimation()
	{
		if (animationRootNode == null) return;
		
		if (activeTween != null && activeTween.IsValid())
		{
			activeTween.Kill();
		}

		activeTween = CreateTween();
		activeTween.TweenProperty(animationRootNode, "position", Vector2.Zero, .3).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In)
			.From(Vector2.Up * 128);
		activeTween.TweenProperty(animationRootNode, "position", Vector2.Up * 16, .1).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		activeTween.TweenProperty(animationRootNode, "position", Vector2.Zero, .1).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
	}

	public void PlayDestroyAnimation()
	{
		if (animationRootNode == null) return;

		if (activeTween != null && activeTween.IsValid())
		{
			activeTween.Kill();
		}

		activeTween = CreateTween();
		activeTween.TweenProperty(animationRootNode, "rotation_degrees", -5, .1);
		activeTween.TweenProperty(animationRootNode, "rotation_degrees", 5, .1);
		activeTween.TweenProperty(animationRootNode, "rotation_degrees", -2, .1);
		activeTween.TweenProperty(animationRootNode, "rotation_degrees", 2, .1);
		activeTween.TweenProperty(animationRootNode, "rotation_degrees", 0, .1);
		activeTween.TweenProperty(animationRootNode, "position", Vector2.Down * 300, .4).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.In);

		activeTween.Finished += () => 
		{ 
			EmitSignal(SignalName.DestroyAnimationFinished); 
		};
	}

	private void SetUpNodes()
	{
		var spriteNode = GetChildren().FirstOrDefault() as Node2D; 

		if (spriteNode == null)
		{
			return;
		}

		RemoveChild(spriteNode);
		Position = new Vector2(spriteNode.Position.X, spriteNode.Position.Y);
		animationRootNode = new Node2D();
		AddChild(animationRootNode);
		animationRootNode.AddChild(spriteNode);
		spriteNode.Position = new Vector2(0, 0);
	}
}
