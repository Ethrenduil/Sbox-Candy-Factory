using Sandbox;
using Sandbox.Network;
using System.Threading.Tasks;

public sealed class NetworkSession : Component
{
	protected override void OnStart()
	{
		if ( !GameNetworkSystem.IsActive )
		{
			GameNetworkSystem.CreateLobby();
		}
	}

}
