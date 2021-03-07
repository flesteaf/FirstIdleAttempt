using Assets.Scripts;
using Assets.Scripts.Commands;
using Assets.Scripts.Store;
using Newtonsoft.Json;
using System.Collections;
using UnityEditor;
using UnityEngine;


public class SceneManager : MonoBehaviour
{
    private const string GameDataKey = "GameData";
    private readonly float oneSecond = 1;

    public ConsoleText Console { get; private set; }
    public MissionText Mission { get; private set; }
    public MoneyText Money { get; private set; }
    public GameData Data { get; set; }

    public bool CommandUnderExecution => Data.CommandUnderExecution;

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

        var data = DataSerializer.LoadString(GameDataKey);

        if (string.IsNullOrEmpty(data))
        {
            TextAsset dataAsset = (TextAsset)Resources.Load("dataStore");
            GameStore store = JsonConvert.DeserializeObject<GameStore>(dataAsset.text);
            Data = new GameData
            {
                Store = store
            };
        }
        else
        {
            Data = JsonConvert.DeserializeObject<GameData>(data);
        }

        Data.MessageSender += SendToConsole;
        Data.ClearHandler += ClearConsole;

        Time.fixedDeltaTime = oneSecond;
    }

    public IEnumerator ExecuteCommand(CommandLine command)
    {
        Console.AddCommand(command.ToString(), MessageType.Info);

        if (command.CommandName == CommandNames.save)
        {
            var data = JsonConvert.SerializeObject(Data);
            DataSerializer.SaveString(GameDataKey, data);
            yield break;
        }

        Command action = CommandFactory.GetCommand(command);
        if (!Data.AvailableSoftware.Contains(action.Name))
        {
            Console.AddMessage($"Command {action.Name} needs to be bought", MessageType.Error);
            yield break;
        }

        CommandOptions option = action.GetOptionFromCommand(command);
        if (option == CommandOptions.Invalid || !Data.AvailableSoftwareOptions.Contains(option))
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
        Money.UpdateText(Data.MoneyAmmount);
    }

    ~SceneManager()
    {
        Data.MessageSender -= SendToConsole;
        Data.ClearHandler -= ClearConsole;
    }
}