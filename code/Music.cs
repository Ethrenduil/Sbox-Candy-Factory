using Sandbox;

public sealed class Music : Component
{
    public MusicPlayer MP { get; set; }
	[Property] public float Volume { get; set; } = 5f;

	protected override void OnAwake()
	{
		MP = MusicPlayer.Play(FileSystem.Mounted , "music/menu.mp3");
        MP.Volume = Volume;
	}

	protected override void OnDestroy()
	{
		MP.Stop();
	}

	protected override void OnUpdate()
	{
		if (MP != null && !MP.Paused)
		{
			MP.Volume = Volume;
		}
	}
}
