using System;
using Eryziac.CandyFactory;
using System.Threading.Tasks;

[Category( "Candy Factory - interactable")]
public class Upgrader : AInteractable
{
    [Property] public Conveyor conveyor { get; set; }
	[Property] public GameObject upgradedObject { get; set; }
	[Property] private SoundEvent upgradeSound { get; set; }
	[Property] private SoundEvent upgradedSound { get; set; }
    [Property] private FurnacePanel furnacePanel { get; set; }
	[Property] public int UpgradeOrder { get; set; }
	[Property] public int UpgradePrice { get; set; }
	private float upgradeTimer { get; set; }
	private Vector3 upgradedOffset { get; set; } = new( 0, -15, 80 );
	private float ReductionPercentage { get; set; } = 0.9f;
	private ProductionSystem ProductionSystem { get; set; }
	private SoundHandle sound;
	private SkinnedModelRenderer Renderer { get; set; }
	private ParticleBoxEmitter Smoke { get; set; }
	private QuestSystem questSystem;


    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to upgrade your candy in {Name}";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
		Renderer = Components.Get<SkinnedModelRenderer>();
		Smoke = Components.Get<ParticleBoxEmitter>(FindMode.EverythingInSelfAndChildren);
		ProductionSystem = GameObject.Root.Components.Get<ProductionSystem>(FindMode.EverythingInSelfAndChildren);
    }
	protected override void OnUpdate()
    {
    }

    public override async void OnInteract(GameObject interactor)
    {
		if (IsInteracted)
			return;
        IsInteracted = true;

        if (conveyor.Candies.Count == 0)
        {
            IsInteracted = false;
            return;
        }
		
		conveyor.IsMoving = false;

		CloseUpgrader();

		await GameTask.Delay( 1000 );
		
		await Upgrade();
		
        IsInteracted = false; 
    }

	public override bool CanInteract(GameObject interactor)
	{
		// If the cooker is already cooking, return false
		if (IsInteracted) return false;

		// If the upgrader have at least one candie on the conveyor, return false
		if (conveyor.Candies.Count > 0) return true;

		return false;
	}

	[Broadcast]
	private void UpgradeStarted(float time)
	{
		if ( upgradeSound is not null )
        	sound = Sound.Play( upgradeSound, Transform.Position + upgradedOffset );
		furnacePanel.StartCooking(time);
		if ( Smoke is not null )
			Smoke.Enabled = true;
	}

	[Broadcast]
	private void UpgradeFinished()
	{
		Renderer.Set( "Closing", false );
		sound?.Stop();
		if ( upgradedSound is not null )
        	Sound.Play( upgradedSound, Transform.Position + upgradedOffset);
		
		if ( Smoke is not null )
			Smoke.Enabled = false;
	}

	[Broadcast]
	private void CloseUpgrader()
	{
		Renderer.Set( "Closing", true );
	}

	private async Task Upgrade()
    {
		var candyName = "";
		var candyNumber = 0;
		upgradeTimer = 0;
		GameObject upgraded = null;
        foreach (var candy in conveyor.Candies.ToList())
    	{
			if ( candy.Name.Contains(upgradedObject.Name) )
				continue;
    	    candy.Destroy();
    	    conveyor.RemoveCandy(candy);
    	    upgraded = upgradedObject.Clone(Transform.Position + upgradedOffset);
    	    upgraded.NetworkSpawn();
			var temp = upgraded.Components.Get<Candies>();
        	upgradeTimer += temp.CookingTime;
			candyNumber++;
			if ( candyName == "" )
				candyName = temp.Name;
		}
		if ( upgradeTimer == 0 )
			return;
		UpgradeStarted(upgradeTimer);
        await GameTask.DelaySeconds( upgradeTimer );
		UpgradeFinished();
		questSystem ??= Scene.GetAllComponents<QuestSystem>().FirstOrDefault();
		if (questSystem.CurrentQuest is not null)
		{
			foreach (QuestObjective objective in questSystem.CurrentQuest.Objectives)
			{
			    if (objective.Type == ObjectiveType.Creation && objective.ObjectTarget == upgraded)
			    {
			        questSystem.Cooked(objective, upgraded, candyNumber);
			    }
			}
		}
		
    	upgraded.Tags.Add("Upgraded");
		conveyor.IsCooking = false;
		conveyor.IsMoving = true;
    }
}
