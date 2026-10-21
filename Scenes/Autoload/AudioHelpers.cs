using Godot;

namespace Game.Autoload;

public partial class AudioHelpers : Node
{
	private static AudioHelpers instance;
	private AudioStreamPlayer explosionAudioStreamPlayer;

    public override void _Notification(int what)
    {
       if (what == NotificationSceneInstantiated)
		{
		   instance = this;
		}
    }

    public override void _Ready()
    {
       explosionAudioStreamPlayer = GetNode<AudioStreamPlayer>("ExplosionAudioStreamPlayer");
    }

	public static void PlayBuildingDestruction()
	{
		instance.explosionAudioStreamPlayer.Play();
	}
}
