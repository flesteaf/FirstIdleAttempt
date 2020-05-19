using Assets.Scripts.MoneyGenerators;
using UnityEngine;
using UnityEngine.UI;

public class MoneyText : MonoBehaviour
{
    public Text Money;
    private float moneyAmmount = 100;
    private MoneyProvider provider;

    private void Awake()
    {
        provider = new MoneyProvider();
    }

    // Start is called before the first frame update
    void Start()
    {
        Money.GetComponent<Text>().text = $"Money: {moneyAmmount}";
    }

    private void Update()
    {
        moneyAmmount += provider.GetAmmountGenerated();
        Money.GetComponent<Text>().text = $"Money: {moneyAmmount}";
    }
}
