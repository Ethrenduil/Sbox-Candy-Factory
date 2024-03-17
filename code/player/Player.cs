using Sandbox;
using Sandbox.Citizen;
using Sandbox.Utility;
using System;
using System.Drawing;
using System.Runtime;
using Eryziac.CandyFactory;
using System.Reflection;

[Category("Player")]
public class Player : Component
{
	[Property] public Vector3 Gravity { get; set; } = new Vector3(0, 0, 800);
	public Vector3 WishVelocity { get; private set; }
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Property] public GameObject ZoomPos { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	public CameraComponent Camera { get; set; }
	[Property] public TagSet CameraIgnoreTags { get; set; }
	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public bool IsRunning { get; set; }
	[Sync] public bool IsCrouching { get; set; }
	public Ray AimRay => new(Camera.Transform.Position + Camera.Transform.Rotation.Forward * 25f, Camera.Transform.Rotation.Forward);
	[Sync] private int Money { get; set; } = 0;
	public Connection Connection { get; set; }
	[Property] [Sync] public ulong SteamId { get; set; }
	[Sync] public string Name { get; set; }
	private bool IsLoading { get; set; }
	private bool IsSaving { get; set; }
	private bool Zoom { get; set; }
	public bool InCinematic { get; set; }
	public bool InMenu { get; set; } = false;
	public bool InDialogue { get; set; } = false;


	protected override void OnEnabled()
	{
		base.OnEnabled();

		if (IsProxy)
			return;

		var cam = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
		if (cam is not null)
		{
			var ee = cam.Transform.Rotation.Angles();
			ee.roll = 0;
			EyeAngles = ee;
			Camera = cam;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();

		if (IsProxy)
			return;

		SetupPlayer();
	}

	protected override void OnUpdate()
	{
		GetInput();
		UpdateEyeInput();
		UpdateCameraPosition();
		UpdateBodyRotation();
		UpdateCrouch();
		UpdateAnimation();	
	}

	private void GetInput()
	{
		if (IsProxy)
			return;

		if (InCinematic)
		{
			return;
		}

		if (Input.Pressed("FirstPerson"))
		{
			Zoom = !Zoom;
		}

		if (Input.Pressed("Slot1"))
		{
			Save();
		} else if (Input.Pressed("Slot2"))
		{
			Load();
		}
	}

	protected override void OnFixedUpdate()
	{
		if (IsProxy)
			return;

		BuildWishVelocity();

		var cc = GameObject.Components.Get<CharacterController>();

		if (!cc.IsOnGround)
		{
			if (AnimationHelper is not null)
			{
				AnimationHelper.Sitting = CitizenAnimationHelper.SittingStyle.None;
			}
		}

		if (cc.IsOnGround && Input.Down("Jump"))
		{
			PerformJump();
		}

		if (cc.IsOnGround)
		{
			ApplyGroundMovement(cc);
		}
		else
		{
			ApplyAirMovement(cc);
		}

		cc.Move();
		AdjustVelocityOnGround(cc);
	}

	[Broadcast]
	public void OnJump(float floatValue, string dataString, object[] objects, Vector3 position)
	{
		AnimationHelper?.TriggerJump();
	}

	private void UpdateEyeInput()
	{
		if (!IsProxy)
		{
			var ee = EyeAngles;
			ee += Input.AnalogLook * 0.5f;
			ee.roll = 0;

			ee.pitch = Math.Clamp(ee.pitch, -80, 80);

			EyeAngles = ee;
		}
	}

	private void UpdateCameraPosition()
	{
		if (!IsProxy)
		{
			
			if (InCinematic)
			{
				return;
			}
			var cam = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();

			var lookDir = EyeAngles.ToRotation();

			var camPos = Eye.Transform.Position;
			var camForward = lookDir.Forward;
			SceneTraceResult collisionResult = Scene.Trace
				.Ray(camPos, camPos - (camForward * 300f))
				.WithoutTags(CameraIgnoreTags)
				.Run();
			if (collisionResult.Hit)
				camPos = collisionResult.HitPosition + (camForward * 20f);
			else
				camPos = collisionResult.EndPosition;

			
			if (Zoom)
			{
				camPos = ZoomPos.Transform.Position;
			}
			
			cam.Transform.Position = camPos;
			cam.Transform.Rotation = lookDir;
		}
	}

	private void UpdateBodyRotation()
	{
		if (Body is not null)
		{
			var bodyRotation = Body.Transform.Rotation.Angles();
			bodyRotation.yaw = EyeAngles.yaw;
			Body.Transform.Rotation = bodyRotation.ToRotation();
		}
	}

	private void UpdateAnimation()
	{
		var cc = GameObject.Components.Get<CharacterController>() ?? null;
		var playerInteract = GameObject.Components.Get<PlayerInteract>() ?? null;
		var holdingItem = GameObject.Children.FirstOrDefault(x => x.Tags.Has("interactable"));

		if (cc is null)
			return;

		float rotateDifference = 0;

		if (AnimationHelper is not null)
		{
			AnimationHelper.WithVelocity(cc.Velocity);
			AnimationHelper.WithWishVelocity(WishVelocity);
			AnimationHelper.IsGrounded = cc.IsOnGround;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook(EyeAngles.Forward, 1, 1, 1.0f);
			AnimationHelper.DuckLevel = IsCrouching ? 1.0f : 0.0f;

			if (IsRunning)
			{
				AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
			}
			else
			{
				AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
			}

			// Set the hands to the correct position
			if (playerInteract != null && holdingItem != null)
			{
				AnimationHelper.IkLeftHand = holdingItem.Children.FirstOrDefault(x => x.Name == "LeftHandSlot");
            	AnimationHelper.IkRightHand = holdingItem.Children.FirstOrDefault(x => x.Name == "RightHandSlot");
			} else if (playerInteract != null && holdingItem == null)
			{
				AnimationHelper.IkLeftHand = null;
				AnimationHelper.IkRightHand = null;
			}
		}
	}

	private void BuildWishVelocity()
	{
		var rot = EyeAngles.ToRotation();

		WishVelocity = 0;

		if (!InCinematic && !InMenu && !InDialogue)
		{
			if (Input.Down("Forward")) WishVelocity += rot.Forward;
			if (Input.Down("Backward")) WishVelocity += rot.Backward;
			if (Input.Down("Left")) WishVelocity += rot.Left;
			if (Input.Down("Right")) WishVelocity += rot.Right;
		}
		IsRunning = Input.Down("Run");

		WishVelocity = WishVelocity.WithZ(0);

		if (!WishVelocity.IsNearZeroLength)
			WishVelocity = WishVelocity.Normal;

		if (IsRunning){
			WishVelocity *= 320.0f;
			IsRunning = true;
		}
		else {
			WishVelocity *= 110.0f;
			IsRunning = false;
		}

		if (WishVelocity.Length > 0.0f)
		{
			if (AnimationHelper is not null)
			{
				AnimationHelper.Sitting = CitizenAnimationHelper.SittingStyle.None;
			}
		}
	}
	float fJumps;
	private void PerformJump()
	{
		float flGroundFactor = 1.0f;
		float flMul = 268.3281572999747f * 1.2f;

		var cc = GameObject.Components.Get<CharacterController>();
		cc.Punch(Vector3.Up * flMul * flGroundFactor);

		OnJump(fJumps, "Hello", new object[] { Time.Now.ToString(), 43.0f }, Vector3.Random);

		fJumps += 1.0f;
	}

	private void UpdateCrouch()
    {
		if (IsProxy)
			return;

		var cc = GameObject.Components.Get<CharacterController>();
        if(cc is null) return;

        if(Input.Pressed("Duck") && !IsCrouching)
        {
            IsCrouching = true;
            cc.Height /= 2f;
        }

        if(Input.Released("Duck") && IsCrouching)
        {
            IsCrouching = false;
            cc.Height *= 2f;
        }
    }

	private void ApplyGroundMovement(CharacterController cc)
	{
		cc.Velocity = cc.Velocity.WithZ(0);
		cc.Accelerate(WishVelocity);
		cc.ApplyFriction(4.0f);
	}

	private void ApplyAirMovement(CharacterController cc)
	{
		cc.Velocity -= Gravity * Time.Delta * 0.5f;
		cc.Accelerate(WishVelocity.ClampLength(50));
		cc.ApplyFriction(0.1f);
	}

	private void AdjustVelocityOnGround(CharacterController cc)
	{
		if (!cc.IsOnGround)
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ(0);
		}
	}

	public void AddMoney(int amount)
	{
		Money += amount;
		Scene.GetAllComponents<CandyFactory>().FirstOrDefault().RefreshMoneyHUD();
	}

	public void RemoveMoney(int amount)
	{
		Money -= amount;
		Scene.GetAllComponents<CandyFactory>().FirstOrDefault().RefreshMoneyHUD();
	}

	public int GetMoney()
	{
		return Money;
	}

	public void SetupPlayer()
    {
        var candyFactory = Scene.GetAllComponents<CandyFactory>().FirstOrDefault();

        if (candyFactory is not null)
        {
            AddMoney(candyFactory.StartingMoney);
        } else
        {
            Log.Error("CandyFactory component not found");
        }

		Scene.NavMesh.IsEnabled = true;
    }

	public void Save()
	{
		if (IsSaving || IsLoading)
			return;
		IsSaving = true;
		SaveSystem.SavePlayer(this);
		IsSaving = false;
	}

	public void Load()
	{
		if (IsLoading || IsSaving)
			return;

		IsLoading = true;
		var data = SaveSystem.LoadPlayer();
		ProductionSystem ps = Scene.GetAllComponents<ProductionSystem>().Where(x => x.IsProxy == false).FirstOrDefault();
		Factory f = Scene.GetAllComponents<Factory>().Where(x => x.IsProxy == false).FirstOrDefault();
		QuestSystem qs = Scene.GetAllComponents<QuestSystem>().FirstOrDefault();

		if (data is not null)
		{
			Money = data.Money;
			Scene.GetAllComponents<CandyFactory>().FirstOrDefault().RefreshMoneyHUD();
			ps.SetProductionUpgrade(data.UpgradeData);
			f.SetStock(data.StockData.Stock);
			qs.SetQuestData(this, data.QuestData);
		}
		IsLoading = false;
	}

	public QuestData GetQuestData()
	{
		QuestSystem qs = Scene.GetAllComponents<QuestSystem>().FirstOrDefault();

		// If there is no quest, return -1
		if (qs.CurrentQuest is null)
		{
			return new QuestData
			{
				CurrentQuestIndex = -1
			};
		}

		// If there is a quest, return the current quest index
		return new QuestData
		{
			CurrentQuestIndex = qs.CurrentQuestIndex
		};
	}

	public UpgradeData GetUpgradeData()
	{
		ProductionSystem us = Scene.GetAllComponents<ProductionSystem>().Where(x => x.IsProxy == false).FirstOrDefault();
		var prodSys = new List<int>();
		foreach (var prodLine in us.ProductionLines)
		{
			prodSys.Add(prodLine.GetActualLevel());
		}
		return new UpgradeData
		{
			UpgradeLevels = us.Upgrades,
			UpgradeProductionLines = prodSys,
		};
	}

	public StockData GetStockData()
	{
		Factory f = Scene.GetAllComponents<Factory>().Where(x => x.IsProxy == false).FirstOrDefault();
		return new StockData
		{
			Stock = f.Stock
		};
	}
}
