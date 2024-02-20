using Sandbox;

public class Candy
{
    public string Name { get; set; }
    public int Quantity { get; set; }

	public Candy(string name, int quantity)
	{
		Name = name;
		Quantity = quantity;
	}
}

public class Needed
{
    public int Money { get; set; }
    public Candy CandyCreated { get; set; }
    public Candy CandySold { get; set; }

	public Needed(int money, Candy candyCreated, Candy candySold)
	{
		Money = money;
		CandyCreated = candyCreated;
		CandySold = candySold;
	}
}

public sealed class PlayerTask : Component
{
	public string Name { get; set; }
	public string Description { get; set; }
	public bool IsComplete { get; set; }
	public Needed Needed { get; set; }

	public PlayerTask(string name, string description)
	{
		Name = name;
		Description = description;
		IsComplete = false;
		Needed = new Needed(100, new Candy("Chocolate", 10), new Candy("Caramel", 10));
	}
	protected override void OnUpdate()
	{

	}
}
