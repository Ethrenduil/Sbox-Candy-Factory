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
        Name = GameObject.Name;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (IsInteracted && Interactor != null)
        {
            Vector3 offsetInFront = Interactor.Components.Get<Player>().EyeAngles.ToRotation().Forward * 100;
            Vector3 verticalOffset = new(0, 0, 50);
            Transform.Position = Interactor.Transform.Position + offsetInFront + verticalOffset;

            if (Input.Pressed("use") && Time.Now - InteractionTime > InteractionCooldown)
            {
                InteractionTime = Time.Now;
                OnInteract(Interactor);
            }
        }
    }

    public override void OnInteract(GameObject interactor)
    {
        if (IsInteracted && interactor == Interactor)
        {
            IsInteracted = false;
            Log.Info("Stopped interacting with " + Name);
            Interactor = null;
            GameObject.Transform.Position = GameObject.Transform.Position;
            GameObject.Components.Get<Collider>(FindMode.DisabledInSelfAndChildren).Enabled = true;
        } else {
            IsInteracted = true;
            Log.Info("Interacted with " + Name);
            Interactor = interactor;
            GameObject.Components.Get<Collider>().Enabled = false;
            InteractionTime = Time.Now;
        }

    }
}
