using Sandbox;
using System;

[Category("Candy Factory - interactable")]
public class Stockable : AInteractable
{

    [Property] public GameObject Owner { get; set; }

    // temporary stock
    [Property] public Factory Stock { get; set; }

    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to store in {Name}";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
        Type = InteractableType.Storage;
        Stock = GameObject.Parent.Components.Get<Factory>();
    }
	protected override void OnUpdate()
    {
    }

    public override void OnInteract(GameObject interactor)
    {
        IsInteracted = true;
        var box = interactor.Components.Get<DeliveryGood>(FindMode.EverythingInSelfAndChildren);
        if (box == null) return;

        var goods = box.Goods;
        foreach (var good in goods)
        {
            if (Stock.Stock.ContainsKey(good.Key))
            {
                Stock.Stock[good.Key] += good.Value;
            }
            else
            {
                Stock.Stock.Add(good.Key, good.Value);
            }
        }

        // Destroy the delivery box
        box.GameObject.Destroy();

        // Reset interaction state
        IsInteracted = false;
        
    }

    public override bool CanInteract(GameObject interactor)
    {
        // If the player is already carrying an object, return true
        if (interactor.Components.Get<PlayerInteract>().IsCarrying) return true;

        return false;
    }
}
