using Godot;

namespace Game.Resources.Level;

[GlobalClass]
public partial class LevelDefinitionResources : Resource
{
    [Export] public string Id { get; private set; }

    [Export] public int StartingResourceCount { get; private set; } = 4;

    [Export(PropertyHint.File, "*.tscn")] public string LevelScenePath { get; private set; }
}
