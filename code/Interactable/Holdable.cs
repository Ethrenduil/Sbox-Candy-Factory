using Sandbox;
using Sandbox.Engine.Utility.RayTrace;
using System;

[Category( "Candy Factory - interactable")]
public class Holdable : AInteractable
{
    private GameObject HoldRelative { get; set; }
    private const float ForwardOffset = 70f;
    private const float VerticalOffset = 40f;

    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to pick up {Name}";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
        Type = InteractableType.Resource;
    }

	protected override void OnUpdate()
	{
		base.OnUpdate();

        if (Transform.Position.z < 0)
        {
            Transform.Position = new Vector3(Transform.Position.x, Transform.Position.y, 40);
        }
	}
	protected override void OnFixedUpdate()
    {
        // Exit early if this is a proxy or not currently interacted
        if (IsProxy || !IsInteracted || Interactor == null) return;

        // Update the position and rotation of the holdable object based on the relative object
        Transform.Position = HoldRelative.Transform.Position + HoldRelative.Transform.Rotation.Forward * new Vector3(0, 0, VerticalOffset);
        Transform.Rotation = HoldRelative.Parent.Transform.Rotation;
    }

    public override void OnInteract(GameObject interactor)
    {
        // If already interacted and the same interactor, release the holdable object
        if (IsInteracted && interactor == Interactor)
        {
            ReleaseHoldable();
        }
        else
        {
            // If not interacted, pick up the holdable object
            PickUpHoldable(interactor);
        }
    }

    public override bool CanInteract(GameObject interactor)
    {
        // Check if player is Carrying an object and if the object is not already being interacted with
        if (interactor.Components.Get<PlayerInteract>().IsCarrying && !IsInteracted) return false;

        return true;
    }

    private void ReleaseHoldable()
    {
        // Reset interaction state
        IsInteracted = false;

        // Enable the Rigidbody for physics simulation
        Rigidbody rigidbody = GameObject.Components.Get<Rigidbody>(FindMode.DisabledInSelfAndChildren);
        rigidbody.Enabled = true;

        Vector3 dropPosition = Interactor.Transform.Position + Interactor.Components.Get<Player>().Camera.Transform.Rotation.Forward * ForwardOffset;
        dropPosition.z = Math.Max(dropPosition.z, 40);
        GameObject.Transform.Position = dropPosition;
        
        // Reset interactor and relative object
        Interactor = null;
        HoldRelative = null;

        // Detach from the parent, drop ownership, and set the drop position
        GameObject.SetParent(null, true);
        // GameObject.Network.DropOwnership();
    }

    private void PickUpHoldable(GameObject interactor)
    {
        // Attach to the interactor, take ownership, and set the animation helper hands
        GameObject.SetParent(interactor, true);
        GameObject.Network.TakeOwnership();

        // Set the current interactor and disable Rigidbody for physics simulation
        Interactor = interactor;
        Rigidbody rigidbody = GameObject.Components.Get<Rigidbody>();
        rigidbody.Enabled = false;

        // Set the holdable's relative object
        HoldRelative = Interactor.Children.FirstOrDefault(x => x.Name == "Body")?.Children.FirstOrDefault(x => x.Name == "pelvis") ?? Interactor;
        
        // Set the interaction state
        IsInteracted = true;
    }

	[Broadcast]
	public void Open()
	{
		Components.Get<SkinnedModelRenderer>().Set( "Opening", true );
	}
}
