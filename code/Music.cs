using Sandbox;

public sealed class Music : Component
{
    public MusicPlayer MP { get; set; }

	protected override void OnAwake()
	{
		MP = MusicPlayer.Play(FileSystem.Mounted , "music/menu.mp3");
        MP.Volume = 10f;
	}

	protected override void OnDestroy()
	{
		MP.Stop();
	}
}
