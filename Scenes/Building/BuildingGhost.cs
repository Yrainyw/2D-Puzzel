using Godot;

namespace Game.Building;

public partial class BuildingGhost : Node2D
{
	private Node2D topLeft;
	private Node2D topRight;
	private Node2D bottomLeft;
	private Node2D bottomRight;

    public override void _Ready()
    {
        topLeft = GetNode<Node2D> ("TopLeft");
		topRight = GetNode<Node2D>("TopRight");
		bottomLeft = GetNode<Node2D>("BottomLeft");
		bottomRight = GetNode<Node2D>("BottomRight");
	}
	
	public void SetInvalid()
	{
		Modulate = Colors.Red;
	}

	public void SetValid()
	{
		Modulate = Colors.White;
	}

	public void SetDemensions(Vector2I dimensions)
	{
		bottomLeft.Position = dimensions * new Vector2I(0, 64);
		bottomRight.Position = dimensions * new Vector2I(64, 64);
		topRight.Position = dimensions * new Vector2I(64, 0);
	}
}
