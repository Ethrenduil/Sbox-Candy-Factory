using System;

[Category( "Candy Factory - interactable")]
public class Bob : AInteractable
{

    protected override void OnStart()
    {
        base.OnStart();
        // Set the default description for interaction
        Description = $"Press E to talk to {Name}";
        // Ensure proper network ownership transfer
        GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
        Type = InteractableType.Resource;
    }

    public override void OnInteract(GameObject interactor)
    {
		Log.Info( "Interacting with Bob" );
        if (IsInteracted)
			return;

		IsInteracted = true;

		var player = interactor.Components.Get<Player>();
		var factory = Scene.GetAllComponents<Factory>().FirstOrDefault( x => !x.IsProxy );
		var startingPosition = factory.GameObject.Children.FirstOrDefault( x => x.Name == "Camera Start" );
		var waypoints = factory.GameObject.Children.Where( x => x.Name == "Camera Waypoints" ).ToList();
		var lookDir = factory.GameObject.Children.FirstOrDefault( x => x.Name == "TitleFactory" );
		Scene.GetAllComponents<IntroductionSystem>().FirstOrDefault().StartIntroduction(player, startingPosition, waypoints, lookDir, "sounds/bob/intro/intro.sound", 20f);
		IsInteracted = false;
	}

    public override bool CanInteract(GameObject interactor)
    {
        return true;
    }

}
