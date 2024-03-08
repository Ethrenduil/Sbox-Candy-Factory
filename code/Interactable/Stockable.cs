using Sandbox;
using System;

public class Stockable : AInteractable
{

    [Property] public GameObject Owner { get; set; }

    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to store in {Name}";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
        Type = InteractableType.Storage;
    }
	protected override void OnUpdate()
    {
    }

    public override void OnInteract(GameObject interactor)
    {
    }
}
