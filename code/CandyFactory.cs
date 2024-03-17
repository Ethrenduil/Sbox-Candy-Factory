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
	[Sync] public NetList<ulong> _isFactoryActive { get; set; } = new NetList<ulong> { 0, 0, 0, 0 };
	private Settings Settings { get; set; }
	[Property] public SoundEvent GameMusic { get; set; }
	private SoundHandle CurrentMusic { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (CurrentMusic != null)
		{
			Settings ??= Scene.GetAllComponents<Settings>().FirstOrDefault(x => !x.IsProxy);
			CurrentMusic.Volume = Settings.GetVolume(VolumeType.Music) / 3;
		}
	}
	protected override void OnStart()
	{
		base.OnStart();
		// for (int i = 0; i < 4; i++)
		// {
		// 	Log.Info( $"Factory {i} is active: {_isFactoryActive[i]}" );
		// }
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (CurrentMusic != null)
			CurrentMusic.Stop();
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
		// Log.Info( $"Free Factory Index: {freeFactoryIndex}" );
		// _isFactoryActive[freeFactoryIndex] = connection.SteamId;


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
		// playerComponent.PlayerSlot = freeFactoryIndex;

		// Network spawn the player and enable the navmesh
		player.NetworkSpawn( connection );

		// Spawn Factory and start it
		var factory = Factory.Clone(FactoryList[freeFactoryIndex].Transform.World);
		factory.Components.Get<Factory>().StartFactory( connection);
		factory.NetworkSpawn(connection);

		// Start the music
		Settings ??= Scene.GetAllComponents<Settings>().FirstOrDefault(x => !x.IsProxy);
		GameMusic.UI = true;
		GameMusic.Volume = Settings.GetVolume(VolumeType.Music) / 3;
		CurrentMusic = Sound.Play( GameMusic );
	}

	public void DeletePlayer(Connection conn)
	{
		for (int i = 0; i < 4; i++)
		{
			if (_isFactoryActive[i] == conn.SteamId)
			{
				_isFactoryActive[i] = 0;
			}
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
