using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Network;

namespace Eryziac.CandyFactory;

[Title( "Gamemode" )]
[Category( "Candy Factory" )]
public class CandyFactory : Component, Component.INetworkListener
{
	public static IEnumerable<Player> Players => InternalPlayers.Where( p => p.IsValid() );
	private static List<Player> InternalPlayers { get; set; } = new( 4 ) { null, null, null, null };
	
	public static CandyFactory Instance { get; private set; }
	
	[Property] public GameObject PlayerPrefab { get; set; }

	[Property] public GameObject SpawnPoint { get; set; }
	public static Player GetPlayer( int slot ) => InternalPlayers[slot];
	public static Player LocalPlayer { get; private set; }
	public static void AddPlayer( int slot, Player player )
	{
		player.PlayerSlot = slot;
		InternalPlayers[slot] = player;
	}

	protected override void OnAwake()
	{
		Instance = this;
		base.OnAwake();
	}
	
	protected override void OnStart()
	{
		if ( !GameNetworkSystem.IsActive )
		{
			GameNetworkSystem.CreateLobby();
		}

		base.OnStart();
	}

	private int FindFreeSlot()
	{
		for ( var i = 0; i < 4; i++ )
		{
			var player = InternalPlayers[i];
			if ( player.IsValid() ) continue;
			return i;
		}

		return -1;
	}

	void INetworkListener.OnActive( Connection connection )
	{
		var player = PlayerPrefab.Clone(SpawnPoint.Transform.World);
		var playerSlot = FindFreeSlot();

		if ( playerSlot < 0 )
		{
			throw new( "Player joined but there's no free slots!" );
		}

		var playerComponent = player.Components.Get<Player>();

		var nameTagPanel = player.Components.Get<NameTagPanel>( FindMode.EverythingInSelfAndDescendants);
		nameTagPanel.Name = connection.DisplayName;
		
		AddPlayer( playerSlot, playerComponent );
		player.NetworkSpawn( connection );
		if ( !player.IsProxy )
		{
			LocalPlayer = playerComponent;
		}
	}
}
