using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
using Assets.Scripts.MoneyGenerators;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float moneyAmmount = 100;
    private MoneyProvider provider;
    internal CommandLineField InputText;
    internal ConsoleText Console;
    internal MissionText Missions;
    internal MoneyText Money;
    internal Computer Computer;

    // Start is called before the first frame update
    void Awake()
    {
        InputText = FindObjectOfType<CommandLineField>();
        Console = FindObjectOfType<ConsoleText>();
        Missions = FindObjectOfType<MissionText>();
        Money = FindObjectOfType<MoneyText>();
        Computer = new InitialComputer();
        provider = new MoneyProvider();
    }

    void Start()
    {
        Money.UpdateText(moneyAmmount);
    }

    internal void ExecuteCommand(string command)
    {
        Console.AddMessage(command, MessageType.Info);
        Command action = CommandFactory.GetCommand(command);
        action.Execute(this, command);
    }

    // Update is called once per frame
    void Update()
    {
        moneyAmmount += provider.GetAmmountGenerated();
        Money.UpdateText(moneyAmmount);
    }
}
