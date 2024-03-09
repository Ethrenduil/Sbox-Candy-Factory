using Sandbox;

public sealed class Factory : Component
{
	[Property] [Sync] public ulong SteamId { get; set; }
	[Property] [Sync] public string Name { get; set; }
	[Property] [Sync] public int Money { get; set; }
	[Property] public Dictionary<DeliveryGoods, int> Stock { get; set; } = new Dictionary<DeliveryGoods, int>();

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	public void StartFactory(Connection owner)
	{
		// Set the factory owner's steam id and name
		SteamId = owner.SteamId;
		Name = owner.DisplayName;

		// For each interactable in the factory, take ownership
		foreach (var interactable in GameObject.Children.Where(c => c.Tags.Has("interactable")))
		{
			interactable.Network.TakeOwnership();
		}

		// Set the Factory Title to the owner's name
		GameObject.Components.Get<TitleFactory>(FindMode.InChildren).SetTitle(owner.DisplayName);
	}
}