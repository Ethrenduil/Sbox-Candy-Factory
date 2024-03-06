using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;


public class Holdable : AInteractable
{
    [Property] override public string Name { get; set; }
    [Property] override public string Description { get; set; }
    [Property] override public InteractableType Type { get; set; } = InteractableType.Resource;
    [Property] override public string PrefabPath { get; set; }
    [Property] override public bool IsInteracted { get; set; }
    private GameObject HoldRelative { get; set; }
    private float forwardOffset = 70f;
    private float verticalOffset = 40f;
    protected override void OnStart()
    {
        base.OnStart();
        Description = "Press E to pick up " + Name;
    }

    protected override void OnUpdate()
    {
        if (IsProxy)
            return;

        if (IsInteracted && Interactor != null)
        {
            PlayerInteract playerInteract = Interactor.Components.Get<PlayerInteract>();
            Transform.Position = HoldRelative.Transform.Position + HoldRelative.Transform.Rotation.Forward * new Vector3(0,0,verticalOffset);
            Transform.Rotation = HoldRelative.Parent.Transform.Rotation;
            if (Input.Pressed("use") && playerInteract.InteractionCooldownPassed())
            {
                playerInteract.StartInteract();
                OnInteract(Interactor);
            }
        }
    }

    public override void OnInteract(GameObject interactor)
    {
        if (IsInteracted && interactor == Interactor)
        {
            IsInteracted = false;
            GameObject.Components.Get<Rigidbody>(FindMode.DisabledInSelfAndChildren).Enabled = true;
            GameObject.SetParent(null, true);
            GameObject.Network.DropOwnership();
            Log.Info(HoldRelative.Transform.Rotation.Forward);
            Vector3 dropPosition = Interactor.Transform.Position + Interactor.Components.Get<Player>().Camera.Transform.Rotation.Forward * forwardOffset;
            if (dropPosition.z < 0)
                dropPosition.z = 40;
            GameObject.Transform.Position = dropPosition;

            var ca = interactor.Components.Get<Player>().AnimationHelper;
            ca.IkLeftHand = null;
            ca.IkRightHand = null;

            Interactor = null;
            HoldRelative = null;
        } else {
            IsInteracted = true;
            Interactor = interactor;
            GameObject.Components.Get<Rigidbody>().Enabled = false;
            GameObject.SetParent(interactor, true);
            GameObject.Network.TakeOwnership();

            var ply = interactor.Components.Get<Player>();

            ply.AnimationHelper.IkLeftHand =  GameObject.Children.FirstOrDefault( x => x.Name == "LeftHandSlot");
		    ply.AnimationHelper.IkRightHand = GameObject.Children.FirstOrDefault( x => x.Name == "RightHandSlot");
            HoldRelative = Interactor.Children.FirstOrDefault( x => x.Name == "Body").Children.FirstOrDefault(x => x.Name == "pelvis") ?? Interactor;
        }

    }
}
