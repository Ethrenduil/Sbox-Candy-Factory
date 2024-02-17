namespace Sandbox;

public sealed class GameNetworkManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject SpawnPoint { get; set; }

	public void OnActive( Connection channel )
	{
		Log.Info( $"Player '{channel.DisplayName}' is becoming active" );

		
		var player = PlayerPrefab.Clone( SpawnPoint.Transform.World );

		var nameTag = player.Components.Get<NameTagPanel>( FindMode.EverythingInSelfAndDescendants );
		if ( nameTag is not null )
		{
			nameTag.Name = channel.DisplayName;
		}

		player.NetworkSpawn( channel );
	}
}
