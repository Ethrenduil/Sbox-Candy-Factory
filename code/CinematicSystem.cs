using Sandbox;
using Sandbox.UI;
using System;
using System.Threading.Tasks;

public sealed class CinematicSystem : Component
{
	public bool ShouldStop = false;
	public bool Next = false;
	public int SoundDuration = 0;
    protected override void OnUpdate()
    {
		if (IsProxy || Input.EscapePressed )
			ShouldStop = true;
    }

	private async Task MoveCameraToWaypoint(CameraComponent camera, GameObject waypoint, GameObject objectToLook, float duration)
	{
		if (Next || ShouldStop)
			return;
		var startTime = Time.Now;
		while (Time.Now - startTime < duration)
		{
			if (Next || ShouldStop)
				break;
			float t = (Time.Now - startTime) / duration;
			camera.Transform.Position = Vector3.Lerp(camera.Transform.Position, waypoint.Transform.Position, t / 1000);
			camera.Transform.Rotation = Rotation.LookAt(objectToLook.Transform.Position - camera.Transform.Position, Vector3.Up);
			await Task.Delay(1);
		}
	}

	public async void StartCinematic(Player player, GameObject toLook, int duration = 0)
	{
		if (IsProxy)
			return;
		player.InCinematic = true;
		var camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
		var startingPosition = toLook.Root.Children.FirstOrDefault( x => x.Name == "Camera Start" );
		var waypoints = toLook.Root.Children.Where( x => x.Name == "Camera Waypoints" ).ToList();
		camera.Transform.Position = startingPosition.Transform.Position;
		ShouldStop = false;
		Next = false;
		for (int i = 0; i < waypoints.Count; i++)
		{
			if (ShouldStop)
				break;
			await MoveCameraToWaypoint(camera, waypoints[i], toLook, duration / waypoints.Count);
		}
	}
}
