using Sandbox;

[Title( "Settings" )]
[Category( "Candy Factory" )]
public sealed class Settings : Component
{
    [Property] public float Volume { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
        Volume = 0.5f;
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
        data.Volume = Volume;

        // Load settings
        if(fs.FileExists("settings.json"))
        {
            // If settings file exists, load it
            data = fs.ReadJson<SettingsData>("settings.json");

            // Load all settings
            Volume = data.Volume;
        }
        else
        {
            // If settings file does not exist, create it and save default settings
            fs.WriteJson("settings.json", data);
        }
        return;
    }

    public void SetVolume(float volume)
    {
        Volume = volume;
        SaveSettings();
    }

    public void SaveSettings()
    {
        var fs = FileSystem.Data;
        var data = new SettingsData();
        data.Volume = Volume;

        fs.WriteJson("settings.json", data);
    }

    public string GetStringVolume()
    {
        return Volume.ToString("0") + "%";
    }
}
