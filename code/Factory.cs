using Sandbox;

public sealed class Factory : Component
{
	[Property] [Sync] public ulong SteamId { get; set; }
	[Property] [Sync] public string Name { get; set; }
	[Property] [Sync] public int Money { get; set; }
	[Property] [Sync] public int MaxItems { get; set; } = 10;
	[Property] public Dictionary<DeliveryGoods, int> Stock { get; set; } = new Dictionary<DeliveryGoods, int>();
	public bool IsStarted { get; set; } = false;

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

		// Set the Factory Title to the owner's name
		
		GameObject.Components.Get<TitleFactory>(FindMode.InChildren).SetTitle(owner.DisplayName + "'s Factory");
		// For each interactable in the factory, take ownership
		foreach (var children in GameObject.Children.Where(c => c.NetworkMode == NetworkMode.Snapshot))
		{
			children.Network.AssignOwnership(owner);
		}

		GameObject.Network.AssignOwnership(owner);

	}

	public string GetStockItemName(DeliveryGoods goods)
	{
		return goods switch
		{
			DeliveryGoods.Sugar => "Sugar",
			DeliveryGoods.Flour => "Flour",
			DeliveryGoods.Milk => "Milk",
			DeliveryGoods.Vanilla => "Vanilla",
			_ => "Unknown",
		};
	}

	public DeliveryGoods GetStockItemNameFromString(string goods)
	{
		return goods switch
		{
			"Sugar" => DeliveryGoods.Sugar,
			"Flour" => DeliveryGoods.Flour,
			"Milk" => DeliveryGoods.Milk,
			"Vanilla" => DeliveryGoods.Vanilla,
			_ => DeliveryGoods.None,
		};
	}

	// Get the stock count of a specific item from a string item name
	public int GetStockItemCountFromString(string goods)
	{
		var item = GetStockItemNameFromString(goods);
		return Stock.TryGetValue( item, out int value ) ? value : 0;
	}

	public bool CanGetQuantity(string goods, int quantity)
	{
		var item = GetStockItemNameFromString(goods);
		return Stock.TryGetValue( item, out int value ) && value >= quantity;
	}

	public bool CanGetQuantity(DeliveryGoods goods, int quantity)
	{
		return Stock.TryGetValue( goods, out int value ) && value >= quantity;
	}

	public void AddStock(DeliveryGoods goods, int quantity)
	{
		if (Stock.TryGetValue( goods, out int value ))
		{
			Stock[goods] = value + quantity;
		}
		else
		{
			Stock[goods] = quantity;
		}
	}

	public void AddStock(string goods, int quantity)
	{
		var item = GetStockItemNameFromString(goods);
		if (Stock.TryGetValue( item, out int value ))
		{
			Stock[item] = value + quantity;
		}
		else
		{
			Stock[item] = quantity;
		}
	}

	public void AddStockFromDictionary(Dictionary<DeliveryGoods, int> stock)
	{
		foreach (var item in stock)
		{
			AddStock(item.Key, item.Value);
		}
	}

	public void AddStockFromDictionary(Dictionary<string, int> order)
	{
		foreach (var item in order)
		{
			AddStock(item.Key, item.Value);
		}
	}

	public bool RemoveStock(DeliveryGoods goods, int quantity)
	{
		if (Stock.TryGetValue( goods, out int value ))
		{
			Stock[goods] = value - quantity;
			return true;
		}
		return false;
	}

	public bool RemoveStock(string goods, int quantity)
	{
		var item = GetStockItemNameFromString(goods);
		if (Stock.TryGetValue( item, out int value ))
		{
			Stock[item] = value - quantity;
			return true;
		}
		return false;
	}

	public bool RemoveStockFromDictionary(Dictionary<string, int> order)
	{
		foreach (var item in order)
		{
			if (!RemoveStock(item.Key, item.Value)) return false;
		}
		return true;
	}

	public bool RemoveStockFromDictionary(Dictionary<DeliveryGoods, int> order)
	{
		foreach (var item in order)
		{
			if(!RemoveStock(item.Key, item.Value)) return false;
		}
		return true;
	}

	public Dictionary<DeliveryGoods, int> ConvertStringStockToDeliveryGoods(Dictionary<string, int> stock)
	{
		var newStock = new Dictionary<DeliveryGoods, int>();
		foreach (var item in stock)
		{
			newStock[GetStockItemNameFromString(item.Key)] = item.Value;
		}
		return newStock;
	}
}
