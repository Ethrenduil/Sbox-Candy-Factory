using Sandbox;

[Category( "Candy Factory - Quests")]
public sealed class QuestSystem : Component
{
    [Property] public List<GameObject> Candies { get; set; } = new();
	public List<Quest> Quests { get; private set; } = new();
    public Quest CurrentQuest { get; private set; }
    public int CurrentQuestIndex { get; private set; } = 0;

    protected override void OnStart()
    {
        base.OnStart();
        Quest quest1 = new Quest("Introduction", "Welcome to Candy Factory!");
        var bob = Scene.GetAllComponents<Bob>().FirstOrDefault().GameObject;
        quest1.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Talk to Bob", bob));
        Quests.Add(quest1);

        Quest quest2 = new Quest("Tutorial", "Buy the Candy Factory");
        var vendingPanel = Scene.GetAllComponents<Buyable>().FirstOrDefault().GameObject;
        quest2.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Interact with the vending panel", vendingPanel));
        Quests.Add(quest2);

        Quest quest3 = new Quest("Tutorial", "Meet Bob at the Candy Factory");
        quest3.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Talk to Bob", bob));
        Quests.Add(quest3);

        Quest quest4 = new Quest("Tutorial", "Ask Bob for a delivery");
        quest4.Objectives.Add(new QuestObjective(ObjectiveType.ResourceOrdering, "Make an delivery order"));
        Quests.Add(quest4);

        Quest quest5 = new Quest("Tutorial", "Deliver the order to the Candy Factory");
        var stockarea = Scene.GetAllComponents<Stockable>().FirstOrDefault().GameObject;
        quest5.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Deliver the order to the Candy Factory", stockarea));
        Quests.Add(quest5);

        Quest quest6 = new Quest("Tutorial", "Make dark chocolate");
        quest6.Objectives.Add(new QuestObjective(ObjectiveType.Creation, "Craft dark chocolate", 1, Candies[0]));
        Quests.Add(quest6);

        Quest quest7 = new Quest("Tutorial", "Sell the dark chocolate");
        var vendingBox = Scene.GetAllComponents<Seller>().FirstOrDefault().GameObject;
        quest7.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Interact with the vending box", vendingBox));
        Quests.Add(quest7);
    }
}
