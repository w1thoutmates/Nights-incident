using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class RecipeEntry
{
    public IngredientData ingredient;
    public float ml = 30f;
}

[CreateAssetMenu(menuName = "Bar/Recipe")]
public class RecipeData : ScriptableObject
{
    public string drinkName = "Cocktail";
    public List<RecipeEntry> entries = new List<RecipeEntry>();
    public bool requiresShake = true;
    public int price = 10;

    [Tooltip("Допустимая погрешность по каждому ингредиенту, мл")]
    public float toleranceMl = 5f;

    public bool Contains(IngredientData ing)
    {
        foreach (var e in entries)
            if (e.ingredient == ing) return true;
        return false;
    }

    public float TotalMl()
    {
        float t = 0f;
        foreach (var e in entries) t += e.ml;
        return t;
    }

    public string GetRecipeText()
    {
        StringBuilder sb = new StringBuilder(drinkName);

        for (int i = 0; i < entries.Count; i++)
        {
            RecipeEntry e = entries[i];
            if (e == null || e.ingredient == null) continue;

            sb.Append('\n')
              .Append(e.ingredient.displayName)
              .Append(' ')
              .Append(Mathf.RoundToInt(e.ml))
              .Append(" ml");
        }

        if (requiresShake) sb.Append("\nshake");
        sb.Append('\n').Append(price).Append(" $");

        return sb.ToString();
    }
}
