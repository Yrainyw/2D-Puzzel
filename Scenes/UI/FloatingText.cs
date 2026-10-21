using System;
using Godot;

namespace Game.UI;

public partial class FloatingText : Node2D
{
	public void SetText(String text)
	{
		GetNode<Label>("%Label").Text = text;
	}
}
