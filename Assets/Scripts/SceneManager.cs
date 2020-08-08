﻿using Assets.Scripts;
using Assets.Scripts.Commands;
using Assets.Scripts.Store;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


public class SceneManager : MonoBehaviour
{
    private readonly float oneSecond = 1;

    public ConsoleText Console { get; private set; }
    public MissionText Mission { get; private set; }
    public MoneyText Money { get; private set; }
    public GameData Data { get; private set; }

    private void Start()
    {
        Money.UpdateText(Data.MoneyAmmount);
    }

    private void Awake()
    {
        //This can be done only in the main scene);
        Console = FindObjectOfType<ConsoleText>();
        Mission = FindObjectOfType<MissionText>();
        Money = FindObjectOfType<MoneyText>();
        
        TextAsset dataAsset = (TextAsset)Resources.Load("dataStore");
        GameStore store = JsonConvert.DeserializeObject<GameStore>(dataAsset.text);
        Data = new GameData(store);
        Data.MessageSender += SendToConsole;

        Time.fixedDeltaTime = oneSecond;
    }

    public void ExecuteCommand(CommandLine command)
    {
        Console.AddMessage(command.ToString(), MessageType.Info);

        Command action = CommandFactory.GetCommand(command);
        if (!Data.AvailableSoftware.Contains(action.Name))
        {
            Console.AddMessage($"Command {action.Name} needs to be bought", MessageType.Error);
            return;
        }

        CommandOptions option = action.GetOptionFromCommand(command);
        if (option == CommandOptions.Invalid || !Data.AvailableSoftwareOptions.Contains(option))
        {
            Console.AddMessage($"Option {option} of command {action.Name} needs to be bought", MessageType.Error);
            return;
        }

        action.MessageNotification += SendToConsole;
        action.Execute(Data, command);
        action.MessageNotification -= SendToConsole;
    }

    private void SendToConsole(string message, MessageType type)
    {
        Console.AddMessage(message, type);
    }

    private void FixedUpdate()
    {
        Data.AddProduction();
        Money.UpdateText(Data.MoneyAmmount);
    }

    ~SceneManager()
    {
        Data.MessageSender -= SendToConsole;
    }
}