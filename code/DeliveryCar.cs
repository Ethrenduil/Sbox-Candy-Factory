using System;
using System.IO;
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

	[Category ("Status")]
	[Property]
	[Sync]
	public bool IsGuyDelivering { get; set; }

	[Category ("Status")]
	[Property]
	[Sync]
	public bool IsWaiting { get; set; }

	[Category ("Variable")]
	[Property]
	[Range (0.0f, 500.0f)]
	public float DesinationMargin { get; set; } = 100.0f;

	[Category ("Preset")]
	[Property]
	public GameObject DeliveryGuyPrefab { get; set; }
	public DeliveryGuy DeliveryGuy { get; set; }
	private GameObject SpawnPositionGuy { get; set; }

	// public NavMeshAgent Agent { get; set; }
	public Vector3 Destination { get; set; }
	public Vector3 BoxDestination { get; set; }

	private SkinnedModelRenderer Renderer { get; set; }

	private Rigidbody rigidbodyCar { get; set; }
	[Property] public float Speed { get; set; } = 100.0f;
	private int Direction { get; set; } = 1;

	protected override void OnAwake()
	{
		IsDelivering = false;
		// Agent = GameObject.Components.Get<NavMeshAgent>();
		Renderer = Components.Get<SkinnedModelRenderer>( true );
		SpawnPositionGuy = GameObject.Children.Where(x => x.Name == "SpawnGuy").FirstOrDefault();
		rigidbodyCar = GameObject.Components.Get<Rigidbody>();

		Renderer.Set("Riding", true);
	}

	protected override void OnStart()
	{
		base.OnStart();
	}
	protected override void OnUpdate()
	{
		if (IsProxy) return;

		// Move the delivery car to the destination
		if ((IsDelivering && !IsWaiting) || IsReturning)
		{
			// Move the delivery car to the destination
			// Agent.MoveTo(Destination);
			// Transform.Position = Transform.Position.LerpTo(Destination, 0.004f);
			AdjustVelocity();
		}

		// if the car has arrived near the delivery destination, stop the car and spawn the delivery guy
		if (IsDelivering && IsCarArrived() && !IsGuyDelivering)
		{
			StartDeliveryGuy();
		}

		// If the delivery guy has finished delivering and has arrived at the spawn, destroy the delivery guy
		if (IsGuyDelivering && DeliveryGuy.IsReturning && IsArrived())
		{
			ReturnToSpawn();
		}

		// If the delivery car has finished delivering and has arrived at the spawn, destroy the delivery car
		if (IsReturning && IsCarArrived())
		{
			GameObject.Destroy();
		}
	}

	// Start the delivery car and move to the destination
	public void StartDelivery(Vector3 destination)
	{
		IsDelivering = true;
		BoxDestination = destination;
		Destination = destination;
		if (Direction < 0) Direction *= -1;
	}

	// Stop the delivery car and return to the spawn
	public void StopDelivery(Vector3 destination)
	{
		Destination = destination;
		DeliveryGuy.StopDelivery(SpawnPositionGuy.Transform.Position);
	}

	// Check if the delivery car has arrived at the destination
	public bool IsCarArrived()
	{
		return Math.Abs(Transform.Position.x - Destination.x) < DesinationMargin;
	}
	public bool IsArrived()
	{
		if (DeliveryGuy == null) return false;
		return DeliveryGuy.IsArrived();
	}

	public void StartDeliveryGuy()
	{
		// Stop the car
		// Agent.Stop();

		// Update Status
		IsGuyDelivering = true;
		IsWaiting = true;
		// Update Animation
		Renderer.Set("Riding", true);
		Renderer.Set("Idle", false);

		// Spawn the delivery guy and start the delivery
		var guy = DeliveryGuyPrefab.Clone();
		guy.Transform.Position = SpawnPositionGuy.Transform.World.Position;
		guy.NetworkSpawn();
		DeliveryGuy = guy.Components.Get<DeliveryGuy>();
		DeliveryGuy.StartDelivery(BoxDestination);
	}

	public void ReturnToSpawn()
	{
		// Update Status
		IsReturning = true;
		IsDelivering = false;
		IsWaiting = false;
		IsGuyDelivering = false;
		
		// Update Animation
		Renderer.Set("Riding", false);
		Renderer.Set("Idle", true);

		// Destroy the delivery guy and rotate the car
		DeliveryGuy.GameObject.Destroy();
	}

	private void AdjustVelocity()
	{
		var wishVelocity = Transform.Rotation.Forward * Speed * Direction;
		if (rigidbodyCar.Velocity.Length > Speed)
		{
			var velocityLength = rigidbodyCar.Velocity.Length;
			wishVelocity = new Vector3(rigidbodyCar.Velocity.x / velocityLength, rigidbodyCar.Velocity.y / velocityLength, rigidbodyCar.Velocity.z / velocityLength) * Speed;
		}

		if (IsCarArrived())
		{
			wishVelocity = Vector3.Zero;
		}

		rigidbodyCar.Velocity = wishVelocity;
	}
}