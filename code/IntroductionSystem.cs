using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

public sealed class IntroductionSystem : Component
{
    protected override void OnUpdate()
    {

    }

    public async void StartIntroduction(Player player)
    {
		if (IsProxy)
			return;
		var camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
		var factory = Scene.GetAllComponents<Factory>().FirstOrDefault( x => !x.IsProxy );
		var cameraPositions = factory.GameObject.Children.Where( x => x.Name == "Introduction" ).ToList();
		var door = factory.GameObject.Children.Where( x => x.Name == "TitleFactory" ).FirstOrDefault();
		
		player.InCinematic = true;
		
		camera.Transform.Position = cameraPositions[0].Transform.Position;
        float duration = 20.0f;
		Sound.Play( "sounds/bob/intro/intro.sound", camera.Transform.Position );
        float startTime = Time.Now;
        while (Time.Now - startTime < duration)
        {
            float t = (Time.Now - startTime) / duration;
            camera.Transform.Position = Vector3.Lerp(camera.Transform.Position, cameraPositions[1].Transform.Position, t / 1000);
			camera.Transform.Rotation = Rotation.LookAt( door.Transform.Position - camera.Transform.Position, Vector3.Up );
            await Task.Delay(1);
        }
		player.InCinematic = false;
    }
}
