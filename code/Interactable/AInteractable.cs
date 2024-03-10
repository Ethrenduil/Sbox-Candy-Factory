using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;


public class AInteractable : Component
{

    [Property] virtual public string Name { get; set; } = "Interactable";
    [Property] virtual public string Description { get; set; } = "This is an interactable object";
    [Property] virtual public InteractableType Type { get; set; } 
    [Property] [Sync] public bool IsInteracted { get; set; } = false;

    virtual protected GameObject Interactor { get; set; }
    virtual public void OnInteract(GameObject interactor) { }

}


// Enum for the different types of interactable objects
public enum InteractableType
{
    None,
    Cooker,
	Upgrader,
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
