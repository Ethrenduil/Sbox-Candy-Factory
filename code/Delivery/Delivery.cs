using Sandbox;

[Category( "Candy Factory - delivery" )]
[Title( "Delivery" )]
public sealed class Delivery : Component
{
    [Property] public float DeliveryTime { get; set; } = 10.0f;
    [Property] public float DeliveryProgress { get; set; } = 0.0f;
    [Property] public DeliveryStatus Status { get; set; } = DeliveryStatus.None;
    [Property] public GameObject DeliveryDestination { get; set; }
    [Property] public GameObject DeliveryBoxPrefab { get; set; }
    [Property] public DeliveryHud DeliveryHud { get; set; }
    [Property] public Dictionary<DeliveryGoods, int> Goods { get; set; } = new Dictionary<DeliveryGoods, int>();
    [Property] public Dictionary<DeliveryGoods, int> GoodsPrices { get; set; }
    [Property] public GameObject DeliveryCarPrefab { get; set; }
    [Property] public DeliveryCar DeliveryCar { get; set; }
    [Property] public GameObject DeliveryCarSpawn { get; set; }
    [Property] public GameObject DeliveryCarSpawnEnd { get; set; }
    [Property] public GameObject Receiver { get; set; }
    [Property] public float DeliveryCooldown { get; set; } = 30.0f;
    [Property] public ProductionSystem ProductionSystem { get; set; }
	private QuestSystem questSystem;


    protected override void OnAwake()
    {
        base.OnAwake();
        if (IsProxy) return;

        DeliveryHud = Scene.GetAllComponents<DeliveryHud>().FirstOrDefault();
        DeliveryCarSpawn = Scene.Children.Where(c => c.Name == "Spawn").FirstOrDefault().Children.Where(c => c.Name == "DeliverySpawn").FirstOrDefault();
        DeliveryCarSpawnEnd = Scene.Children.Where(c => c.Name == "Spawn").FirstOrDefault().Children.Where(c => c.Name == "DeliverySpawnEnd").FirstOrDefault();
        SetUpGoodsPrices();
    }
	protected override void OnUpdate()
	{
		base.OnUpdate();

        if (IsProxy) return;

        //End Test

        if (Status == DeliveryStatus.InProgress)
        {
            DeliveryProgress += Time.Delta;
            if (DeliveryCar.IsArrived())
            {
                Status = DeliveryStatus.Delivered;
                DeliveryProgress = 0.0f;
                SpawnDelivery();
            }
        }

        if (Status == DeliveryStatus.Delivered && !DeliveryCar.Active)
        {
            OnDeliveryFinished();
        }
	}

    public void OnDeliveryFinished()
    {
        DeliveryHud.SetProgress("Delivery cooldown: " + DeliveryCooldown.ToString("0.0") + "s");
        DeliveryCooldown -= Time.Delta;
        if (DeliveryCooldown <= 0)
        {
            Status = DeliveryStatus.None;
            DeliveryCooldown = ProductionSystem.TransportCoolDownSpeed;
            DeliveryHud.SetProgress("Delivery available");
        }
    }

    public bool CanDeliver()
    {
        return Status == DeliveryStatus.None;
    }
    // Start a delivery with a list of goods
    public void StartDelivery(Dictionary<DeliveryGoods, int> goods)
    {
        // Set the delivery status and goods
        Status = DeliveryStatus.InProgress;
        Goods = goods;

        // Spawn and start the delivery car
        var GOCar = DeliveryCarPrefab.Clone(DeliveryCarSpawn.Transform.Position, DeliveryCarSpawn.Transform.Rotation);
        GOCar.NetworkSpawn();
        DeliveryCar = GOCar.Components.Get<DeliveryCar>();
        DeliveryDestination = GameObject.Children.Where(c => c.Name == "DeliveryDestination").FirstOrDefault();
        DeliveryCar.StartDelivery(DeliveryDestination.Transform.World.Position);    

        // Set the delivery progress
        DeliveryHud.SetProgress("Delivery in progress");
		ProductionSystem = GameObject.Components.Get<ProductionSystem>(FindMode.EverythingInChildren);
    }

    // Spawn a delivery object at the delivery destination
    public void SpawnDelivery()
    {
        // Spawn the delivery box and set it up
        var temp = DeliveryBoxPrefab.Clone(DeliveryDestination.Transform.World);
        temp.Components.Get<DeliveryGood>().SetGoods(Goods);
        temp.NetworkSpawn();
        temp.Network.TakeOwnership();

        // Reset the delivery status
        DeliveryHud.SetProgress("Delivery complete");

        // Return the delivery car to the spawn
        DeliveryCar.StopDelivery(DeliveryCarSpawnEnd.Transform.Position);

        // Set the delivery status
        Status = DeliveryStatus.Delivered;

		questSystem ??= Scene.GetAllComponents<QuestSystem>().FirstOrDefault();
		foreach (QuestObjective objective in questSystem.CurrentQuest.Objectives)
		{
			if (objective.Type == ObjectiveType.WaitDelivery)
			{
			    questSystem.CompleteObjective(objective);
			}
		}
        
        // Set Delivery Cooldown
        DeliveryCooldown = ProductionSystem.TransportCoolDownSpeed;
    }

    public void SetUpGoodsPrices()
    {
        GoodsPrices = new Dictionary<DeliveryGoods, int> {
            { DeliveryGoods.Sugar, 1 },
            { DeliveryGoods.Cacao, 3 },
            { DeliveryGoods.Milk, 2 },
            { DeliveryGoods.Vanilla, 5 }
        };
    }

    public static string GetItemName(DeliveryGoods item)
    {
		return item switch
		{
			DeliveryGoods.Sugar => "Sugar",
			DeliveryGoods.Cacao => "Cacao",
			DeliveryGoods.Milk => "Milk",
			DeliveryGoods.Vanilla => "Vanilla",
			_ => "Unknown",
		};
	}

    public static DeliveryGoods GetDeliveryGoodsFromString(string item)
    {
        return item switch
        {
            "Sugar" => DeliveryGoods.Sugar,
            "Cacao" => DeliveryGoods.Cacao,
            "Milk" => DeliveryGoods.Milk,
            "Vanilla" => DeliveryGoods.Vanilla,
            _ => DeliveryGoods.None,
        };
    }

    public static Dictionary<DeliveryGoods, int> GetDictionaryGoodsFromString(Dictionary<string, int> goods)
    {
        var result = new Dictionary<DeliveryGoods, int>();
        foreach (var item in goods)
        {
            result.Add(GetDeliveryGoodsFromString(item.Key), item.Value);
        }
        return result;
    }
}

public enum DeliveryStatus
{
    None,
    InProgress,
    Delivered,
    Failed,
    CoolDown,
}

public enum DeliveryGoods
{
    None,
    Sugar,
    Flour,
    Milk,
    Egg,
    Cacao,
    Vanilla,
}

