using System.Collections.Generic;
using System.Linq;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class ResourceIndicatorManager : Node
{
	[Export] private GridManager gridManager;
	[Export] PackedScene resourceIndicatorScene;

	private HashSet<Vector2I> indicatedTiles = new();
	private Dictionary<Vector2I, ResourceIndicator> tileToResourceIndicator = new();
	private AudioStreamPlayer audioStreamPlayer;

	public override void _Ready()
	{
		audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
	}

	private void UpdateIndicators(IEnumerable<Vector2I> newIndicatorTiles,IEnumerable<Vector2I> toRemoveTiles)
	{
		if (newIndicatorTiles.Any())
		{
			audioStreamPlayer.Play();
		}
		
		foreach(var newTile in newIndicatorTiles)
		{
			var indicator = resourceIndicatorScene.Instantiate<ResourceIndicator>();
			
			AddChild(indicator);
			indicator.GlobalPosition = newTile * 64;
			tileToResourceIndicator[newTile] = indicator;
		}

		foreach(var removeTile in toRemoveTiles)
		{
			tileToResourceIndicator.TryGetValue(removeTile, out var indicator);

			if (IsInstanceValid(indicator))
			{
				indicator.Destroy();
			}
			tileToResourceIndicator.Remove(removeTile);
		}
	}

	private void HandleResourceTileUpdated()
	{
		var currentResourceTiles = gridManager.GetCollectedResourceTiles();
		var newlyIndicatedTiles = currentResourceTiles.Except(indicatedTiles);
		var toRemovedtiles = indicatedTiles.Except(currentResourceTiles);

		indicatedTiles = currentResourceTiles;
		UpdateIndicators(newlyIndicatedTiles, toRemovedtiles);
	}

	private void OnResourceTilesUpdated(int _)
	{
		Callable.From(HandleResourceTileUpdated).CallDeferred();
	}
}
