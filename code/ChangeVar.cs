using Sandbox;

public sealed class ChangeVar : Component, Component.ICollisionListener
{
	protected override void OnUpdate()
	{

	}

	public void OnCollisionStart(Collision o)
	{
		Log.Info("Collision Start");
		if (o.Other.GameObject.Root.Tags.Has("player"))
		{
			var player = o.Other.GameObject.Root.Components.Get<Player>();
			player.AddMoney(500);
			Log.Info("Player has 500 more money");
		}
	}

	public void OnCollisionStop( CollisionStop o )
	{
		if (o.Other.GameObject.Root.Tags.Has("player"))
		{

		}
	}

	public void OnCollisionUpdate( Collision o )
	{
		
	}
}
