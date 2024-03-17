public class PlayerData {
    public int Money { get; set; }
    public UpgradeData UpgradeData { get; set; }
    public QuestData QuestData { get; set; }
    public StockData StockData { get; set; }
}

public class UpgradeData {
    public Dictionary<UpgradeType, int> UpgradeLevels { get; set; }
    public List<int> UpgradeProductionLines { get; set; }
}

public class QuestData {
    public int CurrentQuestIndex { get; set; }
}

public class StockData {
    public Dictionary<DeliveryGoods, int> Stock { get; set; }
}