using Sandbox;
using System;
using Eryziac.CandyFactory;
using System.Security;
using System.Threading.Tasks;

[Category( "Candy Factory - interactable")]
public class Cooker : AInteractable
{
    [Property] public Conveyor conveyor { get; set; }
	[Property] public GameObject BoxPrefab { get; set; }
	[Property] public GameObject CookedObject { get; set; }
	[Property] private SoundEvent CookingSound { get; set; }
	[Property] private SoundEvent CookedSound { get; set; }
	private float cookTimer { get; set; }
	private float ReductionPercentage { get; set; } = 0.9f;
	private Vector3 boxOffset { get; set; } = new( 0, -20, -150 );
	private Vector3 cookedOffset { get; set; } = new( 0, -15, 80 );
	private SoundHandle sound;
	private FurnacePanel furnacePanel { get; set; }
	private SkinnedModelRenderer Renderer { get; set; }
	private ParticleBoxEmitter Smoke { get; set; }
	private ProductionSystem ProductionSystem { get; set; }
	private QuestSystem questSystem;
	private Settings Settings;


    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to put raw ingredients in {Name}";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
		furnacePanel = Components.Get<FurnacePanel>(FindMode.EnabledInSelfAndChildren);
		Renderer = Components.Get<SkinnedModelRenderer>();
		Smoke = Components.Get<ParticleBoxEmitter>(FindMode.EverythingInSelfAndChildren);
		ProductionSystem = GameObject.Root.Components.Get<ProductionSystem>(FindMode.EverythingInSelfAndChildren);
		Settings = Scene.GetAllComponents<Settings>().FirstOrDefault(x => !x.IsProxy);
    }
	protected override void OnUpdate()
    {
		furnacePanel ??= Components.Get<FurnacePanel>(FindMode.EnabledInSelfAndChildren);
		Renderer ??= Components.Get<SkinnedModelRenderer>();
		Smoke ??= Components.Get<ParticleBoxEmitter>(FindMode.EverythingInSelfAndChildren);
		ProductionSystem ??= GameObject.Root.Components.Get<ProductionSystem>(FindMode.EverythingInSelfAndChildren);
		Settings ??= Scene.GetAllComponents<Settings>().FirstOrDefault(x => !x.IsProxy);
    }

    public override async void OnInteract(GameObject interactor)
    {
        IsInteracted = true;
        var box = interactor.Components.Get<DeliveryGood>(FindMode.EverythingInSelfAndChildren);
        if (box == null) return;

		// Remove the ingredients from the box
		box.RemoveGoods(furnacePanel.GetIngredients());
		if (box.IsEmpty()) box.GameObject.Destroy();
		
		// Create a new box, animate, and place it in the cooker to be cooked
		var boxGo = BoxPrefab.Clone();
        boxGo.Transform.Position = Transform.Position+ Transform.Rotation.Forward * boxOffset + new Vector3(0,0,150);
		boxGo.Transform.Rotation *= new Angles( 0, 0, 180 );
		boxGo.Components.Get<Rigidbody>().Destroy();
		boxGo.Components.Get<Holdable>().Open();
		await GameTask.Delay( 3000 );

		conveyor.IsMoving = false;
		boxGo.Destroy();

		await Cook();

		if (box != null)
		{
			box.Enabled = true;
		}
        IsInteracted = false;
    }

	public override bool CanInteract(GameObject interactor)
	{
		// If the cooker is already cooking, return false
		if (IsInteracted) return false;

		// if the cooker is not cooking and the player is carrying a box, return true
		if (!interactor.Components.Get<PlayerInteract>().IsCarrying) return false;

		var box = interactor.Components.Get<DeliveryGood>(FindMode.EverythingInSelfAndChildren);
		if (!box.FromStock) return false;
		if (furnacePanel.CanCook(box.Goods)) return true;
		
		return false;
	}

	[Broadcast]
	private void CookingStarted(float time)
	{
		if ( CookingSound is not null )
		{
			CookingSound.Volume = Settings.GetVolume(VolumeType.Sound);
        	sound = Sound.Play( CookingSound, Transform.Position + Transform.Rotation.Forward * cookedOffset + new Vector3(0,0,80) );
		}
		furnacePanel.StartCooking(time);
		if ( Smoke is not null )
			Smoke.Enabled = true;
	}

	[Broadcast]
	private void CookingFinished()
	{
		Renderer.Set( "Opening", true );
		sound?.Stop();
		if ( CookedSound is not null )
		{
			CookedSound.Volume = Settings.GetVolume(VolumeType.Sound);
        	Sound.Play( CookedSound, Transform.Position + Transform.Rotation.Forward * cookedOffset + new Vector3(0,0,80));
		}
	}

	[Broadcast]
	private void CloseOven()
	{
		Renderer.Set( "Opening", false );
		Smoke.Enabled = false;
	}

	private async Task Cook()
    {
		conveyor.IsCooking = true;
        var cooked = CookedObject.Clone( Transform.Position + Transform.Rotation.Forward * cookedOffset + new Vector3(0,0,80));
        cooked.NetworkSpawn();
		var candy = cooked.Components.Get<Candies>();
		ProductionSystem ??= GameObject.Root.Components.Get<ProductionSystem>(FindMode.EverythingInSelfAndChildren);
        cookTimer = candy.CookingTime * (float)Math.Pow(ReductionPercentage, ProductionSystem.ProductionSpeed);
		CookingStarted(cookTimer);
        await GameTask.DelaySeconds( cookTimer );
		CookingFinished();
		questSystem ??= Scene.GetAllComponents<QuestSystem>().FirstOrDefault();
		if (questSystem is not null && questSystem.CurrentQuest is not null)
		{
			foreach (QuestObjective objective in questSystem.CurrentQuest.Objectives)
			{
			    if (objective.Type == ObjectiveType.Creation && cooked.Name.Contains(objective.ObjectTarget.Name))
			    {	
			        questSystem.Cooked(objective, cooked, 1);
			    }
			}
		}
		candy.GameObject.Tags.Add("cooked");
		conveyor.IsCooking = false;
		conveyor.IsMoving = true;
        await GameTask.DelaySeconds( 2 );
		CloseOven();
    }
}
