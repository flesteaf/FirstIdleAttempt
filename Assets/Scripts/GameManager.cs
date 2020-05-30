using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float moneyAmmount = 100;
    internal CommandLineField InputText;
    internal ConsoleText Console;
    internal MissionText Missions;
    internal MoneyText Money;
    internal Computer Computer;

    // Start is called before the first frame update
    private void Awake()
    {
        InputText = FindObjectOfType<CommandLineField>();
        Console = FindObjectOfType<ConsoleText>();
        Missions = FindObjectOfType<MissionText>();
        Money = FindObjectOfType<MoneyText>();
        Computer = new InitialComputer();
    }

    private void Start()
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
    private void Update()
    {
        moneyAmmount += 0.001f;
        Money.UpdateText(moneyAmmount);
    }
}