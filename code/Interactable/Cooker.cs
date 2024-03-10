using Sandbox;
using System;
using Eryziac.CandyFactory;

public class Cooker : AInteractable
{
    [Property] public Conveyor conveyor { get; set; }
	[Property] public GameObject CookedObject { get; set; }
	[Property] private SoundEvent CookingSound { get; set; }
	[Property] private SoundEvent CookedSound { get; set; }
	private float cookTimer { get; set; }
	private Vector3 boxOffset { get; set; } = new( 0, -20, 150 );
	private Vector3 cookedOffset { get; set; } = new( 0, -15, 80 );
	private SoundHandle sound;
	private FurnacePanel furnacePanel { get; set; }
	private SkinnedModelRenderer Renderer { get; set; }


    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to put raw ingredients in {Name}";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
		furnacePanel = Components.Get<FurnacePanel>(FindMode.EnabledInSelfAndChildren);
		Renderer = Components.Get<SkinnedModelRenderer>();
    }
	protected override void OnUpdate()
    {
    }

    public override async void OnInteract(GameObject interactor)
    {
        IsInteracted = true;
        var box = interactor.Components.Get<DeliveryGood>(FindMode.EverythingInSelfAndChildren);
        if (box == null) return;
		box.GameObject.Components.Get<AInteractable>().OnInteract(interactor);
        box.GameObject.Transform.Position = Transform.Position + boxOffset;
		box.GameObject.Transform.Rotation *= new Angles( 0, 0, 180 );
		box.GameObject.Components.Get<Rigidbody>().Destroy();
		box.GameObject.Components.Get<Holdable>().Open();
		await GameTask.Delay( 3000 );
		box.Destroy();
		conveyor.IsMoving = false;
		Cook();

        box.GameObject.Destroy();

        IsInteracted = false;
        
    }

	[Broadcast]
	private void CookingStarted(float time)
	{
		if ( CookingSound is not null )
        	sound = Sound.Play( CookingSound, Transform.Position + cookedOffset );
		furnacePanel.StartCooking(time);
	}

	[Broadcast]
	private void CookingFinished()
	{
		Renderer.Set( "Opening", true );
		sound?.Stop();
		if ( CookedSound is not null )
        	Sound.Play( CookedSound, Transform.Position + cookedOffset);
	}

	[Broadcast]
	private void CloseOven()
	{
		Renderer.Set( "Opening", false );
	}

	private async void Cook()
    {
        var cooked = CookedObject.Clone( Transform.Position + cookedOffset );
        cooked.NetworkSpawn();
		var candy = cooked.Components.Get<Candies>();
        cookTimer = candy.CookingTime;
		CookingStarted(cookTimer);
        await GameTask.DelaySeconds( cookTimer );
		CookingFinished();
		var currentTask = Scene.GetAllComponents<Player>().FirstOrDefault( x => !x.IsProxy ).CurrentTask;
		if ( currentTask is not null )
		{
			if ( currentTask.Needed.CandyCreated.Name == candy.Name && currentTask.Needed.CandyCreated.Current < currentTask.Needed.CandyCreated.Quantity) {
				currentTask.Needed.CandyCreated.Current++;
				Scene.GetAllComponents<CandyFactory>().FirstOrDefault().RefreshTaskHUD();
			}
		}
		conveyor.IsMoving = true;
        await GameTask.DelaySeconds( 2 );
		CloseOven();
    }
}
