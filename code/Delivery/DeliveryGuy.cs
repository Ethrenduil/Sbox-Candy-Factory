using Sandbox;
using Sandbox.Citizen;

[Category( "Candy Factory - delivery" )]
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
	[Sync]
	public float DesinationMargin { get; set; } = 10.0f;

	[Property] public NavMeshAgent Agent { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	public Vector3 Destination { get; set; }

	protected override void OnAwake()
	{
		IsDelivering = false;
	}

    protected override void OnUpdate()
    {
		if (IsProxy) return;
        UpdateAnimation(Agent.Velocity.Length, Agent.WishVelocity.Length);
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
		DressDeliveryGuy(FileSystem.Mounted.ReadAllText( "materials/clothes/delivery_guy.json" ));
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

	[Broadcast]
    private void UpdateAnimation(float velocity, float wishVelocity)
	{
		float rotateDifference = 0;

		if (AnimationHelper is not null)
		{
			AnimationHelper.WithVelocity(velocity);
			AnimationHelper.WithWishVelocity(wishVelocity);
			AnimationHelper.IsGrounded = true;
			AnimationHelper.FootShuffle = rotateDifference;
		} else 
		{
			AnimationHelper = GameObject.Components.Get<CitizenAnimationHelper>();
		}
	}

	[Broadcast]
	private void DressDeliveryGuy(string clothes)
	{
		// Dress the delivery guy
		var clothing = new ClothingContainer();
		clothing.Deserialize(clothes);
		clothing.Apply( GameObject.Components.Get<SkinnedModelRenderer>(FindMode.EverythingInChildren));
	}
}
