using System.Diagnostics;
using System.Threading.Tasks;
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
        Description = IsOpen ? "Press E to close" : "Press E to open";
    }

    public override async void OnInteract(GameObject interactor)
    {
        if (!IsInteracted)
        {
            IsInteracted = true;
            Log.Info("Interacted with " + Name);
            Rotation direction = CalculateRotation();
            IsOpen = !IsOpen;
            Description = IsOpen ? "Press E to close" : "Press E to open";
            await RotateOverTime(direction, 1f);
            IsInteracted = false;
        }
    }

    private Rotation CalculateRotation()
    {
        int rotation = IsOpen ? -90 : 90;
        switch (Side)
        {
            case OpenableSide.Left:
                return GameObject.Transform.Rotation.RotateAroundAxis(Vector3.Up, rotation);
            case OpenableSide.Right:
                return GameObject.Transform.Rotation.RotateAroundAxis(Vector3.Down, rotation);
            default:
                return GameObject.Transform.Rotation;
        }
    }

    private async Task RotateOverTime(Rotation targetRotation, float duration)
    {
        var startRotation = GameObject.Transform.Rotation;
        var startTime = Time.Now;
        while (Time.Now - startTime < duration)
        {
            var t = (Time.Now - startTime) / duration;
            GameObject.Transform.Rotation = Rotation.Lerp(startRotation, targetRotation, t);
            await Task.Frame();
        }
        GameObject.Transform.Rotation = targetRotation;
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
