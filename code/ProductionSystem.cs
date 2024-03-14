using System;
using Sandbox;

public sealed class ProductionSystem : Component
{
	[Property] public Factory Factory { get; set; }
	[Property] public Dictionary<UpgradeType, int> Upgrades { get; set; } = new Dictionary<UpgradeType, int>();
	[Property] public Dictionary<UpgradeType, int> UpgradesCost { get; set; } = new Dictionary<UpgradeType, int>();
	[Property] public List<Upgrader> Upgrader { get; set; }
	[Property] public int ProductionSpeed { get; set; } = 0;
	[Property] public int StorageCapacity { get; set; } = 20;
	[Property] public int HoldableCapacity { get; set; } = 5;
	[Property] public float TransportCoolDownSpeed { get; set; } = 30.0f;
	[Property] public bool IsStarted { get; set; } = false;
	protected override void OnStart()
	{
		base.OnStart();
		Factory = GameObject.Parent.Components.Get<Factory>();
	}
	protected override void OnUpdate()
	{
		if (Input.Pressed("Slot4"))
		{
			Upgrade(UpgradeType.Upgrader);
		} else if (Input.Pressed("Slot5"))
		{
			Upgrade(UpgradeType.Production);
		} else if (Input.Pressed("Slot6"))
		{
			Upgrade(UpgradeType.Storage);
		} else if (Input.Pressed("Slot7"))
		{
			Upgrade(UpgradeType.Transport);
		} else if (Input.Pressed("Slot8"))
		{
			Upgrade(UpgradeType.HoldableCapacity);
		}
	}

	public void StartProduction()
	{
		// Set the Upgrade production system
		Upgrades[UpgradeType.Production] = 0;
		Upgrades[UpgradeType.Storage] = 0;
		Upgrades[UpgradeType.Transport] = 0;
		Upgrades[UpgradeType.Upgrader] = 0;
		Upgrades[UpgradeType.HoldableCapacity] = 0;

		// Set the Upgrade production system cost
		UpgradesCost[UpgradeType.Production] = 100;
		UpgradesCost[UpgradeType.Storage] = 100;
		UpgradesCost[UpgradeType.Transport] = 100;
		UpgradesCost[UpgradeType.Upgrader] = 100;
		UpgradesCost[UpgradeType.HoldableCapacity] = 100;

		// Set the Upgrader List and sort it
		Upgrader = GameObject.Components.GetAll<Upgrader>(FindMode.EverythingInDescendants).ToList();
		Upgrader.Sort((x, y) => x.Components.Get<Upgrader>().UpgradeOrder.CompareTo(y.Components.Get<Upgrader>().UpgradeOrder));

		// Disable all the upgraders
		for (int i = 0; i < Upgrader.Count; i++)
			Upgrader[i].GameObject.Enabled = false;

		// Set the Upgrade production system
		foreach ( var upgrade in Upgrades)
		{
			for (int i = 0; i < upgrade.Value; i++)
			{
				Upgrade(upgrade.Key, false);
			}
		}
	}

	public void Upgrade(UpgradeType type, bool increase = true)
	{
		if (increase) Upgrades[type]++;
		switch (type)
		{
			case UpgradeType.Storage:
				UpgradeStorage();
				break;
			case UpgradeType.Production:
				UpgradeProduction();
				break;
			case UpgradeType.Transport:
				UpgradeTransport();
				break;
			case UpgradeType.Decoration:
				UpgradeDecoration();
				break;
			case UpgradeType.Upgrader:
				UpgradeUpgrader();
				break;
			case UpgradeType.HoldableCapacity:
				UpgradeHoldableCapacity();
				break;
			default:
				break;
		}
	}

	public void UpgradeHoldableCapacity()
	{
		HoldableCapacity += 5;
		// Increase the holdable capacity

		// Update the holdable capacity upgrade cost
		UpgradesCost[UpgradeType.HoldableCapacity] = (int)(UpgradesCost[UpgradeType.HoldableCapacity] * 1.2);
	}

	public void UpgradeUpgrader()
	{
		// Error check
		if (Upgrader.Count < Upgrades[UpgradeType.Upgrader])
		{
			// Cannot upgrade anymore
			Upgrades[UpgradeType.Upgrader] = Upgrader.Count;
			return;
		}

		// Enable the next upgrader
		Upgrader[Upgrades[UpgradeType.Upgrader] - 1].GameObject.Enabled = true;
		Upgrader[Upgrades[UpgradeType.Upgrader] - 1].GameObject.Parent.Children.Where(x => x.Tags.Has("conveyor") && !x.Tags.Has("upgrader")).FirstOrDefault().Enabled = false;


		// Update the upgrader upgrade cost
		UpgradesCost[UpgradeType.Upgrader] = (int)(UpgradesCost[UpgradeType.Upgrader] * 1.2);
	}

	public void UpgradeProduction()
	{
		// Increase the production speed
		ProductionSpeed++;

		// Update the production upgrade cost
		UpgradesCost[UpgradeType.Production] = (int)(UpgradesCost[UpgradeType.Production] * 1.2);
	}

	public void UpgradeStorage()
	{
		// Increase the storage capacity
		StorageCapacity = (int)(StorageCapacity * 1.2);

		// Update the storage upgrade cost
		UpgradesCost[UpgradeType.Storage] = (int)(UpgradesCost[UpgradeType.Storage] * 1.2);
	}

	public void UpgradeTransport()
	{
		// Decrease the transport cooldown speed
		TransportCoolDownSpeed = (float)(TransportCoolDownSpeed * 0.9);

		// Update the transport cooldown upgrade cost
		UpgradesCost[UpgradeType.Transport] = (int)(UpgradesCost[UpgradeType.Transport] * 1.2);
	}

	public void UpgradeDecoration()
	{
		// Increase the decoration
	}

	public static string GetItemName(UpgradeType type)
	{
		return type switch
		{
			UpgradeType.Storage => "Storage",
			UpgradeType.Production => "Production",
			UpgradeType.Transport => "Transport",
			UpgradeType.Decoration => "Decoration",
			UpgradeType.Upgrader => "Upgrader",
			UpgradeType.HoldableCapacity => "Holdable Capacity",
			_ => "Other",
		};
	}

	public static UpgradeType GetUpgradeType(string name)
	{
		return name switch
		{
			"Storage" => UpgradeType.Storage,
			"Production" => UpgradeType.Production,
			"Transport" => UpgradeType.Transport,
			"Decoration" => UpgradeType.Decoration,
			"Upgrader" => UpgradeType.Upgrader,
			"Holdable Capacity" => UpgradeType.HoldableCapacity,
			_ => UpgradeType.Other,
		};
	}

	public int GetUpgradeCost(UpgradeType type)
	{
		return UpgradesCost[type];
	}
}

public enum UpgradeType
{
	Storage,
	Production,
	Transport,
	Decoration,
	Upgrader,
	HoldableCapacity,
	Other
}
