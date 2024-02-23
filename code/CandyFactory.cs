using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Network;
using Sandbox.Utility;
using Sandbox.Services;

namespace Eryziac.CandyFactory;

[Title( "Gamemode" )]
[Category( "Candy Factory" )]
public class CandyFactory : Component, Component.INetworkListener
{
	[Sync] public static IEnumerable<Player> Players => InternalPlayers.Where( p => p.IsValid() );
	[Sync] private static List<Player> InternalPlayers { get; set; } = new( 4 ) { null, null, null, null };
	
	public static CandyFactory Instance { get; private set; }
	
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject SpawnPoint { get; set; }
	[Property] public int StartingMoney { get; set; } = 100;
	public static Player GetPlayer( int slot ) => InternalPlayers[slot];
	public static List<Player> GetPlayers() => InternalPlayers;
	public static void AddPlayer( int slot, Player player )
	{
		player.PlayerSlot = slot;
		InternalPlayers[slot] = player;
	}

	public static void RemovePlayer( int slot )
	{
		InternalPlayers[slot] = null;
	}

	protected override void OnAwake()
	{
		Instance = this;
		base.OnAwake();
	}
	
	protected override void OnStart()
	{

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

		playerComponent.Name = connection.DisplayName;
		Log.Info( $"Player {connection.DisplayName} joined, slot {playerSlot}" );

		AddPlayer( playerSlot, playerComponent );
		player.NetworkSpawn( connection );
	}

	void INetworkListener.OnDisconnected(Sandbox.Connection conn)
	{
		var player = Players.FirstOrDefault( p => p.Connection == conn );
		if ( player is not null )
		{
			RemovePlayer( player.PlayerSlot );
		}
	}

	public void RefreshTaskHUD()
	{
		Scene.GetAllComponents<CurrentTaskHUD>().FirstOrDefault().StateHasChanged();
	}

	public void RefreshMoneyHUD()
	{
		Scene.GetAllComponents<MoneyHUD>().FirstOrDefault().StateHasChanged();
	}

	public void RefreshAllHUDs()
	{
		RefreshTaskHUD();
		RefreshMoneyHUD();
	}
}
