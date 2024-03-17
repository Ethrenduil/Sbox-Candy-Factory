using System.IO;
using System.Text.Json;
using System.Text;
using System;

public static class SaveSystem 
{
    public static void SetupSaveFolder()
    {
        var fs = FileSystem.Data;
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
            QuestData = player.GetQuestData(),
            UpgradeData = player.GetUpgradeData(),
            StockData = player.GetStockData()
        };

        string jsonString = JsonSerializer.Serialize<PlayerData>(data);
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
            PlayerData data = JsonSerializer.Deserialize<PlayerData>(info);
            return data;
        }
        else
        {
            return null;
        }
    }

}
