using Sandbox;
using System;
using Eryziac.CandyFactory;

public class Upgrader : AInteractable
{
    [Property] public Conveyor conveyor { get; set; }
	[Property] public GameObject upgradedObject { get; set; }
	[Property] private SoundEvent upgradeSound { get; set; }
	[Property] private SoundEvent upgradedSound { get; set; }
    [Property] private FurnacePanel furnacePanel { get; set; }
	private float upgradeTimer { get; set; }
	private Vector3 upgradedOffset { get; set; } = new( 0, -15, 80 );
	private SoundHandle sound;
	private SkinnedModelRenderer Renderer { get; set; }


    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to upgrade your candy in {Name}";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
		Renderer = Components.Get<SkinnedModelRenderer>();
    }
	protected override void OnUpdate()
    {
    }

    public override async void OnInteract(GameObject interactor)
    {
        IsInteracted = true;

        if (conveyor.Candies.Count == 0)
        {
            IsInteracted = false;
            return;
        }
		
		conveyor.IsMoving = false;

		CloseUpgrader();

		await GameTask.Delay( 1000 );
		
		Upgrade();


        IsInteracted = false;
        
    }

	[Broadcast]
	private void UpgradeStarted()
	{
		if ( upgradeSound is not null )
        	sound = Sound.Play( upgradeSound, Transform.Position + upgradedOffset );
		furnacePanel.StartCooking(upgradeTimer);
	}

	[Broadcast]
	private void UpgradeFinished()
	{
		Renderer.Set( "Closing", false );
		sound?.Stop();
		if ( upgradedSound is not null )
        	Sound.Play( upgradedSound, Transform.Position + upgradedOffset);
	}

	[Broadcast]
	private void CloseUpgrader()
	{
		Renderer.Set( "Closing", true );
	}

	private async void Upgrade()
    {
        conveyor.Candies.First().Destroy();
        conveyor.Candies.RemoveAt( 0 );
        var upgraded = upgradedObject.Clone( Transform.Position + upgradedOffset );
		upgraded.Tags.Add( "Upgraded" );
        upgraded.NetworkSpawn();
		var candy = upgraded.Components.Get<Candies>();
        upgradeTimer = candy.CookingTime;
		UpgradeStarted();
        await GameTask.DelaySeconds( upgradeTimer );
		UpgradeFinished();
		var currentTask = Scene.GetAllComponents<Player>().FirstOrDefault( x => !x.IsProxy ).CurrentTask;
		if ( currentTask is not null )
		{
			if ( currentTask.Needed.CandyUpgraded.Name == candy.Name && currentTask.Needed.CandyUpgraded.Current < currentTask.Needed.CandyUpgraded.Quantity) {
				currentTask.Needed.CandyUpgraded.Current++;
				Scene.GetAllComponents<CandyFactory>().FirstOrDefault().RefreshTaskHUD();
			}
		}
		conveyor.IsMoving = true;
    }
}
