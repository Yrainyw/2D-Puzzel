using System.Linq;
using Godot;

namespace Game.Component;

public partial class BuildingAnimatorComponent : Node2D
{
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

	private void SetUpNodes()
	{
		var spriteNode = GetChildren().FirstOrDefault() as Node2D; 

		if (spriteNode == null)
		{
			return;
		}

		RemoveChild(spriteNode);
		Position = new Vector2(Position.X, spriteNode.Position.Y);
		animationRootNode = new Node2D();
		AddChild(animationRootNode);
		animationRootNode.AddChild(spriteNode);
		spriteNode.Position = new Vector2(spriteNode.Position.X, 0);
	}
}
