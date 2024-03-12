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
    }

    public override void OnInteract(GameObject interactor)
    {
		Log.Info( "Interacting with Bob" );
        if (IsInteracted)
			return;

		IsInteracted = true;
		
		var player = interactor.Components.Get<Player>();
		var factory = Scene.GetAllComponents<Factory>().FirstOrDefault( x => !x.IsProxy );
		var dialogue = Scene.GetAllComponents<DialogueMenu>().FirstOrDefault();
		if (factory.IsStarted)
			StartDialogue(player);
		else
			IntroductionInteract(factory, dialogue);

		IsInteracted = false;
	}

	private void IntroductionInteract(Factory factory, DialogueMenu dialogue)
	{
		var dialogues = new List<string>
			{
				"Cock-a-doodle-doo! Welcome to Candyland, where the wackiest dreams get whipped up into sugary goodness! I'm Bob, the most hilarious rooster this side of the coop. You wanna know how we make the craziest candies the world has ever seen? Buckle up, buttercup, because things are about to get batty!",
				"See that factory over there? That, my friend, is where the magic (almost) happens. We take regular ol' stuff and turn it into candy so awesome, it'll blow your socks off! But watch out, it's gonna be a wild ride. You gotta manage the machines, keep an eye on production, and most importantly, make sure every candy is so good, it'll make your taste buds do a happy dance.",
				"Up for the challenge, sugar chum? Then slap on that hat and gloves, 'cause we're about to get this candy party started! First things first, a tour of our humble abode. You'll see the workshop, learn how the machines work their magic, and meet your one and only co-worker: the magnificent me!",
				"Don't you worry, I'll be here to hold your wing every step of the way. Together, we'll turn this factory into the greatest candy haven the world has ever known!"
			};
			
		dialogue.StartDialogue(factory, Name, dialogues);
	}

	private void StartDialogue(Player player)
	{
		
	}

    public override bool CanInteract(GameObject interactor)
    {
        return true;
    }

}
