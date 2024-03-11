using Microsoft.VisualBasic;
using Sandbox;
using System.Numerics;

[Category( "Candy Factory - Candies" )]
public class Candies : Component
{
	[Property] public float CookingTime { get; set; } = 5.0f;
	[Property] public float SellingTime { get; set; } = 5.0f;
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }
	[Property] public int Price { get; set; }

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
	}

	
}
