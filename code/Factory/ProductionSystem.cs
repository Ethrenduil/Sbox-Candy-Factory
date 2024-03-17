using System;
using Sandbox;

public sealed class ProductionSystem : Component
{
	[Property] public Factory Factory { get; set; }
	[Property] public Dictionary<UpgradeType, int> Upgrades { get; set; } = new Dictionary<UpgradeType, int>();
	[Property] public Dictionary<UpgradeType, int> UpgradesCost { get; set; } = new Dictionary<UpgradeType, int>();
	[Property] public List<ProductionLine> ProductionLines { get; set; } = new List<ProductionLine>();
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
		base.OnUpdate();
	}

	public void StartProduction()
	{
		// Set the Upgrade production system
		Upgrades[UpgradeType.ProductionSpeed] = 0;
		Upgrades[UpgradeType.ProductionLine] = 1;
		Upgrades[UpgradeType.Storage] = 0;
		Upgrades[UpgradeType.Transport] = 0;
		Upgrades[UpgradeType.Upgrader] = 0;
		Upgrades[UpgradeType.HoldableCapacity] = 0;

		// Set the Upgrade production system cost
		UpgradesCost[UpgradeType.ProductionSpeed] = 100;
		UpgradesCost[UpgradeType.ProductionLine] = 100;
		UpgradesCost[UpgradeType.Storage] = 100;
		UpgradesCost[UpgradeType.Transport] = 100;
		UpgradesCost[UpgradeType.Upgrader] = 100;
		UpgradesCost[UpgradeType.HoldableCapacity] = 100;

		// Set the Production Lines
		SetUpProductionLines();

		// Set the Upgrade production system already upgraded
		foreach ( var upgrade in Upgrades)
		{
			for (int i = 0; i < upgrade.Value; i++)
			{
				if (upgrade.Key == UpgradeType.ProductionLine || upgrade.Key == UpgradeType.Upgrader)
				{
					Upgrade(upgrade.Key, false, i + 1);
				}
				else
				Upgrade(upgrade.Key, false);
			}
		}
	}

	public void Upgrade(UpgradeType type, bool increase = true, int line = 0)
	{
		if (increase) Upgrades[type]++;
		switch (type)
		{
			case UpgradeType.Storage:
				UpgradeStorage();
				break;
			case UpgradeType.ProductionSpeed:
				UpgradeProductionSpeed();
				break;
			case UpgradeType.Transport:
				UpgradeTransport();
				break;
			case UpgradeType.Decoration:
				UpgradeDecoration();
				break;
			case UpgradeType.Upgrader:
				UpgradeUpgrader(line);
				break;
			case UpgradeType.HoldableCapacity:
				UpgradeHoldableCapacity();
				break;
			case UpgradeType.ProductionLine:
				UpgradeProductionLine(line);
				break;
			default:
				break;
		}
	}


	public void UpgradeProductionLine(int line)
	{
		// Error check
		if (ProductionLines.Count < line || ProductionLines[line - 1].IsActive)
		{
			// Cannot upgrade anymore
			Log.Info("Cannot upgrade anymore");
			return;
		}

		// Enable the next production line and set it to active
		ProductionLines[line - 1].IsActive = true;
		ProductionLines[line - 1].ProductionLineObject.Enabled = true;

		// Set other parameters

		// Update the production line upgrade cost
		if (ProductionLines.Count > line)
			UpgradesCost[UpgradeType.ProductionLine] = ProductionLines[line].Price;
		else
			UpgradesCost[UpgradeType.ProductionLine] = -1;
	}
	
	public void UpgradeHoldableCapacity()
	{
		HoldableCapacity += 5;
		// Increase the Box Capacity

		// Update the Box Capacity upgrade cost
		UpgradesCost[UpgradeType.HoldableCapacity] = (int)(UpgradesCost[UpgradeType.HoldableCapacity] * 1.2);
	}

	public void UpgradeUpgrader(int line)
	{
		// Error check
		if (!ProductionLines[line - 1].CanUpgrade())
		{
			// Cannot upgrade anymore
			return;
		}

		// Enable the next upgrader
		ProductionLines[line - 1].Upgrade();


		// Update the upgrader upgrade cost
		UpgradesCost[UpgradeType.Upgrader] = (int)(UpgradesCost[UpgradeType.Upgrader] * 1.2);
	}

	public void UpgradeProductionSpeed()
	{
		// Increase the production speed
		ProductionSpeed++;

		// Update the production upgrade cost
		UpgradesCost[UpgradeType.ProductionSpeed] = (int)(UpgradesCost[UpgradeType.ProductionSpeed] * 1.2);
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
			UpgradeType.ProductionSpeed => "Production Speed",
			UpgradeType.ProductionLine => "Production Line",
			UpgradeType.Transport => "Transport",
			UpgradeType.Decoration => "Decoration",
			UpgradeType.Upgrader => "Upgrader",
			UpgradeType.HoldableCapacity => "Box Capacity",
			_ => "Other",
		};
	}

	public static UpgradeType GetUpgradeType(string name)
	{
		return name switch
		{
			"Storage" => UpgradeType.Storage,
			"Production Speed" => UpgradeType.ProductionSpeed,
			"Production Line" => UpgradeType.ProductionLine,
			"Transport" => UpgradeType.Transport,
			"Decoration" => UpgradeType.Decoration,
			"Upgrader" => UpgradeType.Upgrader,
			"Box Capacity" => UpgradeType.HoldableCapacity,
			_ => UpgradeType.Other,
		};
	}

	public int GetUpgradeCost(UpgradeType type)
	{
		return UpgradesCost[type];
	}

	public void SetUpProductionLines()
	{
		// Set the production lines
		ProductionLines = GameObject.Components.GetAll<ProductionLine>(FindMode.EverythingInDescendants).ToList();
		// Sort the production lines by ProductionLine Order
		ProductionLines.Sort((x, y) => x.Order.CompareTo(y.Order));
	}

	public UpgradeType GetProductionLineUpgradeType(string lineStr)
	{
		var line = int.Parse(lineStr.Split(' ')[1]);

		if (!ProductionLines[line - 1].IsActive) return UpgradeType.ProductionLine;
		
		return UpgradeType.Upgrader;
	}

	public Dictionary<string, int> GetProdList()
	{
		var result = new Dictionary<string, int>();
		var i = 0;
		foreach (var line in ProductionLines)
		{
			i++;
			if (line.IsActive)
			{
				foreach (var upgrade in line.Upgrader)
				{
					if (!upgrade.Value)
					{
						result.Add($"Line {i}", upgrade.Key.UpgradePrice);
						break;
					}
				}
				if(!result.ContainsKey($"Line {i}")) result.Add($"Line {i}", -1);
			} else
			{
				result.Add($"Line {i}", line.Price);
			}
		}
		return result;
	}

	public bool IsProductionLineLocked(string lineStr)
	{
		var line = int.Parse(lineStr.Split(' ')[1]);
		
		if (ProductionLines[line - 1].IsActive) return false;

		if (line > 1 && !ProductionLines[line - 2].FullUpgraded()) return true;

		return false;
	}

	public bool IsProductionLineFullUpgraded(string lineStr)
	{
		var line = int.Parse(lineStr.Split(' ')[1]);
		return ProductionLines[line - 1].FullUpgraded();
	}

	public Dictionary<string, int> GetFacList()
	{
		var result = new Dictionary<string, int>();
		foreach (var upgrade in Upgrades)
		{
			if (upgrade.Key == UpgradeType.ProductionLine || upgrade.Key == UpgradeType.Upgrader) continue;
			result.Add(GetItemName(upgrade.Key), UpgradesCost[upgrade.Key]);
		}
		return result;
	}
}

public enum UpgradeType
{
	Storage,
	ProductionSpeed,
	Transport,
	Decoration,
	ProductionLine,
	Upgrader,
	HoldableCapacity,
	Other
}
