using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;

public class PlayerInteract : Component
{
	MeshTraceRequest request = new MeshTraceRequest();
	[Property] Player PlayerComponent;
	
	[Property]
	[Range( 0f, 1000f, 50f )]
	public float InteractDistance {get ; set; } = 150.0f;
	public bool isInteracting = false;
	[Property] public Interact interactHud;

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
			isInteracting = true;
			AInteractable interactable = collisionResult.GameObject.Components.Get<AInteractable>();
			interactHud.SetValue(interactable.Description);
			if (Input.Pressed("use"))
			{
				interactable?.OnInteract(GameObject);
				interactHud.SetValue(null);
				isInteracting = false;
			}
		} else
		{
			isInteracting = false;
			interactHud.SetValue(null);
		}
	}
}