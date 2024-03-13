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
    public Candy CandyUpgraded { get; set; }
	public bool TalkToBob { get; set; }
	public bool Talked { get; set; }
	public Needed(int money, Candy candyCreated, Candy candyUpgraded, bool talkToBob)
	{
		CurrentMoney = 0;
		Money = money;
		CandyCreated = candyCreated;
		CandyUpgraded = candyUpgraded;
		TalkToBob = talkToBob;
		Talked = false;
	}
}

public sealed class PlayerTask : Component
{
	public string Name { get; set; }
	public string Description { get; set; }
	public bool IsComplete { get; set; }
	public Needed Needed { get; set; }
	public string CompletedMessage { get; set; }
	public bool ShowArrow { get; set; } = false;
	public GameObject Arrow { get; set; }
	public GameObject ArrowTarget { get; set; }
	private CandyFactory candyFactory;

	public PlayerTask()
	{
		Name = "Introduction";
		Description = "Welcome in Candy Factory !";
		IsComplete = false;
		Needed = new Needed(0, null, null, true);
		CompletedMessage = "You have completed the introduction !";
		ShowArrow = true;
	}

	protected override void OnStart()
	{
		base.OnStart();
		var arrowComponent = Scene.GetAllComponents<FollowTask>().FirstOrDefault();	
		Arrow = Scene.GetAllComponents<FollowTask>().FirstOrDefault().GameObject;
		ArrowTarget = Scene.GetAllComponents<Bob>().FirstOrDefault().GameObject;
		arrowComponent.startPosition = ArrowTarget.Transform.Position + new Vector3(0, 0, 50);
		arrowComponent.endPosition = ArrowTarget.Transform.Position + new Vector3(0, 0, 80);
		Arrow.SetParent(ArrowTarget);
		candyFactory = Scene.GetAllComponents<CandyFactory>().FirstOrDefault();
	}

	protected override void OnUpdate()
	{
		if (IsComplete)
		{
			NextTask();
		}
		else
		{
			CheckIfComplete();
		}
	}

	private void NextTask()
	{
		Name = "Tutorial";
		Description = "First steps in the factory !";
		IsComplete = false;
		Needed = new Needed(100, new Candy("Dark Chocolate", 10), new Candy("White Chocolate Cone", 10), false);
		CompletedMessage = "You have completed the tutorial ! Thanks for playing ! New updates are coming soon!";
		ShowArrow = false;
		Arrow.Enabled = false;
		candyFactory.RefreshTaskHUD();
	}

	private void CheckIfComplete()
	{
		if (Needed.TalkToBob)
		{
			if (Needed.Talked)
			{
				IsComplete = true;
				candyFactory.RefreshTaskHUD();
			}
		}
		else if (Needed.Money <= Needed.CurrentMoney && Needed.CandyCreated.Quantity <= Needed.CandyCreated.Current && Needed.CandyUpgraded.Quantity <= Needed.CandyUpgraded.Current)
		{
			IsComplete = true;
			candyFactory.RefreshTaskHUD();
		}
	}
}
