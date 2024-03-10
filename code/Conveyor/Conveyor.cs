using Microsoft.VisualBasic;
using Sandbox;
using System.Numerics;

[Category( "Candy Factory - Factory" )]
public class Conveyor : Component, Component.ICollisionListener
{
    [Property] [Sync] public bool IsMoving { get; set; } = true;
    [Property] private readonly float Speed = 100; // Change this to the speed you want
	[Property] private readonly bool Turn = false;
	[Property] private readonly bool special = false;
	[Property] public List<GameObject> Candies { get; set; } = new();

    protected override void OnFixedUpdate()
    {
		base.OnFixedUpdate();

		if (!IsMoving)
    	{
    	    return;
    	}

    	var conveyorDirection = this.GameObject.Transform.Rotation.Forward;

		if (Turn)
		{
			var targetDirection = this.GameObject.Transform.Rotation.Left;
    		conveyorDirection = Vector3.Lerp(conveyorDirection, targetDirection, 0.5f);
		}

    	foreach (var candy in Candies)
    	{
    	    var rigidbodyCandy = candy.Components.Get<Rigidbody>();
    	    var newVelocity = conveyorDirection * Speed;

    	    if (rigidbodyCandy.Velocity.Length > Speed)
    	    {
    	        var velocityLength = newVelocity.Length;
    	        newVelocity = new Vector3(rigidbodyCandy.Velocity.x / velocityLength, rigidbodyCandy.Velocity.y / velocityLength, rigidbodyCandy.Velocity.z / velocityLength) * Speed;
    	    }

    	    rigidbodyCandy.Velocity = newVelocity;

			if (special && !candy.Tags.Has("Upgraded"))
        	{
            	var centerPosition = this.GameObject.Transform.Position;
				centerPosition.z += 64;

            	var distanceToCenter = candy.Transform.Position.Distance(centerPosition);
            	if (distanceToCenter < 20f)
            	{
            	    IsMoving = false;
            	}
        	}
    	}
    }

	[Broadcast]
	public void RemoveCandies()
	{
		Candies.Clear();
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
