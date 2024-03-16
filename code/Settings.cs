using Sandbox;

[Title( "Settings" )]
[Category( "Candy Factory" )]
public sealed class Settings : Component
{
    [Property] public float VolumeMusic { get; set; }
    [Property] public float VolumeSound { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
        VolumeMusic = 0.5f;
        VolumeSound = 0.5f;
        SetUpSettings();
	}
	protected override void OnUpdate()
	{
		base.OnUpdate();		
	}

	protected override void OnAwake()
	{	
		base.OnAwake();
	}

    private void SetUpSettings()
    {
        var fs = FileSystem.Data;
        var data = new SettingsData();
        data.VolumeMusic = VolumeMusic;
        data.VolumeSound = VolumeSound;

        // Load settings
        if(fs.FileExists("settings.json"))
        {
            // If settings file exists, load it
            data = fs.ReadJson<SettingsData>("settings.json");

            // Load all settings
            VolumeMusic = data.VolumeMusic;
            VolumeSound = data.VolumeSound;
        }
        else
        {
            // If settings file does not exist, create it and save default settings
            fs.WriteJson("settings.json", data);
        }
        return;
    }

    public void SetVolume(VolumeType type, float volume)
    {
        if(type == VolumeType.Music)
            VolumeMusic = volume;
        else
            VolumeSound = volume;
        SaveSettings();
    }

    public void SaveSettings()
    {
        var fs = FileSystem.Data;
        var data = new SettingsData();
        data.VolumeMusic = VolumeMusic;
        data.VolumeSound = VolumeSound;

        fs.WriteJson("settings.json", data);
    }

    public string GetStringVolume(VolumeType type)
    {
        if(type == VolumeType.Music)
            return VolumeMusic.ToString("0") + "%";
        else
            return VolumeSound.ToString("0") + "%";
    }
}


public enum VolumeType
{
    Music,
    Sound
}