using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;


public class Holdable : IInteractable
{
    [Property] public override string Name { get; set; }
    [Property] public override string Description { get; set; }
    protected override void OnStart()
    {
        base.OnStart();
        Type = InteractableType.Resource;
        Name = GameObject.Name;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    

}
