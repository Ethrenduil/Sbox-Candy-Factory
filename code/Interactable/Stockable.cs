using Sandbox;
using System;

[Category("Candy Factory - interactable")]
public class Stockable : AInteractable
{
    // temporary stock
    [Property] public Factory FactoryPlayer { get; set; }
    [Property] public GameObject boxPrefab { get; set; }
    [Property] public StoragePanel StoragePanel { get; set; }

    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to store in {Name}";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
        Type = InteractableType.Storage;
        FactoryPlayer = GameObject.Parent.Components.Get<Factory>();
        StoragePanel = GameObject.Parent.Components.Get<StoragePanel>(FindMode.EverythingInChildren);
    }
	protected override void OnUpdate()
    {
    }

    public override void OnInteract(GameObject interactor)
    {
        IsInteracted = true;

        var box = interactor.Components.Get<Holdable>(FindMode.EverythingInSelfAndChildren) ?? null;
        Interactor = interactor;
        if (box != null)
        {
            StockGoods();
        }
        else
        {
            StoragePanel.OpenStorageWindow(this);
        }

        // Reset interaction state
        IsInteracted = false;
        
    }

    public override bool CanInteract(GameObject interactor)
    {
        // If the player is already carrying an object, return true
        if (interactor.Components.Get<PlayerInteract>().IsCarrying) return true;


        return true;
    }

    public void StockGoods()
    {
        var box = Interactor.Components.Get<DeliveryGood>(FindMode.EverythingInSelfAndChildren);
        if (box == null) return;

        var overflow = FactoryPlayer.AddStockFromDictionary(box.Goods);
        if (overflow != null)
        {
            SpawnOverflowBox(overflow);
        }
        // Destroy the delivery box
        box.GameObject.Destroy();
    }

    public bool RetrieveGoods(Dictionary<DeliveryGoods, int> goods)
    {
        var box = boxPrefab.Clone();
        var delivery = box.Components.Get<DeliveryGood>();
        if (!FactoryPlayer.RemoveStockFromDictionary(goods)) return false;
        delivery.AddGoods(goods);
		delivery.FromStock = true;
        box.Components.Get<Holdable>().OnInteract(Interactor);
        box.NetworkSpawn();
        box.Network.TakeOwnership();
        return true;
    }

    private void SpawnOverflowBox(Dictionary<DeliveryGoods, int> stock)
	{
		var box = boxPrefab.Clone(GameObject.Transform.Position + new Vector3(0, -100, 20));
		var delivery = box.Components.Get<DeliveryGood>();
		delivery.AddGoods(stock);
		delivery.FromStock = true;
		box.NetworkSpawn();
		box.Network.TakeOwnership();
	}
}
