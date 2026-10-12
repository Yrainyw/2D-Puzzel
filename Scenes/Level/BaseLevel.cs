using Game.Manager;
using Game.UI;
using Game.Resources.Level;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{
    [Export] private PackedScene levelCompleteScreenScene;
    [Export] private LevelDefinitionResources levelDefinitionResource;

    private GridManager gridManager;
    private GoldMine goldMine;
    private GameCamera gameCamera;
    private TileMapLayer baseTerrainTileMapLayer;
    private Node2D baseBuilding;
    private GameUI gameUI;
    private BuildingManager buildingManager;

    public override void _Ready()
    {
        gridManager = GetNode<GridManager>("GridManager");
        goldMine = GetNode<GoldMine>("%GoldMine");
        gameCamera = GetNode<GameCamera>("GameCamera");
        baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
        baseBuilding = GetNode<Node2D>("%Base");
        gameUI = GetNode<GameUI>("GameUI");
        buildingManager = GetNode<BuildingManager>("BuildingManager");

        buildingManager.SetStartingResourceCount(levelDefinitionResource.StartingResourceCount);

        gameCamera.SetBoundingRect(baseTerrainTileMapLayer.GetUsedRect());
        gameCamera.CenterOnPosition(baseBuilding.GlobalPosition);

        gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void OnGridStateUpdated()
    {
        var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(goldMine.GlobalPosition);

        if (gridManager.IsTilePositionInAnyBuildingRadius(goldMineTilePosition))
        {
            var levelCompleteScreen = levelCompleteScreenScene.Instantiate<LevelCompleteScreen>();
            
            AddChild(levelCompleteScreen);
            goldMine.SetActive();
            gameUI.HideUI();
        }
    }
}