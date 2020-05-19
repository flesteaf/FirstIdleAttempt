using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
using UnityEngine;
using static Assets.Scripts.Computers.Computer;

namespace Assets.Scripts
{
    public class Game : MonoBehaviour
    {
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
        }

        // Update is called once per frame
        void Update()
        {

        }

        internal void ExecuteCommand(string command)
        {
            Command action = CommandFactory.GetCommand(command);
            action.Execute(this, command);
        }
    }
}