using System;
using System.Collections;
using TMPro;
using UnityEngine;

public static class Economy
{
    public static event Action<int> OnMoneyChanged;

    private static int _money;

    public static int Money
    {
        get { return _money; }
        set
        {
            if (_money == value) return;
            _money = value;
            if (OnMoneyChanged != null) OnMoneyChanged(_money);
        }
    }
}

[RequireComponent(typeof(Collider))]
public class Customer : MonoBehaviour
{
    [Header("Заказ")]
    public RecipeData order;

    [Header("Подсказка над головой")]
    public bool showRecipeHint = true;
    public TMP_Text orderLabel;

    [Header("Терпение, секунд")]
    public float patience = 90f;

    [Header("Оплата")]
    [Range(0f, 1f)] public float minScoreToPay = 0.3f;
    [Range(0f, 1f)] public float tipScore = 0.9f;
    [Range(0f, 1f)] public float tipPercent = 0.2f;

    public BarSeat Seat { get; set; }
    public bool IsServed { get; private set; }

    private float _elapsed;
    private RecipeData _shownOrder;

    private void Update()
    {
        if (_shownOrder != order) RefreshLabel();

        if (IsServed || order == null) return;

        _elapsed += Time.deltaTime;
        if (_elapsed >= patience) Leave();
    }

    public bool TryServe(Container glass)
    {
        if (IsServed || order == null || glass == null) return false;
        if (glass.Total < 1f) return false;

        float score = glass.Evaluate(order);
        bool paid = score >= minScoreToPay;
        int pay = paid ? Mathf.RoundToInt(order.price * score) : 0;
        int tip = (paid && score >= tipScore) ? Mathf.RoundToInt(order.price * tipPercent) : 0;

        if (paid) Economy.Money += pay + tip;

        IsServed = true;
        order = null;
        RefreshLabel();
        StartCoroutine(RemoveAfter(1f));
        return true;
    }

    private void Leave()
    {
        if (IsServed) return;

        IsServed = true;
        order = null;
        RefreshLabel();
        StartCoroutine(RemoveAfter(0.5f));
    }

    private IEnumerator RemoveAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Seat != null)
        {
            if (Seat.Occupant == this) Seat.Occupant = null;
            Seat = null;
        }

        Destroy(gameObject);
    }

    private void RefreshLabel()
    {
        _shownOrder = order;
        if (orderLabel == null) return;

        orderLabel.text = (showRecipeHint && order != null) ? order.GetRecipeText() : string.Empty;
    }
}
