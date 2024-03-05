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
    protected override void OnStart()
    {
        base.OnStart();
        Description = "Press E to pick up " + Name;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (IsInteracted && Interactor != null)
        {
            PlayerInteract playerInteract = Interactor.Components.Get<PlayerInteract>();
            Vector3 offsetInFront = Interactor.Components.Get<Player>().EyeAngles.ToRotation().Forward * 100;
            Vector3 verticalOffset = new(0, 0, 50);
            Transform.Position = Interactor.Transform.Position + offsetInFront + verticalOffset;
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
            Interactor = null;
            GameObject.Transform.Position = GameObject.Transform.Position;
            GameObject.Components.Get<Collider>(FindMode.DisabledInSelfAndChildren).Enabled = true;
        } else {
            IsInteracted = true;
            Interactor = interactor;
            GameObject.Components.Get<Collider>().Enabled = false;
        }

    }
}
