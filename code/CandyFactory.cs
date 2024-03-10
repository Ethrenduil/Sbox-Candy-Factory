using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Network;
using Sandbox.Utility;
using Sandbox.Services;
using Sandbox.Navigation;

namespace Eryziac.CandyFactory;

[Title( "Gamemode" )]
[Category( "Candy Factory" )]
public class CandyFactory : Component, Component.INetworkListener
{
	
	[Property] public GameObject PlayerPrefab { get; set; }
	public static List<GameObject> SpawnPoint { get; set; }
	[Property] public List<GameObject> FactoryList { get; set; }
	[Property] public int StartingMoney { get; set; } = 100;
	[Property] public GameObject Factory { get; set; }
	[Property] public List<ulong> _isFactoryActive { get; set; } = new List<ulong> { 0, 0, 0, 0 };

	protected override void OnAwake()
	{
		base.OnAwake();
	}
	
	protected override void OnStart()
	{
		base.OnStart();
	}

	void INetworkListener.OnActive( Connection connection )
	{
		// Get the list of spawn points and the number of players
		SpawnPoint = Scene.GetAllComponents<SpawnPoint>().Select( s => s.GameObject ).ToList();

		int nbPlayer = Scene.Components.GetAll<Player>().Count();
		if ( nbPlayer >= 4 )
		{
			throw new( "Player joined but there's no free slots!" );
		}

		// Spawn the player
		var player = PlayerPrefab.Clone(SpawnPoint[nbPlayer].Transform.World);

		// Set the player's name and name tag
		player.Name = connection.DisplayName;
		var nameTagPanel = player.Components.Get<NameTagPanel>( FindMode.EverythingInSelfAndDescendants);
		nameTagPanel.Name = connection.DisplayName;

		// Set the player Components's steam id and name
		var playerComponent = player.Components.Get<Player>();
		playerComponent.Name = connection.DisplayName;
		playerComponent.SteamId = connection.SteamId;

		Log.Info( $"Player {connection.DisplayName} joined" );

		// Network spawn the player and enable the navmesh
		player.NetworkSpawn( connection );	
		// Spawn Factory and start it
		var factory = Factory.Clone(FactoryList[GetFreeFactoryIndex(connection)].Transform.World);
		factory.Components.Get<Factory>().StartFactory( connection);
		factory.NetworkSpawn(connection);
	}

	void INetworkListener.OnDisconnected(Connection conn)
	{
		Log.Info( $"Player {conn.DisplayName} disconnected" );
		_isFactoryActive[_isFactoryActive.IndexOf(conn.SteamId)] = 0;
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

	private int GetFreeFactoryIndex(Connection connection)
	{
		for (int i = 0; i < _isFactoryActive.Count; i++)
		{
			if (_isFactoryActive[i] == 0)
			{
				_isFactoryActive[i] = connection.SteamId;
				return i;
			}
		}
		return -1;
	}
}
