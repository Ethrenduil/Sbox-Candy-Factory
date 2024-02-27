using System.IO;
using System.Text.Json;
using System.Text;
using System;

public static class SaveSystem {

    public static void SetupSaveFolder()
    {
        var fs = FileSystem.Data;
        Log.Info(fs.FindDirectory("cdySaves"));
        if (fs.DirectoryExists("cdySaves"))
            return;
        fs.CreateDirectory("cdySaves");
    }

    public static void SavePlayer(Player player)
    {
        SetupSaveFolder();

        var fs = FileSystem.Data;
        PlayerData data = new()
		{
            Money = player.GetMoney(),
            CurrentTask = player.CurrentTask.Name
        };

        string jsonString = JsonSerializer.Serialize<PlayerData>(data);
        Log.Info(jsonString);
        fs.WriteAllText("cdySaves/player.json", jsonString);
        Log.Info("Player saved");
        return;
    }

    public static PlayerData LoadPlayer()
    {
        string path = "cdySaves/player.json";
        var fs = FileSystem.Data;
        if (fs.FileExists(path))
        {
            Log.Info("Loading player data");
            var info = fs.ReadAllText(path);
            Log.Info(info);
            PlayerData data = JsonSerializer.Deserialize<PlayerData>(info);
            return data;
        }
        else
        {
            return null;
        }
    }

}