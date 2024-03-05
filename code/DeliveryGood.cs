using Sandbox;

[Category("Candy Factory")]
[Title( "Delivery Good" )]
public sealed class DeliveryGood : Component
{
    [Property] public Dictionary<DeliveryGoods, int> Goods { get; set; } = new Dictionary<DeliveryGoods, int>();
	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

    public void SetGoods(Dictionary<DeliveryGoods, int> goods)
    {
        Goods = goods;
    }
}

