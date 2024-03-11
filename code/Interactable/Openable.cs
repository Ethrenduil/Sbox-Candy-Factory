using System.Diagnostics;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;

[Category( "Candy Factory - interactable")]
public class Openable : AInteractable
{
    [Property] public float OpeningTime { get; set; } = 1f;
    [Property][Sync] public bool IsOpen { get; set; }
    [Property] public OpenableSide Side { get; set; }

    protected override void OnStart()
    {
        base.OnStart();
        Description = IsOpen ? "Press E to close" : "Press E to open";
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
        Type = InteractableType.Building;
    }

    public override async void OnInteract(GameObject interactor)
    {
        if (!IsInteracted)
        {
            IsInteracted = true;
			GameObject.Network.TakeOwnership();
            Rotation direction = CalculateRotation();
            IsOpen = !IsOpen;
            Description = IsOpen ? "Press E to close" : "Press E to open";
            await RotateOverTime(direction, OpeningTime);
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
