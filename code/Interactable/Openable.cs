using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;


public class Openable : AInteractable
{
    [Property] override public string Name { get; set; }
    [Property] override public string Description { get; set; }
    [Property] override public InteractableType Type { get; set; } = InteractableType.Resource;
    [Property] override public string PrefabPath { get; set; }
    [Property] override public bool IsInteracted { get; set; }
    [Property] public bool IsOpen { get; set; }
    [Property] public OpenableSide Side { get; set; }
    protected override void OnStart()
    {
        base.OnStart();
        Name = GameObject.Name;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnInteract(GameObject interactor)
    {
        if (!IsInteracted)
        {
            IsInteracted = true;
            Log.Info("Interacted with " + Name);
            Rotation direction;
            int rotation;
            if (IsOpen)
            {
                rotation = -90;
            }
            else
            {
                rotation = 90;
            }
            switch (Side)
            {
                case OpenableSide.Left:
                    direction = GameObject.Transform.Rotation.RotateAroundAxis(Vector3.Up, rotation);
                    break;
                case OpenableSide.Right:
                    direction = GameObject.Transform.Rotation.RotateAroundAxis(Vector3.Down, rotation);
                    break;
                default:
                    direction = GameObject.Transform.Rotation;
                    break;
            }
            if (IsOpen)
            {
                IsOpen = false;
                GameObject.Transform.Rotation = direction;
            }
            else
            {
                IsOpen = true;
                GameObject.Transform.Rotation = direction;
            }
            IsInteracted = false;
        }

    }
}


public enum OpenableSide
{
    Left,
    Right,
    Top,
    Bottom,
    Front,
    Back
}

