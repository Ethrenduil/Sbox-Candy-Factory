using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;


public class AInteractable : Component
{

    virtual public string Name { get; set; } = "Interactable";
    virtual public string Description { get; set; } = "This is an interactable object";
    virtual public InteractableType Type { get; set; } 
    virtual public string PrefabPath { get; set; } 
    [Sync] public bool IsInteracted { get; set; } = false;

    virtual protected GameObject Interactor { get; set; }
    virtual public void OnInteract(GameObject interactor) { }


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