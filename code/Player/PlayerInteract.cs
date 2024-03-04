using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;

public class PlayerInteract : Component
{
	MeshTraceRequest request = new MeshTraceRequest();
	[Property] Player PlayerComponent;
	
	[Property]
	[Range( 0f, 2000f, 50f )]
	public float InteractDistance {get ; set; } = 500.0f;
	public bool isInteracting = false;

	protected override void OnStart()
	{
		base.OnStart();
		PlayerComponent = GameObject.Components.Get<Player>(FindMode.EnabledInSelf);
		request = request.WithTag( "interactable" );
	}

	protected override void OnUpdate()
	{
		var position = GameObject.Transform.Position;
		position.z += 50;
		// Ray currentRay = new Ray(new Vector3(position.x, position.y, position.z + 50),  PlayerComponent.Camera.Transform.Rotation.Forward);

		SceneTraceResult collisionResult = Scene.Trace
				.Ray(position, position + PlayerComponent.Camera.Transform.Rotation.Forward * InteractDistance)
				.WithTag("interactable")
				.Run();
		// Log.Info("Raycast result data: " + result.Hit + " " + result.EndPosition + " " + result.Normal + " " + result.GetType() + " " + result.Material); 
		if (collisionResult.Hit)
		{
			Log.Info("Hit " + collisionResult.Component);
		}
	}
}