using Sandbox;

[Category( "Candy Factory - Quests")]
public sealed class Quest : Component
{
	public string Name { get; set; }
    public string Description { get; set; }
    public List<QuestObjective> Objectives { get; set; }
    public bool IsCompleted { get; set; }
	public bool ShowArrow { get; set; } = false;
	public GameObject Arrow { get; set; }
	public GameObject ArrowTarget { get; set; }

    public Quest(string name, string description)
    {
        Name = name;
        Description = description;
        Objectives = new List<QuestObjective>();
        IsCompleted = false;
    }
	
}
