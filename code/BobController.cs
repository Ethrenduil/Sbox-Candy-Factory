using Sandbox;

[Title( "Bob" )]
[Category( "Candy Factory" )]
public sealed class BobController : Component
{
	private SkinnedModelRenderer Renderer { get; set; }
	protected override void OnUpdate()
	{
		base.OnUpdate();

		
	}

	protected override void OnAwake()
	{
		Renderer = Components.Get<SkinnedModelRenderer>( true );
		
		base.OnAwake();
	}
}
