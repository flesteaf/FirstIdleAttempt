using TMPro;
using UnityEngine;

public class MoneyText : MonoBehaviour, IMoneyText
{
    public TextMeshProUGUI Money;

    public void UpdateText(float moneyAmmount)
    {
        Money.text = $"Money: {moneyAmmount}";
    }
}