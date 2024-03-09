using Sandbox;


public sealed class DeliveryCar : Component
{
	[Category ("Status")]
	[Property]
	[Sync]
	public bool IsDelivering { get; set; }

	[Category ("Status")]
	[Property]
	[Sync]
	public bool IsReturning { get; set; }

	[Category ("Parameter")]
	[Property]
	public GameObject Spawn { get; set; }

	[Category ("Variable")]
	[Property]
	[Range (0.0f, 500.0f)]
	public float DesinationMargin { get; set; } = 100.0f;

	public NavMeshAgent Agent { get; set; }
	public Vector3 Destination { get; set; }

	private SkinnedModelRenderer Renderer { get; set; }	
	protected override void OnAwake()
	{
		IsDelivering = false;
		Agent = GameObject.Components.Get<NavMeshAgent>();
		Renderer = Components.Get<SkinnedModelRenderer>( true );

		Renderer.Set("Riding", true);
		
	}
	protected override void OnUpdate()
	{
		if (IsProxy) return;

		// Move the delivery car to the destination
		if (IsDelivering || IsReturning)
			Agent.MoveTo(Destination);

		// If the delivery car has finished delivering and has arrived at the spawn, destroy the delivery car
		if (IsReturning && IsArrived())
		{
			IsReturning = false;
			IsDelivering = false;
			GameObject.Destroy();
		}
	}

	// Start the delivery car and move to the destination
	public void StartDelivery(Vector3 destination)
	{
		IsDelivering = true;
		Destination = destination;
	}

	// Stop the delivery car and return to the spawn
	public void StopDelivery(Vector3 destination)
	{
		IsReturning = true;
		Destination = destination;
	}

	// Check if the delivery car has arrived at the destination
	public bool IsArrived()
	{
		return Transform.Position.Distance(Destination) < DesinationMargin;
	}
}
