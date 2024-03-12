using Sandbox;
using Sandbox.UI;
using System;
using System.Threading.Tasks;

public sealed class IntroductionSystem : Component
{
    private SoundHandle currentSound = null;
    protected override void OnUpdate()
    {
        if (currentSound is not null) currentSound.Position = Scene.Camera.Transform.Position;
    }

    public async void StartIntroduction(Player player, GameObject startingPosition, List<GameObject> waypoints, GameObject objectToLook, String soundPath, float duration)
	{
		if (IsProxy)
			return;
		var camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
		var factory = Scene.GetAllComponents<Factory>().FirstOrDefault( x => !x.IsProxy );
		var cameraPositions = factory.GameObject.Children.Where( x => x.Name == "Introduction" ).ToList();
		var door = factory.GameObject.Children.Where( x => x.Name == "TitleFactory" ).FirstOrDefault();
		
		player.InCinematic = true;
		
		camera.Transform.Position = startingPosition.Transform.Position;
		Sound.Play( soundPath );
		for (int i = 0; i < waypoints.Count; i++)
		{
			await MoveCameraToWaypoint(camera, waypoints[i], objectToLook, duration);
		}
		player.InCinematic = false;
	}

	private async Task MoveCameraToWaypoint(CameraComponent camera, GameObject waypoint, GameObject objectToLook, float duration)
	{
		var startTime = Time.Now;
		while (Time.Now - startTime < duration)
		{
			float t = (Time.Now - startTime) / duration;
			camera.Transform.Position = Vector3.Lerp(camera.Transform.Position, waypoint.Transform.Position, t / 1000);
			camera.Transform.Rotation = Rotation.LookAt(objectToLook.Transform.Position - camera.Transform.Position, Vector3.Up);
			await Task.Delay(1);
		}
	}
}
