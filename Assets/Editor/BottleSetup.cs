using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Автонастройка бутылок: коллайдер, Rigidbody, тег, слой и компонент Bottle
/// с подбором ингредиента по имени объекта.
/// Меню: Bar → Настроить выделенные бутылки / Bar → Настроить выделенный стакан.
/// </summary>
public static class BottleSetup
{
    // Порядок важен: FinskyIce должен попасть в Vodka, а не в Ice.
    private static readonly string[,] NameHints =
    {
        { "finsky",  "Vodka" },
        { "santo",   "Wine" },
        { "abrau",   "Champagne" },
        { "wiskey",  "Whiskey" },
        { "whisk",   "Whiskey" },
        { "100bars", "Energy100Bars" },
        { "orange",  "OrangeJuice" },
        { "juice",   "OrangeJuice" },
        { "cola",    "Cola" },
        { "lime",    "LimeJuice" },
        { "ice",     "Ice" }
    };

    [MenuItem("Bar/Настроить выделенные бутылки")]
    [MenuItem("Tools/Bar/Настроить выделенные бутылки")]
    private static void SetupBottles()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("[Bar] Выдели в иерархии бутылки и повтори");
            return;
        }

        Dictionary<string, IngredientData> ingredients = LoadIngredients();
        int layer = LayerMask.NameToLayer("Interact");
        int done = 0;

        foreach (GameObject go in selection)
        {
            if (go == null) continue;

            Undo.RecordObject(go, "Bar bottle setup");

            Collider collider = go.GetComponent<Collider>();
            if (collider == null)
            {
                BoxCollider box = Undo.AddComponent<BoxCollider>(go);
                FitBox(go, box);
            }

            Rigidbody body = go.GetComponent<Rigidbody>();
            if (body == null) body = Undo.AddComponent<Rigidbody>(go);
            body.mass = 0.4f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            if (layer >= 0) go.layer = layer;
            go.tag = "Pickable";

            Bottle bottle = go.GetComponent<Bottle>();
            if (bottle == null) bottle = Undo.AddComponent<Bottle>(go);

            if (bottle.ingredient == null)
            {
                IngredientData found = MatchIngredient(go.name, ingredients);
                if (found != null) bottle.ingredient = found;
            }

            EditorUtility.SetDirty(go);
            done++;

            string ingredientName = bottle.ingredient != null ? bottle.ingredient.displayName : "НЕ НАЙДЕН";
            Debug.Log(string.Format("[Bar] {0}: ингредиент = {1}", go.name, ingredientName));
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Bar] Настроено бутылок: " + done);
    }

    [MenuItem("Bar/Настроить выделенный стакан")]
    [MenuItem("Tools/Bar/Настроить выделенный стакан")]
    private static void SetupContainers()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("[Bar] Выдели стакан или шейкер и повтори");
            return;
        }

        int layer = LayerMask.NameToLayer("Interact");

        foreach (GameObject go in selection)
        {
            if (go == null) continue;

            Undo.RecordObject(go, "Bar container setup");

            if (go.GetComponent<Collider>() == null)
            {
                BoxCollider box = Undo.AddComponent<BoxCollider>(go);
                FitBox(go, box);
            }

            Rigidbody body = go.GetComponent<Rigidbody>();
            if (body == null) body = Undo.AddComponent<Rigidbody>(go);
            body.mass = 0.3f;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            if (layer >= 0) go.layer = layer;
            go.tag = "Pickable";

            Container container = go.GetComponent<Container>();
            if (container == null) container = Undo.AddComponent<Container>(go);

            if (container.liquidTransform == null)
            {
                Renderer liquid = FindLiquidRenderer(go);
                if (liquid != null)
                {
                    container.liquidTransform = liquid.transform;
                    container.liquidRenderer = liquid;
                    Debug.Log(string.Format("[Bar] {0}: жидкость = {1}", go.name, liquid.name));
                }
                else
                {
                    Debug.LogWarning(string.Format(
                        "[Bar] {0}: куб жидкости не найден, назначь Liquid Transform вручную", go.name));
                }
            }

            EditorUtility.SetDirty(go);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Bar] Готово");
    }

    private static Renderer FindLiquidRenderer(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            string n = r.gameObject.name.ToLowerInvariant();
            if (n.Contains("liquid") || n.Contains("fluid") || n.Contains("жидк")) return r;
        }

        return null;
    }

    private static void FitBox(GameObject go, BoxCollider box)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

        Transform t = go.transform;
        Vector3 scale = t.lossyScale;
        box.center = t.InverseTransformPoint(bounds.center);
        box.size = new Vector3(
            bounds.size.x / Mathf.Max(Mathf.Abs(scale.x), 0.0001f),
            bounds.size.y / Mathf.Max(Mathf.Abs(scale.y), 0.0001f),
            bounds.size.z / Mathf.Max(Mathf.Abs(scale.z), 0.0001f));
    }

    private static Dictionary<string, IngredientData> LoadIngredients()
    {
        Dictionary<string, IngredientData> result = new Dictionary<string, IngredientData>();

        string[] guids = AssetDatabase.FindAssets("t:IngredientData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            IngredientData data = AssetDatabase.LoadAssetAtPath<IngredientData>(path);
            if (data == null) continue;
            result[data.name] = data;
        }

        return result;
    }

    private static IngredientData MatchIngredient(string objectName, Dictionary<string, IngredientData> ingredients)
    {
        string lower = objectName.ToLowerInvariant();

        for (int i = 0; i < NameHints.GetLength(0); i++)
        {
            if (!lower.Contains(NameHints[i, 0])) continue;

            IngredientData data;
            if (ingredients.TryGetValue(NameHints[i, 1], out data)) return data;
        }

        foreach (KeyValuePair<string, IngredientData> pair in ingredients)
        {
            if (lower.Contains(pair.Key.ToLowerInvariant())) return pair.Value;
        }

        return null;
    }
}
