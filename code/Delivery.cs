using Sandbox;

[Category("Candy Factory")]
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
    [Property] public GameObject DeliveryCarPrefab { get; set; }
    [Property] public DeliveryCar DeliveryCar { get; set; }
    [Property] public GameObject DeliveryCarSpawn { get; set; }
    [Property] public GameObject Receiver { get; set; }

    protected override void OnAwake()
    {
        base.OnAwake();
        if (IsProxy) return;

        DeliveryHud = Scene.GetAllComponents<DeliveryHud>().FirstOrDefault();
        DeliveryCarSpawn = Scene.Children.Where(c => c.Name == "Spawn").FirstOrDefault().Children.Where(c => c.Name == "DeliverySpawn").FirstOrDefault();
    }
	protected override void OnUpdate()
	{
		base.OnUpdate();

        if (IsProxy) return;

        // Trigger Order Delivery // Test
        if (Input.Pressed("drop") && Status == DeliveryStatus.None)
        {
            StartDelivery(new Dictionary<DeliveryGoods, int> { { DeliveryGoods.Sugar, 10 }, { DeliveryGoods.Flour, 10 }, { DeliveryGoods.Milk, 10 } });
        }
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
            DeliveryHud.SetProgress(null);
            DeliveryCar = null;
            Status = DeliveryStatus.None;
        }
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
        DeliveryCar.StartDelivery(DeliveryDestination.Transform.Position);

        // Set the delivery progress
        DeliveryHud.SetProgress("Delivery in progress");
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
        DeliveryCar.StopDelivery(DeliveryCarSpawn.Transform.Position);

        // Set the delivery status
        Status = DeliveryStatus.Delivered;
    }

}

public enum DeliveryStatus
{
    None,
    InProgress,
    Delivered,
    Failed
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

