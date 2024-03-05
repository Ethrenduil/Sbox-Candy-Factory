using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;

[Category("Player")]
[Title ("Player Interact")]
public class PlayerInteract : Component
{
	MeshTraceRequest request = new MeshTraceRequest();
	[Property] Player PlayerComponent;
	
	[Property]
	[Range( 0f, 1000f, 50f )]
	public float InteractDistance {get ; set; } = 150.0f;
	public bool isInteracting = false;
	[Property] public Interact interactHud;
	protected float InteractionTime = 0.0f;
    protected const float InteractionCooldown = 0.5f;

	protected override void OnStart()
	{
		base.OnStart();
		PlayerComponent = GameObject.Components.Get<Player>(FindMode.EnabledInSelf);
		request = request.WithTag( "interactable" );
		interactHud = Scene.GetAllComponents<Interact>().FirstOrDefault();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (IsProxy)
			return;
		var position = GameObject.Transform.Position;
		position.z += 50;

		SceneTraceResult collisionResult = Scene.Trace
				.Ray(position, position + PlayerComponent.Camera.Transform.Rotation.Forward * InteractDistance)
				.WithTag("interactable")
				.Run();
		if (collisionResult.Hit)
		{
			AInteractable interactable = collisionResult.GameObject.Components.Get<AInteractable>();
			interactHud.SetValue(interactable.Description);
			if (Input.Pressed("use") && !isInteracting )
			{
				StartInteract();
				interactable?.OnInteract(GameObject);
				interactHud.SetValue(null);
			}
		} else
		{
			interactHud.SetValue(null);
		}
		if (isInteracting && InteractionCooldownPassed())
		{
			isInteracting = false;
		}
	}

	public void StartInteract()
	{
		isInteracting = true;
		InteractionTime = Time.Now;
	}
	public bool InteractionCooldownPassed()
	{
		return Time.Now - InteractionTime > InteractionCooldown;
	}
}