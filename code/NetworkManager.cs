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

[Title( "Network Manager" )]
[Category( "Candy Factory" )]
public class NetworkManager : Component, Component.INetworkListener
{
	protected override void OnAwake()
	{
		base.OnAwake();
	}

	void INetworkListener.OnActive( Connection connection )
	{
        Log.Info( "Player connected" );
        var cdyfac = Scene.Components.GetAll<CandyFactory>().FirstOrDefault();
        if (connection.IsHost)
        {
            cdyfac.Network.AssignOwnership( connection );
        }
        cdyfac.NewPlayer( connection );
	}

	void INetworkListener.OnDisconnected(Connection conn)
	{
        Log.Info( "Player disconnected" );
		var cdyfac = Scene.Components.GetAll<CandyFactory>().FirstOrDefault();
        if (conn.IsHost && Networking.Connections.Count > 1)
        {
            cdyfac.Network.AssignOwnership( Networking.Connections[1] );
        }
        cdyfac.DeletePlayer( conn );
	}
}
