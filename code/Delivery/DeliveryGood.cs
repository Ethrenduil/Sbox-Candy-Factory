using Sandbox;

[Category( "Candy Factory - delivery" )]
[Title( "Delivery Good" )]
public sealed class DeliveryGood : Component
{
    [Property] public Dictionary<DeliveryGoods, int> Goods { get; set; } = new Dictionary<DeliveryGoods, int>();
	public bool FromStock { get; set; } = false;
	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

    public void SetGoods(Dictionary<DeliveryGoods, int> goods)
    {
        Goods = goods;
    }

    public void RemoveGoods(Dictionary<DeliveryGoods, int> goods)
    {
        foreach (var good in goods)
        {
            if (Goods.ContainsKey(good.Key))
            {
                Goods[good.Key] -= good.Value;
                if (Goods[good.Key] <= 0)
                    Goods.Remove(good.Key);
            }
        }
    }
    public bool IsEmpty()
    {
        return Goods.Count == 0;
    }

    public void AddGoods(Dictionary<DeliveryGoods, int> goods)
    {
        foreach (var good in goods)
        {
            if (Goods.ContainsKey(good.Key))
            {
                Goods[good.Key] += good.Value;
            }
            else
            {
                Goods.Add(good.Key, good.Value);
            }
        }

        goods.Clear();
    }
}

