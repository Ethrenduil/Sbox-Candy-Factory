using Sandbox;

public enum ObjectiveType
{
    Interaction,
    Creation,
    ResourceOrdering,
    FactoryUpgrade,
    EarnMoney
}

[Category( "Candy Factory - Quests")]
public class QuestObjective
{
    public ObjectiveType Type { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public int TargetAmount { get; set; }
    public int CurrentAmount { get; set; }
    public GameObject ObjectTarget { get; set; }

    public QuestObjective(ObjectiveType type, string description)
    {
        Type = type;
        Description = description;
        IsCompleted = false;
    }

    public QuestObjective(ObjectiveType type, string description, GameObject gameObject)
    {
        Type = type;
        Description = description;
        ObjectTarget = gameObject;
        IsCompleted = false;
    }

    public QuestObjective(ObjectiveType type, string description, int targetAmount, GameObject gameObject)
    {
        Type = type;
        Description = description;
        TargetAmount = targetAmount;
        CurrentAmount = 0;
        ObjectTarget = gameObject;
        IsCompleted = false;
    }

    public QuestObjective(ObjectiveType type, string description, int targetAmount)
    {
        Type = type;
        Description = description;
        TargetAmount = targetAmount;
        CurrentAmount = 0;
        IsCompleted = false;
    }

}

