using UnityEngine;

[CreateAssetMenu(menuName = "Bar/Ingredient")]
public class IngredientData : ScriptableObject
{
    public string displayName = "Ingredient";
    public Color liquidColor = Color.white;
    public bool isAlcohol;
}