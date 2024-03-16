using Sandbox;

public sealed class Music : Component
{
    public MusicPlayer MP { get; set; }
	[Property] public float Volume { get; set; }

	[Property] public Settings Settings { get; set; }

	protected override void OnAwake()
	{
		MP = MusicPlayer.Play(FileSystem.Mounted , "music/menu.mp3");
		MP.Repeat = true;
        MP.Volume = Settings.VolumeMusic;
	}

	protected override void OnDestroy()
	{
		MP.Stop();
	}

	protected override void OnUpdate()
	{
		if (MP != null && !MP.Paused)
		{
			MP.Volume = Settings.VolumeMusic;
		}
	}
}
