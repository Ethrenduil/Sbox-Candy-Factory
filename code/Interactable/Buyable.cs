using System;
using Microsoft.VisualBasic;
using Eryziac.CandyFactory;

[Category( "Candy Factory - interactable")]
public class Buyable : AInteractable
{
	public DialogueMenu dialogue { get; set; }

    protected override void OnStart()
    {
        base.OnStart();
        Description = $"Press E to buy your factory";
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
    }

    public override void OnInteract(GameObject interactor)
    {
        if (IsInteracted)
			return;

		IsInteracted = true;
		
		var factory = GameObject.Scene.GetAllComponents<Factory>().FirstOrDefault(x => !x.IsProxy);
		factory.IsStarted = true;
		factory.Components.Get<TitleFactory>(FindMode.InChildren).SetTitle(factory.Name + "'s Factory");
		var bob_dest = factory.GameObject.Children.FirstOrDefault(x => x.Name == "BobPlace");
		var bob = Scene.GetAllComponents<Bob>().FirstOrDefault();
		bob.Transform.Position = bob_dest.Transform.Position;
		bob.Transform.Rotation = bob_dest.Transform.Rotation;
		GameObject.Destroy();
	}

    public override bool CanInteract(GameObject interactor)
    {
        return true;
    }

}
