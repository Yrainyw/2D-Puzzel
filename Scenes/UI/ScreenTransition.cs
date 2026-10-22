using Godot;

namespace Game.UI;

public partial class ScreenTransition : CanvasLayer
{
	[Signal]
	public delegate void ScreenCoveredEventHandler();

	public void EmitScreenCovered()
	{
		EmitSignal(SignalName.ScreenCovered);
	}
}