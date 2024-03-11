using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;

[Category( "Candy Factory - interactable")]
public class AInteractable : Component
{

    [Property] virtual public string Name { get; set; } = "Interactable";
    [Property] virtual public string Description { get; set; } = "This is an interactable object";
    [Property] virtual public InteractableType Type { get; set; } 
    [Property] [Sync] public bool IsInteracted { get; set; } = false;

    virtual public GameObject Interactor { get; set; }
    virtual public void OnInteract(GameObject interactor) { }
    virtual public bool CanInteract(GameObject interactor) { return true; }

}


// Enum for the different types of interactable objects
public enum InteractableType
{
    None,
    Cooker,
	Upgrader,
	VendingBox,
    Storage,
    Resource,
    Building,
    Vehicle,
    NPC,
    Player,
    UI,
    Other
}
