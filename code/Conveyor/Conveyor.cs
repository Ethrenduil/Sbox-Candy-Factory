using Microsoft.VisualBasic;
using Sandbox;
using Sandbox.UI;
using System.Numerics;

[Category( "Candy Factory - Factory" )]
public class Conveyor : Component, Component.ICollisionListener
{
	public delegate void ConveyorMovementChangedHandler(Conveyor conveyor, bool isMoving);
    public event ConveyorMovementChangedHandler OnConveyorMovementChanged;
    private bool _isMoving = true;
    [Property] [Sync] 
    public bool IsMoving 
    { 
        get { return _isMoving; } 
        set 
        { 
            if (_isMoving != value)
            {
                _isMoving = value;
                OnConveyorMovementChanged?.Invoke(this, _isMoving);
            }
        } 
    }

    [Property] private readonly float Speed = 100; // Change this to the speed you want
	[Property] private readonly bool Turn = false;
	[Property] public bool special = false;
	[Property] public List<GameObject> Candies { get; set; } = new();
	[Property]private Conveyor NextConveyor { get; set; }

	public bool IsCooking = false;

	protected override void OnStart()
	{
		base.OnStart();
		SceneTraceResult collisionResult = Scene.Trace
				.Box( Transform.Scale / 2 , Transform.Position, Transform.Position + Transform.Rotation.Forward * 100 )
				.WithTag("conveyor")
				.IgnoreGameObject(this.GameObject)
				.Run();

		if (collisionResult.Hit)
        {
            NextConveyor = collisionResult.GameObject.Components.Get<Conveyor>();
            NextConveyor.OnConveyorMovementChanged += HandleNextConveyorMovementChanged;
        }
	}

	 private void HandleNextConveyorMovementChanged(Conveyor conveyor, bool isMoving)
    {
        if (!isMoving)
        {
            // Le convoyeur suivant s'est arrêté, donc ce convoyeur doit également s'arrêter
            IsMoving = false;
        }
        else
        {
            // Le convoyeur suivant a commencé à bouger, donc ce convoyeur peut également commencer à bouger
            IsMoving = true;
        }
    }

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (NextConveyor is not null)
		{
			if (NextConveyor.GameObject.Enabled == false)
			{
				SceneTraceResult collisionResult = Scene.Trace
				.Box( Transform.Scale / 2 , Transform.Position, Transform.Position + Transform.Rotation.Forward * 100 )
				.WithTag("conveyor")
				.IgnoreGameObject(GameObject)
				.Run();

				if (collisionResult.Hit)
				{
					NextConveyor = collisionResult.GameObject.Components.Get<Conveyor>();
				}
			}

		}
	}

	protected override void OnFixedUpdate()
    {
		base.OnFixedUpdate();
		if (!IsMoving || IsCooking)
    	{
    	    return;
    	}

    	var conveyorDirection = Transform.Rotation.Forward;

		if (Turn)
		{
			var targetDirection = Transform.Rotation.Left;
    		conveyorDirection = Vector3.Lerp(conveyorDirection, targetDirection, 0.5f);
		}

    	foreach (var candy in Candies)
    	{
			if (!candy.IsValid())
			{
				Candies.Remove(candy);
				continue;
			}
    	    var rigidbodyCandy = candy.Components.Get<Rigidbody>();
    	    var newVelocity = conveyorDirection * Speed;


    	    if (rigidbodyCandy.Velocity.Length > Speed)
    	    {
    	        var velocityLength = newVelocity.Length;
    	        newVelocity = new Vector3(rigidbodyCandy.Velocity.x / velocityLength, rigidbodyCandy.Velocity.y / velocityLength, rigidbodyCandy.Velocity.z / velocityLength) * Speed;
    	    }

    	    rigidbodyCandy.Velocity = newVelocity;

			if (special  && !candy.Name.Contains(GameObject.Components.Get<Upgrader>().upgradedObject.Name))
			{
				var centerPosition = this.GameObject.Transform.Position;
				centerPosition.z += 64;

				var distanceToCenter = candy.Transform.Position.Distance(centerPosition);
				if (distanceToCenter < 20f)
				{
					IsMoving = false;
					rigidbodyCandy.Velocity = Vector3.Zero; // Stop the candy's movement
				}
			}
    	}
    }

	[Broadcast]
	public void RemoveCandy(GameObject candy)
	{
		if (Candies.Count == 0)
		{
			return;
		}
		
		Candies.Remove(candy);
	}

    public virtual void OnCollisionStart(Collision o)
    {
        var gameObject = o.Other.GameObject.Root;
		

        if (!gameObject.Tags.Has("Candy"))
			return;

        HandleCollision(gameObject);
    }

    public void OnCollisionStop( CollisionStop o )
    {
		Candies.Remove(o.Other.GameObject.Root);

		var rigidbodyCandy = o.Other.GameObject.Root.Components.Get<Rigidbody>();
    	var centerDirection = (this.GameObject.Transform.Position - o.Other.GameObject.Root.Transform.Position).Normal;
    	var forwardForce = centerDirection * 500;
    	rigidbodyCandy.ApplyForce(forwardForce);
    }

    public void OnCollisionUpdate(Collision o)
    {
    }

    private void HandleCollision(GameObject gameObject)
    {
        if (!IsAboveConveyor(gameObject))
        {
            return;
        }

		Candies.Add(gameObject);
	}

   	private bool IsAboveConveyor(GameObject gameObject)
	{
    	var conveyorPosition = this.GameObject.Transform.Position;
    	var objectPosition = gameObject.Transform.Position;
    	var conveyorSize = this.GameObject.Transform.Scale; // Assuming the conveyor has a 'Scale' property

    	// Check if the object is within the conveyor's collision area
    	return objectPosition.z > conveyorPosition.z;
	}

}
