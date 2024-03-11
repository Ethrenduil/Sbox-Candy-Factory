using Sandbox;
using Sandbox.Citizen;


public sealed class DeliveryGuy : Component
{
	[Category ("Status")]
	[Property]
	[Sync]
	public bool IsDelivering { get; set; }

	[Category ("Status")]
	[Property]
	[Sync]
	public bool IsReturning { get; set; }

	[Category ("Variable")]
	[Property]
	[Range (0.0f, 500.0f)]
	public float DesinationMargin { get; set; } = 10.0f;

	public NavMeshAgent Agent { get; set; }
	public Vector3 Destination { get; set; }

	private CitizenAnimationHelper AnimationHelper { get; set; }	
	protected override void OnAwake()
	{
		IsDelivering = false;
		Agent = GameObject.Components.Get<NavMeshAgent>();
		AnimationHelper = Components.Get<CitizenAnimationHelper>( true );
		DressDeliveryGuy();
	}

    protected override void OnUpdate()
    {
        UpdateAnimation();
    }
	protected override void OnFixedUpdate()
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
		if (Transform.Position.Distance(Destination) < DesinationMargin)
			Log.Info(Transform.Position.Distance(Destination));

		return Transform.Position.Distance(Destination) < DesinationMargin;
	}

    private void UpdateAnimation()
	{
		float rotateDifference = 0;

		if (AnimationHelper is not null)
		{
			AnimationHelper.WithVelocity(Agent.Velocity.Length);
			AnimationHelper.WithWishVelocity(Agent.WishVelocity.Length);
			AnimationHelper.IsGrounded = true;
			AnimationHelper.FootShuffle = rotateDifference;
		}
	}

	private void DressDeliveryGuy()
	{
		// Dress the delivery guy
		var clothing = new ClothingContainer();
		clothing.Deserialize( FileSystem.Mounted.ReadAllText( "clothes/delivery_guy.json" ) );
		clothing.Apply( GameObject.Components.Get<SkinnedModelRenderer>(FindMode.EverythingInChildren));
	}
}
