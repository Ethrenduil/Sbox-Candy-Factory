using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;

[Category("Player")]
[Title ("Player Interact")]
public class PlayerInteract : Component
{	
	[Category("Parameters")]
	[Property]
	[Range( 0f, 1000f, 50f )]
	public float InteractDistance {get ; set; } = 150.0f;

	Player PlayerComponent;
	public bool IsInteracting = false;

	[Sync] public bool IsCarrying { get; set; } = false;
	public Interact interactHud;
	protected float InteractionTime = 0.0f;
    protected const float InteractionCooldown = 0.5f;

	protected override void OnStart()
	{
		base.OnStart();
		PlayerComponent = GameObject.Components.Get<Player>(FindMode.EnabledInSelf);
		interactHud = Scene.GetAllComponents<Interact>().FirstOrDefault();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (IsProxy)
			return;
		var position = GameObject.Transform.Position;
		position.z += 50;

		// If the plyaer has interacted and the cooldown has passed, reset the interaction state
		IsInteracting = IsInteracting && !InteractionCooldownPassed();

		SceneTraceResult collisionResult = Scene.Trace
				.Ray(position, position + PlayerComponent.Camera.Transform.Rotation.Forward * InteractDistance)
				.WithTag("interactable")
				.Run();
		if (collisionResult.Hit)
		{
			AInteractable interactable = collisionResult.GameObject.Components.Get<AInteractable>();
			interactHud.SetValue(interactable.Description);
			if (Input.Pressed("use") && !IsInteracting)
			{
				if (interactable.Type == InteractableType.Resource && IsCarrying) return;
				StartInteract();
				interactable?.OnInteract(GameObject);
				interactHud.SetValue(null);
				IsCarrying = interactable.Type == InteractableType.Resource || IsCarrying;
			}
		} else
		{
			interactHud.SetValue(null);

			// If the player is carrying an object and the use button is pressed, drop the object
			if (Input.Pressed("use") && !IsInteracting && IsCarrying)
			{
				GameObject.Children.FirstOrDefault(x => x.Tags.Has("interactable")).Components.Get<AInteractable>().OnInteract(GameObject);
				IsCarrying = false;
			}
		}
	}

	public void StartInteract()
	{
		IsInteracting = true;
		InteractionTime = Time.Now;
	}
	public bool InteractionCooldownPassed()
	{
		return Time.Now - InteractionTime > InteractionCooldown;
	}
}