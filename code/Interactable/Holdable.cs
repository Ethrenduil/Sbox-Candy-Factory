using Sandbox;
using Sandbox.Engine.Utility.RayTrace;
using System;
public class Holdable : AInteractable
{
    [Property] public override string Name { get; set; }
    [Property] public override string Description { get; set; }
    [Property] public override InteractableType Type { get; set; } = InteractableType.Resource;
    [Property] public override string PrefabPath { get; set; }
    [Property] public override bool IsInteracted { get; set; }

    [Sync] GameObject HoldRelative { get; set; }
    private const float ForwardOffset = 70f;
    private const float VerticalOffset = 40f;

    protected override void OnStart()
    {
        base.OnStart();
        Description = $"Press E to pick up {Name}";
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
    }

  

    protected override void OnUpdate()
    {
        Log.Info($"Owner is {Network.OwnerId}");

        if (IsProxy || !IsInteracted || Interactor == null) return;

        PlayerInteract playerInteract = Interactor.Components.Get<PlayerInteract>();
        Transform.Position = HoldRelative.Transform.Position + HoldRelative.Transform.Rotation.Forward * new Vector3(0, 0, VerticalOffset);
        Transform.Rotation = HoldRelative.Parent.Transform.Rotation;

        if (Input.Pressed("use") && playerInteract.InteractionCooldownPassed())
        {
            playerInteract.StartInteract();
            OnInteract(Interactor);
        }
    }

    public override void OnInteract(GameObject interactor)
    {
        if (IsInteracted && interactor == Interactor)
        {
            IsInteracted = false;
            Rigidbody rigidbody = GameObject.Components.Get<Rigidbody>(FindMode.DisabledInSelfAndChildren);
            rigidbody.Enabled = true;
            GameObject.SetParent(null, true);
            GameObject.Network.DropOwnership();

            Vector3 dropPosition = Interactor.Transform.Position + Interactor.Components.Get<Player>().Camera.Transform.Rotation.Forward * ForwardOffset;
            dropPosition.z = Math.Max(dropPosition.z, 40);
            GameObject.Transform.Position = dropPosition;

            var animationHelper = interactor.Components.Get<Player>().AnimationHelper;
            animationHelper.IkLeftHand = null;
            animationHelper.IkRightHand = null;

            Interactor = null;
            HoldRelative = null;
        }
        else
        {
            IsInteracted = true;
            Interactor = interactor;
            Rigidbody rigidbody = GameObject.Components.Get<Rigidbody>();
            rigidbody.Enabled = false;
            GameObject.SetParent(interactor, true);
            GameObject.Network.TakeOwnership();

            var player = interactor.Components.Get<Player>();
            var animationHelper = player.AnimationHelper;

            animationHelper.IkLeftHand = GameObject.Children.FirstOrDefault(x => x.Name == "LeftHandSlot");
            animationHelper.IkRightHand = GameObject.Children.FirstOrDefault(x => x.Name == "RightHandSlot");
            HoldRelative = Interactor.Children.FirstOrDefault(x => x.Name == "Body")?.Children.FirstOrDefault(x => x.Name == "pelvis") ?? Interactor;
        }
    }
}
