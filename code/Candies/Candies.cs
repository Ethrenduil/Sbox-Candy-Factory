using Microsoft.VisualBasic;
using Sandbox;
using System.Numerics;

[Category( "Candy Factory - Candies" )]
public class Candies : Component
{
	[Property] public float CookingTime { get; set; } = 5.0f;
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }
	[Property] public int Price { get; set; }
	[Property] public Dictionary<DeliveryGoods, int> Ingredients { get; set; } = new();

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
	}

	public bool CanCook(Dictionary<DeliveryGoods, int> ingredients)
	{
		foreach (var Ingredient in Ingredients)
		{
			if (!ingredients.ContainsKey(Ingredient.Key) || ingredients[Ingredient.Key] < Ingredient.Value)
				return false;
		}
		return true;
	}

	public Dictionary<DeliveryGoods, int> GetIngredients()
	{
		return Ingredients;
	}

	public string GetIngredientsString()
	{
		string ingredients = "";
		foreach (var ingredient in Ingredients)
		{
			ingredients += $"{ingredient.Value}x {ingredient.Key.ToString()} ";
		}
		return ingredients;
	}

	
}
