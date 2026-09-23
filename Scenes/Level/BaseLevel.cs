using Game.Manager;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{
    private GridManager gridManager;
    private GoldMine goldMine;
    
    public override void _Ready()
    {
        gridManager = GetNode<GridManager>("GridManager");
        goldMine = GetNode<GoldMine>("%GoldMine");
        gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void OnGridStateUpdated()
    {
        var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(goldMine.GlobalPosition);

        if (gridManager.IsTilePositionBuildable(goldMineTilePosition))
        {
            goldMine.SetActive();
            GD.Print("Win!");
        }
    }

}