using System;
using Sandbox;

public sealed class ProductionSystem : Component
{
	[Property] public Factory Factory { get; set; }
	[Property] public Dictionary<UpgradeType, int> Upgrades { get; set; } = new Dictionary<UpgradeType, int>();
	[Property] public List<GameObject> Upgrader { get; set; }
	[Property] public int ProductionSpeed { get; set; } = 0;
	[Property] public int StorageCapacity { get; set; } = 20;
	[Property] public int HoldableCapacity { get; set; } = 5;
	[Property] public float TransportCoolDownSpeed { get; set; } = 30.0f;
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
		Upgrader[Upgrades[UpgradeType.Upgrader] - 1].Enabled = true;
	}

	public void UpgradeProduction()
	{
		// Increase the production speed
		ProductionSpeed++;
	}

	public void UpgradeStorage()
	{
		// Increase the storage capacity
		StorageCapacity = (int)(StorageCapacity * 1.2);
	}

	public void UpgradeTransport()
	{
		// Decrease the transport cooldown speed
		TransportCoolDownSpeed = (float)(TransportCoolDownSpeed * 0.9);
	}

	public void UpgradeDecoration()
	{
		// Increase the decoration
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