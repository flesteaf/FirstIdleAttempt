using Assets.Scripts;
using Assets.Scripts.Commands;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;


public class SceneManager : MonoBehaviour
{
    private readonly float oneSecond = 1;

    public ConsoleText Console { get; private set; }
    public MissionText Mission { get; private set; }
    public MoneyText Money { get; private set; }
    public IGameLogic Data { get; set; }

    public bool CommandUnderExecution => Data.CommandUnderExecution;

    private void Start()
    {
        Money.UpdateText(Data.PlayerData.MoneyAmmount);
    }

    private void Awake()
    {
        //This can be done only in the main scene);
        Console = FindObjectOfType<ConsoleText>();
        Mission = FindObjectOfType<MissionText>();
        Money = FindObjectOfType<MoneyText>();

        PlayerData playerData = SaveManager.LoadPlayerData();
        bool newPlayer = playerData.NewPlayer;
        Data = new GameLogic(playerData);

        if (!newPlayer)
        {
            CalculateOfflineEarnings(Data.PlayerData);
        }

        Data.MessageSender += SendToConsole;
        Data.ClearHandler += ClearConsole;

        Time.fixedDeltaTime = oneSecond;
    }

    private void CalculateOfflineEarnings(PlayerData playerData)
    {
        var currentDate = DateTime.UtcNow;
        var timePassed = currentDate.Subtract(playerData.Timestamp);
        Data.AddMultiProduction((float)timePassed.TotalSeconds);
        Money.UpdateText(Data.PlayerData.MoneyAmmount);
    }

    public IEnumerator ExecuteCommand(CommandLine command)
    {
        Console.AddCommand(command.ToString(), MessageType.Info);

        Command action = CommandFactory.GetCommand(command);
        if (!Data.PlayerData.AvailableSoftware.Contains(action.Name))
        {
            Console.AddMessage($"Command {action.Name} needs to be bought", MessageType.Error);
            yield break;
        }

        CommandOptions option = action.GetOptionFromCommand(command);
        if (option == CommandOptions.Invalid || !Data.PlayerData.AvailableSoftwareOptions.Contains(option))
        {
            Console.AddMessage($"Option {option} of command {action.Name} needs to be bought", MessageType.Error);
            yield break;
        }

        action.MessageNotification += SendToConsole;
        action.ActionProgress += PresentProgress;
        yield return action.Execute(Data, command);
        action.MessageNotification -= SendToConsole;
        action.ActionProgress -= PresentProgress;
        yield break;
    }

    private void SendToConsole(string message, MessageType type)
    {
        Console.AddMessage(message, type);
    }

    private void PresentProgress(int progress)
    {
        Console.PresentProgress(progress);
    }

    private void ClearConsole()
    {
        Console.ClearConsole();
    }

    private void FixedUpdate()
    {
        Data.AddProduction();
        Money.UpdateText(Data.PlayerData.MoneyAmmount);
    }

    ~SceneManager()
    {
        Data.MessageSender -= SendToConsole;
        Data.ClearHandler -= ClearConsole;
    }
}