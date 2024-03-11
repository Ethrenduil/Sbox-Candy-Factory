using Sandbox;
using System;
using Eryziac.CandyFactory;

[Category( "Candy Factory - interactable")]
public class Seller : AInteractable, Component.ICollisionListener
{
	[Property] public SoundEvent sellSound { get; set; }
    [Property] private SellerPanel sellerPanel { get; set; }
	private SoundHandle sound;
	private ParticleBoxEmitter Money { get; set; }
	[Property]public List<Candies> Candies { get; set; } = new();



    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to sell your candies";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
		Money = Components.Get<ParticleBoxEmitter>(FindMode.EverythingInSelfAndChildren);
    }
	protected override void OnUpdate()
    {
    }

	[Broadcast]
	private void SellStarted(float time)
	{
		sellerPanel.StartSelling(time);
	}

	[Broadcast]
	private void SellFinished()
	{
		if ( sellSound is not null )
		{
			sound = Sound.Play( sellSound, Transform.Position );
		}
	}

	[Broadcast]
	private void RemoveCandy()
	{
		Candies.RemoveAt(0);
	}

    public async override void OnInteract(GameObject interactor)
    {
		if (IsInteracted)
			return;
        IsInteracted = true;

		if (Candies.Count == 0)
		{
			IsInteracted = false;
			return;
		}
		var player = interactor.Components.Get<Player>();
		var moneyEarning = 0;
		foreach (var candy in Candies.ToList())
		{
			player.AddMoney(candy.Price);
			moneyEarning += candy.Price;
			SellStarted(candy.SellingTime);
			await GameTask.DelaySeconds(candy.SellingTime);
			SellFinished();
			candy.GameObject.Destroy();
			RemoveCandy();
		}
		var currentTask = player.CurrentTask;
		if ( currentTask is not null )
		{
			if ( currentTask.Needed.CurrentMoney < currentTask.Needed.Money) {
				currentTask.Needed.CurrentMoney += moneyEarning;
				currentTask.Needed.CurrentMoney = Math.Min(currentTask.Needed.CurrentMoney, currentTask.Needed.Money);
				Scene.GetAllComponents<CandyFactory>().FirstOrDefault().RefreshTaskHUD();
			}
		}
        IsInteracted = false; 
    }

	public virtual void OnCollisionStart(Collision o)
    {
        var gameObject = o.Other.GameObject.Root;
		

        if (!gameObject.Tags.Has("Candy"))
			return;

		Candies.Add(gameObject.Components.Get<Candies>());
    }

    public void OnCollisionStop( CollisionStop o )
    {
		 var gameObject = o.Other.GameObject.Root;
		

        if (!gameObject.Tags.Has("Candy"))
			return;
		Candies.Remove(o.Other.GameObject.Components.Get<Candies>());
    }

    public void OnCollisionUpdate(Collision o)
    {
    }

}
