using System.Diagnostics;
using Sandbox;
using Sandbox.Engine.Utility.RayTrace;


public class AInteractable : Component
{

    virtual public string Name { get; set; } = "Interactable";
    virtual public string Description { get; set; } = "This is an interactable object";
    virtual public InteractableType Type { get; set; } 
    virtual public string PrefabPath { get; set; } 
    virtual public bool IsInteracted { get; set; } = false;

    virtual protected GameObject Interactor { get; set; }
    protected float InteractionTime = 0.0f;
    protected const float InteractionCooldown = 0.5f;
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