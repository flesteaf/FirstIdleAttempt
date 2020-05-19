using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class CommandLineField : MonoBehaviour
{
    public InputField InputField;
    private Game theGame;
    private ConsoleText console;
    private string command;

    private void Awake()
    {
        theGame = FindObjectOfType<Game>();
        console = FindObjectOfType<ConsoleText>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InputField.placeholder.GetComponent<Text>().text = "Enter command...";
        InputField.ActivateInputField();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return) && InputField.isActiveAndEnabled)
        {
            AddCommand();
        }
    }

    public void AddCommand()
    {
        command = InputField.text;
        if (string.IsNullOrEmpty(command))
        {
            return;
        }

        InputField.placeholder.GetComponent<Text>().text = "Enter command...";
        InputField.text = string.Empty;
        InputField.ActivateInputField();
        console.AddMessage(command);
        theGame.ExecuteCommand(command);
    }
}
