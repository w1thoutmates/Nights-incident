using System.Collections.Generic;
using UnityEngine;

public class Container : Usable
{
    [Header("Параметры")]
    public float capacity = 300f;
    public bool isShaker;
    public float shakeTimeRequired = 3f;
    public float pourRate = 40f;

    [Header("Визуал жидкости")]
    public Transform liquidTransform;
    public Renderer liquidRenderer;

    [Header("Наклон и встряхивание")]
    public Vector3 pourTiltEuler = new Vector3(70f, 0f, 0f);
    public float tiltSpeed = 8f;
    public float shakeAmplitude = 0.03f;
    public float shakeFrequency = 35f;

    public bool IsShaken { get; private set; }

    public float Total
    {
        get { float t = 0f; foreach (var v in contents.Values) t += v; return t; }
    }

    readonly Dictionary<IngredientData, float> contents = new Dictionary<IngredientData, float>();
    float shakeTimer;
    float bottomY;
    float fullHeight;

    private Quaternion _targetRotation;
    private Vector3 _targetLocalPosition;
    private bool _hasTarget;

    void Awake()
    {
        if (liquidTransform != null)
        {
            fullHeight = liquidTransform.localScale.y;
            bottomY = liquidTransform.localPosition.y - fullHeight / 2f;
        }
        UpdateVisual();
    }

    void Start()
    {
        _targetRotation = Quaternion.Euler(heldLocalEuler);
        _targetLocalPosition = heldLocalPosition;
    }

    void Update()
    {
        if (!_hasTarget || transform.parent == null) return;

        transform.localRotation = tiltSpeed > 0f
            ? Quaternion.Slerp(transform.localRotation, _targetRotation, Time.deltaTime * tiltSpeed)
            : _targetRotation;

        transform.localPosition = Vector3.Lerp(transform.localPosition, _targetLocalPosition, Time.deltaTime * 20f);
    }

    public bool Add(IngredientData ing, float ml)
    {
        if (!AddInternal(ing, ml)) return false;
        IsShaken = false;
        shakeTimer = 0f;
        UpdateVisual();
        return true;
    }

    bool AddInternal(IngredientData ing, float ml)
    {
        float space = capacity - Total;
        if (space <= 0f || ml <= 0f) return false;
        ml = Mathf.Min(ml, space);
        contents.TryGetValue(ing, out float cur);
        contents[ing] = cur + ml;
        return true;
    }

    public void PourInto(Container target, float ml)
    {
        float total = Total;
        if (total <= 0.01f || target == this) return;
        ml = Mathf.Min(ml, total, target.capacity - target.Total);
        if (ml <= 0f) return;

        bool targetWasEmpty = target.Total <= 0.01f;
        float ratio = ml / total;
        foreach (var key in new List<IngredientData>(contents.Keys))
        {
            float part = contents[key] * ratio;
            contents[key] -= part;
            target.AddInternal(key, part);
        }
        if (targetWasEmpty) target.IsShaken = IsShaken;

        if (Total <= 0.5f) Clear();
        UpdateVisual();
        target.UpdateVisual();
    }

    public void Clear()
    {
        contents.Clear();
        IsShaken = false;
        shakeTimer = 0f;
        UpdateVisual();
    }

    public float Evaluate(RecipeData recipe)
    {
        float need = recipe.TotalMl();
        if (need <= 0f) return 0f;

        float diff = 0f;
        foreach (var e in recipe.entries)
        {
            contents.TryGetValue(e.ingredient, out float have);
            diff += Mathf.Max(0f, Mathf.Abs(have - e.ml) - recipe.toleranceMl);
        }

        foreach (var kv in contents)
            if (!recipe.Contains(kv.Key)) diff += kv.Value;

        float score = 1f - diff / need;
        if (recipe.requiresShake && !IsShaken) score *= 0.6f;
        return Mathf.Clamp01(score);
    }

    public override void OnPrimaryHeld(Container target, float dt)
    {
        if (isShaker && Total > 0.5f && !IsShaken)
        {
            shakeTimer += dt;
            _targetLocalPosition = heldLocalPosition + Vector3.up * Mathf.Sin(Time.time * shakeFrequency) * shakeAmplitude;
            _hasTarget = true;

            if (shakeTimer >= shakeTimeRequired) IsShaken = true;
            return;
        }

        _targetRotation = Quaternion.Euler(heldLocalEuler + pourTiltEuler);
        _targetLocalPosition = heldLocalPosition;
        _hasTarget = true;

        if (target != null) PourInto(target, pourRate * dt);
    }

    public override void OnPrimaryReleased()
    {
        _targetRotation = Quaternion.Euler(heldLocalEuler);
        _targetLocalPosition = heldLocalPosition;
        _hasTarget = true;
    }

    public override void OnSecondaryHeld(Container target, float dt)
    {
        if (target != null) PourInto(target, pourRate * dt);
    }

    void UpdateVisual()
    {
        if (liquidTransform == null) return;
        float total = Total;
        bool has = total > 0.5f;
        liquidTransform.gameObject.SetActive(has);
        if (!has) return;

        float h = Mathf.Clamp01(total / capacity) * fullHeight;
        var s = liquidTransform.localScale; s.y = h; liquidTransform.localScale = s;
        var p = liquidTransform.localPosition; p.y = bottomY + h / 2f; liquidTransform.localPosition = p;

        if (liquidRenderer != null)
        {
            Color c = Color.black;
            foreach (var kv in contents) c += kv.Key.liquidColor * (kv.Value / total);
            c.a = 1f;
            liquidRenderer.material.color = c;
        }
    }
}
