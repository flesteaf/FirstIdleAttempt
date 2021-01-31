using TMPro;
using UnityEngine;

public class MoneyText : MonoBehaviour
{
    public TextMeshProUGUI Money;

    public void UpdateText(float moneyAmmount)
    {
        Money.text = $"Money: {moneyAmmount}";
    }
}