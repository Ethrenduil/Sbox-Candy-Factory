using Sandbox;

[Category( "Candy Factory - Quests")]
public sealed class QuestSystem : Component
{
    [Property] public List<GameObject> Candies { get; set; } = new();
	public List<Quest> Quests { get; private set; } = new();
    public Quest CurrentQuest { get; private set; }
    public int CurrentQuestIndex { get; private set; } = 0;
	private CurrentTaskHUD currentTaskHUD;
	public GameObject Arrow { get; set; }
	private SoundHandle sound;
	private Settings Settings { get; set; }

    protected async override void OnStart()
    {
        base.OnStart();
        await GameTask.DelaySeconds(1);
        Quest quest1 = new Quest("Introduction", "Welcome to Candy Factory!");
        var bob = Scene.GetAllComponents<Bob>().FirstOrDefault().GameObject;
        quest1.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Talk to Bob", bob));
        Quests.Add(quest1);

        Quest quest2 = new Quest("Tutorial", "Buy the Candy Factory");
        var vendingPanel = Scene.GetAllComponents<Buyable>().FirstOrDefault(x => !x.IsProxy).GameObject;
        quest2.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Interact with the vending panel", vendingPanel));
        Quests.Add(quest2);


        Quest quest3 = new Quest("Tutorial", "Meet Bob at the Candy Factory");
        quest3.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Talk to Bob", bob));
        Quests.Add(quest3);

        Quest quest4 = new Quest("Tutorial", "Ask Bob for a delivery");
        quest4.Objectives.Add(new QuestObjective(ObjectiveType.ResourceOrdering, "Make an delivery order"));
        Quests.Add(quest4);

        Quest quest5 = new Quest("Tutorial", "Grab the delivery outside the factory");
        quest5.Objectives.Add(new QuestObjective(ObjectiveType.WaitDelivery, "Take the ingredients"));
        Quests.Add(quest5);

        Quest quest6 = new Quest("Tutorial", "Deliver the order to the Candy Factory");
        var stockarea = Scene.GetAllComponents<Stockable>().FirstOrDefault(x => !x.IsProxy).GameObject;
        quest6.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Deliver the order to the Candy Factory", stockarea));
        Quests.Add(quest6);

        Quest quest7 = new Quest("Tutorial", "Put the ingredients in the oven");
		var oven = Scene.GetAllComponents<Cooker>().FirstOrDefault(x => x.GameObject.Enabled && !x.IsProxy).GameObject;
        quest7.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Put the ingredients in the oven", oven));
        Quests.Add(quest7);

		Quest quest8 = new Quest("Tutorial", "Make dark chocolate");
        quest8.Objectives.Add(new QuestObjective(ObjectiveType.Creation, "Wait that the chocolate is ready", 1, Candies[0]));
        Quests.Add(quest8);

        Quest quest9 = new Quest("Tutorial", "Sell the dark chocolate");
        var vendingBox = Scene.GetAllComponents<Seller>().FirstOrDefault( X => !X.IsProxy  && X.GameObject.Parent.Name == "Line1").GameObject;
        quest9.Objectives.Add(new QuestObjective(ObjectiveType.Interaction, "Interact with the vending box", vendingBox));
        Quests.Add(quest9);


		CurrentQuest = Quests[CurrentQuestIndex];
		currentTaskHUD = Scene.GetAllComponents<CurrentTaskHUD>().FirstOrDefault();
		
		var arrowComponent = Scene.GetAllComponents<FollowTask>().FirstOrDefault();	
		Arrow = Scene.GetAllComponents<FollowTask>().FirstOrDefault().GameObject;
		SetArrow();
    }

	protected override void OnUpdate()
	{
		
	}

	public void SetArrow()
	{
		if (CurrentQuest.Objectives.Any( X => X.ShowArrow))
		{
			Arrow.Enabled = true;
			var target = CurrentQuest.Objectives.FirstOrDefault( X => X.ShowArrow).ObjectTarget;
			var arrowComponent = Scene.GetAllComponents<FollowTask>().FirstOrDefault();	
			arrowComponent.startPosition = target.Transform.Position + new Vector3(0, 0, 100);
			arrowComponent.endPosition = target.Transform.Position + new Vector3(0, 0, 130);
			Arrow.SetParent(target);
		}
		else
		{
			Arrow.Enabled = false;
		}
	}

	public async void CompleteObjective(QuestObjective objective)
	{
		Settings ??= Scene.GetAllComponents<Settings>().FirstOrDefault(x => !x.IsProxy);
		objective.IsCompleted = true;
		if (CurrentQuest.Objectives.All( X => X.IsCompleted))
		{
			CurrentQuest.IsCompleted = true;
            while(Scene.GetAllComponents<Player>().FirstOrDefault().InDialogue)
            {
                await GameTask.Delay(100);
            }
			CurrentQuestIndex++;
			sound?.Stop();
			SoundEvent soundEvent = new( "/sounds/bob/tuto_" + CurrentQuestIndex + ".sound" )
			{
				UI = true,
				Volume = Settings.GetVolume( VolumeType.Sound )
			};
			sound = Sound.Play(soundEvent);
			if (CurrentQuestIndex < Quests.Count)
			{
				CurrentQuest = Quests[CurrentQuestIndex];
				SetArrow();
			}
			else 
			{
				CurrentQuest = null;
				Arrow.Enabled = false;
			}
		}
		currentTaskHUD.StateHasChanged();
	}

	public void EarnedMoney(QuestObjective objective, int money)
	{
		objective.CurrentAmount += money;
		if (objective.CurrentAmount >= objective.TargetAmount)
		{
			CompleteObjective(objective);
		}
	}

	public void Cooked(QuestObjective objective, GameObject cooked, int amount = 1)
	{
		if (cooked.Name.Contains(objective.ObjectTarget.Name))
		{
			CompleteObjective(objective);
		}
	}
}
