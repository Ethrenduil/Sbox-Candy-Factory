using Sandbox;
using Eryziac.CandyFactory;

public class Candy
{
    public string Name { get; set; }
    public int Quantity { get; set; }
	public int Current { get; set; }

	public Candy(string name, int quantity)
	{
		Name = name;
		Quantity = quantity;
		Current = 0;
	}
}

public class Needed
{
    public int Money { get; set; }
	public int CurrentMoney { get; set; }
    public Candy CandyCreated { get; set; }
    public Candy CandySold { get; set; }

	public Needed(int money, Candy candyCreated, Candy candySold)
	{
		CurrentMoney = 0;
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

	public PlayerTask()
	{
		Name = "Tutorial";
		Description = "Welcome in Candy Factory";
		IsComplete = false;
		Needed = new Needed(100, new Candy("Chocolate", 10), new Candy("Caramel", 10));
	}

	protected override void OnUpdate()
	{
		if (Needed.CurrentMoney >= Needed.Money && Needed.CandyCreated.Current >= Needed.CandyCreated.Quantity && Needed.CandySold.Current >= Needed.CandySold.Quantity)
		{
			IsComplete = true;
		}
	}
}
