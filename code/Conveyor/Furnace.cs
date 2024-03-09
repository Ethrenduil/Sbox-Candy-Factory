using Microsoft.VisualBasic;
using Sandbox;
using System.Numerics;
using Eryziac.CandyFactory;

[Category( "Candy Factory - Factory" )]
public class Furnace : Component, Component.ICollisionListener
{
	[Property] private bool IsMoving = true;
	[Property] private float Speed = 100f;
	[Property] public GameObject CookedObject { get; set; }
	[Property] private SoundEvent CookingSound { get; set; }
	[Property] private SoundEvent CookedSound { get; set; }
	public List<GameObject> Candies { get; set; } = new();
	private bool IsCooking = false;
	private float CookTimer { get; set; }
	private Vector3 BoxOffset { get; set; } = new( 0, -20, 150 );
	private Vector3 CookedOffset { get; set; } = new( 0, -15, 80 );
	private SoundHandle sound;

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( IsMoving && !IsCooking )
		{
			MoveCandies();
		}
	}

	public void MoveCandies()
    {
        var conveyorDirection = this.GameObject.Transform.Rotation.Forward;
        var newVelocity = conveyorDirection * Speed;

        foreach ( var candy in Candies )
        {
            var rigidbodyCandy = candy.Components.Get<Rigidbody>();

            if ( rigidbodyCandy.Velocity.Length > Speed )
            {
                var velocityLength = newVelocity.Length;
                newVelocity = new Vector3( rigidbodyCandy.Velocity.x / velocityLength, rigidbodyCandy.Velocity.y / velocityLength, rigidbodyCandy.Velocity.z / velocityLength ) * Speed;
            }

            rigidbodyCandy.Velocity = newVelocity;
        }
    }

	public virtual void OnCollisionStart( Collision o )
	{
		if ( !o.Other.GameObject.Root.IsProxy && !IsProxy )
			HandleCollision( o );
	}

	public void OnCollisionStop( CollisionStop o )
	{
		Candies.Remove( o.Other.GameObject.Root );

		var rigidbodyCandy = o.Other.GameObject.Root.Components.Get<Rigidbody>();
		var centerDirection = (Transform.Position - o.Other.GameObject.Root.Transform.Position).Normal;
		var forwardForce = centerDirection * 500;
		rigidbodyCandy.ApplyForce( forwardForce );
	}

	public void OnCollisionUpdate( Collision o )
	{
	}

	private async void HandleCollision( Collision o )
	{
		if ( !IsMoving )
		{
			return;
		}

		var gameObject = o.Other.GameObject.Root;
		if ( !IsAboveConveyor( gameObject ) )
		{
			return;
		}

		if ( gameObject.Tags.Has( "interactable" ) )
		{
			gameObject.Transform.Position = Transform.Position + BoxOffset;
			gameObject.Transform.Rotation *= new Angles( 0, 0, 180 );
			gameObject.Components.Get<Rigidbody>().Destroy();
			gameObject.Components.Get<ModelCollider>().Destroy();
			gameObject.Components.Get<Holdable>().Open();
			await GameTask.Delay( 3000 );
			gameObject.Destroy();
		    IsCooking = true;
			Cook();
			return;
		}

		if ( gameObject.Tags.Has( "Candy" ) )
		{
			Candies.Add( gameObject );
		}
	}

	private bool IsAboveConveyor( GameObject gameObject )
	{
		var conveyorPosition = Transform.Position;
		var objectPosition = gameObject.Transform.Position;

		return objectPosition.z > conveyorPosition.z;
	}

	[Broadcast]
	private void CookingStarted(float CookTime)
	{
		if ( CookingSound is not null )
        	sound = Sound.Play( CookingSound, Transform.Position + CookedOffset );
		Components.Get<FurnacePanel>(FindMode.EnabledInSelfAndChildren).StartCooking(CookTime);
	}

	[Broadcast]
	private void CookingFinished()
	{
		Components.Get<SkinnedModelRenderer>().Set( "Opening", true );
		sound?.Stop();
		if ( CookedSound is not null )
        	Sound.Play( CookedSound, Transform.Position + CookedOffset);
	}

	[Broadcast]
	private void CloseOven()
	{
		Components.Get<SkinnedModelRenderer>().Set( "Opening", false );
	}

	private async void Cook()
    {
        var cooked = CookedObject.Clone( Transform.Position + CookedOffset );
        cooked.NetworkSpawn();
		var candy = cooked.Components.Get<Candies>();
        var CookTime = candy.CookingTime;
		CookingStarted(CookTime);
        await GameTask.DelaySeconds( CookTime );
		CookingFinished();
		var currentTask = Scene.GetAllComponents<Player>().FirstOrDefault( x => !x.IsProxy ).CurrentTask;
		if ( currentTask is not null )
		{
			if ( currentTask.Needed.CandyCreated.Name == candy.Name && currentTask.Needed.CandyCreated.Current < currentTask.Needed.CandyCreated.Quantity) {
				currentTask.Needed.CandyCreated.Current++;
				Scene.GetAllComponents<CandyFactory>().FirstOrDefault().RefreshTaskHUD();
			}
		}
        IsCooking = false;
        await GameTask.DelaySeconds( 2 );
		CloseOven();
    }
}
