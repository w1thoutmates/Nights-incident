using TMPro;
using UnityEngine;

public class MoneyHUD : MonoBehaviour
{
    public TMP_Text moneyText;
    public string format = "Money: {0}";

    private void OnEnable()
    {
        Economy.OnMoneyChanged += OnMoneyChanged;
        Refresh(Economy.Money);
    }

    private void OnDisable()
    {
        Economy.OnMoneyChanged -= OnMoneyChanged;
    }

    private void OnMoneyChanged(int money)
    {
        Refresh(money);
    }

    private void Refresh(int money)
    {
        if (moneyText == null) return;
        moneyText.text = string.Format(format, money);
    }
}
