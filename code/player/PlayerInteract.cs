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
	[Property] public bool IsInteracting = false;

	[Property] [Sync] public bool IsCarrying { get; set; }
	public Interact interactHud;
	protected float InteractionTime = 0.0f;
    protected const float InteractionCooldown = 0.5f;

	protected override void OnStart()
	{
		base.OnStart();
		PlayerComponent = GameObject.Components.Get<Player>(FindMode.EnabledInSelf);
		interactHud = Scene.GetAllComponents<Interact>().FirstOrDefault();
		IsCarrying = false;
	}

	protected override void OnUpdate()
	{
		if (IsProxy)
			return;

		// If the plyaer has interacted and the cooldown has passed, reset the interaction state
		IsInteracting = !InteractionCooldownPassed();

		// Check if the player is carrying an object
		IsCarrying = GameObject.Components.Get<Holdable>(FindMode.EverythingInChildren) != null;

		InteractionHandler();


		// If the player is carrying an object and the use button is pressed, drop the object
		if (Input.Pressed("use") && !IsInteracting && IsCarrying)
		{
			GameObject.Children.FirstOrDefault(x => x.Tags.Has("interactable")).Components.Get<AInteractable>().OnInteract(GameObject);
			IsCarrying = false;
			StartInteract();
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

	public static bool IsOwner(GameObject gameObject)
	{
		return gameObject.Network.IsOwner;
	}

	private bool ErrorChecking(AInteractable interactable)
	{
		if (interactable == null) return false;

		if (PlayerComponent.InMenu) return false;

		if (PlayerComponent.InCinematic) return false;

		if (PlayerComponent.InDialogue) return false;

		return true;
	}

	private void InteractionHandler()
	{
		// Perform a ray trace to check for interactable objects
		var position = GameObject.Transform.Position;
		position.z += 50;
		SceneTraceResult collisionResult = Scene.Trace
				.Ray(position, position + PlayerComponent.Camera.Transform.Rotation.Forward * InteractDistance)
				.WithTag("interactable")
				.Run();
		
		// If the ray trace hits an object with the interactable tag, check if the object is valid for interaction
		if (collisionResult.Hit)
		{
			AInteractable interactable = collisionResult.GameObject.Components.Get<AInteractable>();

			// If the interactable is null, return
			if (interactable == null) return;

			// If the interactable is not valid for the current interaction, return
			if (!ErrorChecking(interactable)) return;

			if(!interactable.CanInteract(GameObject)) return;
			
			// Set the interact HUD value to the interactable description
			interactHud.SetValue(interactable.Description);

			// If the use button is pressed and the player is not interacting, start the interaction
			// Interactor must be the owner of the object
			if (Input.Pressed("use") && !IsInteracting && (IsOwner(interactable.GameObject) || interactable.Type == InteractableType.Bob))
			{
				// Start the interaction and call the OnInteract method of the interactable
				StartInteract();
				interactable?.OnInteract(GameObject);
				interactHud.SetValue(null);
			}
		} else
		{
			interactHud.SetValue(null);
		}
	}
}
