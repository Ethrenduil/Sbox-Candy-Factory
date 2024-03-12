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
public class CandyFactory : Component
{
	
	[Property] public GameObject PlayerPrefab { get; set; }
	public static List<GameObject> SpawnPoint { get; set; }
	[Property] public List<GameObject> FactoryList { get; set; }
	[Property] public int StartingMoney { get; set; } = 100;
	[Property] public GameObject Factory { get; set; }
	[Property] [Sync] public List<bool> _isFactoryActive { get; set; } = new List<bool> { false, false, false, false };

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}
	protected override void OnStart()
	{
		base.OnStart();
	}

	public void NewPlayer( Connection connection )
	{
		// Get the list of spawn points and the number of players
		SpawnPoint = Scene.GetAllComponents<SpawnPoint>().Select( s => s.GameObject ).ToList();

		// Check if there's a free slot for the player
		int nbPlayer = Scene.Components.GetAll<Player>().Count();
		if ( nbPlayer >= 4 )
		{
			throw new( "Player joined but there's no free slots!" );
		}

		// Get Free Factory Index
		int freeFactoryIndex = GetFreeFactoryIndex(connection);
		_isFactoryActive[freeFactoryIndex] = true;


		// Spawn the player
		var player = PlayerPrefab.Clone(SpawnPoint[freeFactoryIndex].Transform.World);

		// Set the player's name and name tag
		player.Name = connection.DisplayName;
		var nameTagPanel = player.Components.Get<NameTagPanel>( FindMode.EverythingInSelfAndDescendants);
		nameTagPanel.Name = connection.DisplayName;

		// Set the player Components's steam id and name
		var playerComponent = player.Components.Get<Player>();
		playerComponent.Name = connection.DisplayName;
		playerComponent.SteamId = connection.SteamId;
		playerComponent.PlayerSlot = freeFactoryIndex;

		Log.Info( $"Player {connection.DisplayName} joined" );

		// Network spawn the player and enable the navmesh
		player.NetworkSpawn( connection );	

		// Spawn Factory and start it
		var factory = Factory.Clone(FactoryList[freeFactoryIndex].Transform.World);
		factory.Components.Get<Factory>().StartFactory( connection);
		factory.NetworkSpawn(connection);
	}

	public void DeletePlayer(Connection conn)
	{
		Log.Info( $"Player {conn.DisplayName} disconnected" );
		var player = Scene.GetAllComponents<Player>().FirstOrDefault( p => p.SteamId == conn.SteamId );
		_isFactoryActive[player.PlayerSlot] = false;
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
			if (_isFactoryActive[i] == false)
			{
				_isFactoryActive[i] = true;
				return i;
			}
		}
		return -1;
	}
}
