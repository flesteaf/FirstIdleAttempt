using Assets.Scripts.Commands;
using TMPro;
using UnityEngine;

public class CommandLineField : MonoBehaviour
{
    public TMP_InputField InputField;
    public TextMeshProUGUI Placeholder;
    private SceneManager theGame;
    private string line;

    private void Awake()
    {
        theGame = FindObjectOfType<SceneManager>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        Placeholder.text = "Enter command...";
        InputField.ActivateInputField();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return) && InputField.isActiveAndEnabled)
        {
            AddCommand();
        }

        if (Input.GetKeyUp(KeyCode.UpArrow) && InputField.isActiveAndEnabled)
        {
            InputField.text = line;
            InputField.caretPosition = line.Length;
        }
    }

    public void AddCommand()
    {
        line = InputField.text;
        if (string.IsNullOrEmpty(line))
        {
            return;
        }

        Placeholder.text = "Enter command...";
        InputField.text = string.Empty;
        InputField.ActivateInputField();
        CommandLine command = new CommandLine(line);
        theGame.ExecuteCommand(command);
    }
}