using Sandbox;
using System;
using Eryziac.CandyFactory;

[Category( "Candy Factory - interactable")]
public class Seller : AInteractable, Component.ICollisionListener
{
	[Property] public SoundEvent sellSound { get; set; }
	private SoundHandle sound;
	private ParticleBoxEmitter Money { get; set; }
	[Property]public List<Candies> Candies { get; set; } = new();
	private QuestSystem questSystem;
	private Settings Settings;



    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to sell your candies";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
		Money = Components.Get<ParticleBoxEmitter>(FindMode.EverythingInSelfAndChildren);
		Settings = Scene.GetAllComponents<Settings>().FirstOrDefault(x => !x.IsProxy);
    }
	protected override void OnUpdate()
    {
		Settings ??= Scene.GetAllComponents<Settings>().FirstOrDefault(x => !x.IsProxy);
    }

	[Broadcast]
	private void SellFinished()
	{
		if ( sellSound is not null )
		{
			sellSound.Volume = Settings.VolumeSound;
			sound = Sound.Play( sellSound, Transform.Position );
		}
	}

	[Broadcast]
	private void RemoveCandy()
	{
		Candies.RemoveAt(0);
	}

    public override void OnInteract(GameObject interactor)
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
		questSystem ??= Scene.GetAllComponents<QuestSystem>().FirstOrDefault();

		foreach (var candy in Candies)
		{
			if (candy is not null)
			{
				player.AddMoney(candy.Price);
				candy.GameObject.Destroy();
				RemoveCandy();

				if (questSystem.CurrentQuest is not null)
				{
					foreach (QuestObjective objective in questSystem.CurrentQuest.Objectives)
					{
						if (objective.Type == ObjectiveType.EarnMoney)
						{
							questSystem.EarnedMoney(objective, candy.Price);
						}
					}
				}
			}
		}
		SellFinished();
        IsInteracted = false; 
    }

	public override bool CanInteract(GameObject interactor)
	{
		if (IsInteracted) return false;

		if (Candies.Count > 0) return true;

		return false;
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
