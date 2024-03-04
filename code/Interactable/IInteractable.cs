using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;


public class IInteractable : Component
{

    [Property] virtual public string Name { get; set; } = "Interactable";
    [Property] virtual public string Description { get; set; } = "An interactable object";
    [Property] public InteractableType Type { get; set; }
    [Property] public bool IsInteracted { get; set; } = false;

}


// Enum for the different types of interactable objects
public enum InteractableType
{
    None,
    Machine,
    Conveyor,
    Storage,
    Resource,
    Building,
    Vehicle,
    NPC,
    Player,
    UI,
    Other
}