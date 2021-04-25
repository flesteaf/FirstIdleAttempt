using Assets.Scripts;
using Assets.Scripts.Commands;
using Newtonsoft.Json;
using System.Collections;
using UnityEditor;
using UnityEngine;


public class SceneManager : MonoBehaviour
{
    private const string PlayerDataKey = "PlayerData";
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
        Data = new GameLogic(GetPlayerData());

        CalculateOfflineEarnings(Data.PlayerData);

        Data.MessageSender += SendToConsole;
        Data.ClearHandler += ClearConsole;

        Time.fixedDeltaTime = oneSecond;
    }

    private void CalculateOfflineEarnings(PlayerData playerData)
    {
        //TODO: this needed during load
    }

    private PlayerData GetPlayerData()
    {
        var playerData = DataSerializer.LoadString(PlayerDataKey);

        if (string.IsNullOrEmpty(playerData))
        {
            return new PlayerData();
        }
        else
        {
            return JsonConvert.DeserializeObject<PlayerData>(playerData);
        }
    }

    public IEnumerator ExecuteCommand(CommandLine command)
    {
        Console.AddCommand(command.ToString(), MessageType.Info);

        if (command.CommandName == CommandNames.save)
        {
            var playerData = JsonConvert.SerializeObject(Data.PlayerData);
            DataSerializer.SaveString(PlayerDataKey, playerData);
            yield break;
        }

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