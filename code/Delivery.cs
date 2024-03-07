using Sandbox;

[Category("Candy Factory")]
[Title( "Delivery" )]
public sealed class Delivery : Component
{
    [Property] public float DeliveryTime { get; set; } = 10.0f;
    [Property] public float DeliveryProgress { get; set; } = 0.0f;
    [Property] public DeliveryStatus Status { get; set; } = DeliveryStatus.None;
    [Property] public GameObject DeliveryDestination { get; set; }
    [Property] public GameObject DeliveryPrefab { get; set; }
    [Property] public DeliveryHud DeliveryHud { get; set; }
    [Property] public Dictionary<DeliveryGoods, int> Goods { get; set; } = new Dictionary<DeliveryGoods, int>();
	protected override void OnUpdate()
	{
		base.OnUpdate();

        // Trigger Order Delivery // Test
        if (Input.Pressed("drop"))
        {
            StartDelivery(new Dictionary<DeliveryGoods, int> { { DeliveryGoods.Sugar, 10 }, { DeliveryGoods.Flour, 10 }, { DeliveryGoods.Milk, 10 } });
        }
        //End Test

        if (Status == DeliveryStatus.InProgress)
        {
            DeliveryProgress += Time.Delta;
            DeliveryHud.SetProgress((DeliveryTime - DeliveryProgress).ToString("0") + "s left");
            if (DeliveryProgress >= DeliveryTime)
            {
                Status = DeliveryStatus.Delivered;
                DeliveryProgress = 0.0f;
                SpawnDelivery();
            }
        }
	}

    // Start a delivery with a list of goods
    public void StartDelivery(Dictionary<DeliveryGoods, int> goods)
    {
        Status = DeliveryStatus.InProgress;
        Goods = goods;
    }

    // Spawn a delivery object at the delivery destination
    public void SpawnDelivery()
    {
        var temp = DeliveryPrefab.Clone(DeliveryDestination.Transform.World);
        temp.Components.Get<DeliveryGood>().SetGoods(Goods);
        temp.NetworkSpawn();
        DeliveryHud.SetProgress(null);
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
    Milk
}

