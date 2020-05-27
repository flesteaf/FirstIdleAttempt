using Assets.Scripts.MoneyGenerators;
using UnityEngine;
using TMPro;

public class MoneyText : MonoBehaviour
{
    public TextMeshProUGUI Money;

    public void UpdateText(float moneyAmmount)
    {
        
        Money.text = $"Money: {moneyAmmount}";
    }
}
