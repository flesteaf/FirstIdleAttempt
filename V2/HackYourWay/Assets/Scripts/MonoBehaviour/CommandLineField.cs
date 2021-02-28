using Assets.Scripts.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandLineField : MonoBehaviour
{
    [SerializeField] private TMP_InputField InputField;
    [SerializeField] private TextMeshProUGUI Placeholder;
    [SerializeField] private Color ActiveColor;
    [SerializeField] private Color DisableColor;
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
        ActivateInputField();
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

        SetInputFiledStatus();
    }

    private void SetInputFiledStatus()
    {
        if (theGame.CommandUnderExecution && InputField.enabled)
        {
            DeactivateInputField();
        }

        if (!theGame.CommandUnderExecution && !InputField.enabled)
        {
            ActivateInputField();
        }
    }

    private void DeactivateInputField()
    {
        ColorBlock colors = InputField.colors;
        colors.normalColor = DisableColor;
        InputField.colors = colors;
        InputField.enabled = false;
        InputField.DeactivateInputField(true);
    }

    private void ActivateInputField()
    {
        ColorBlock colors = InputField.colors;
        colors.normalColor = ActiveColor;
        InputField.colors = colors;
        InputField.enabled = true;
        InputField.ActivateInputField();
    }

    private void AddCommand()
    {
        line = InputField.text;
        if (string.IsNullOrEmpty(line))
        {
            return;
        }

        Placeholder.text = "Enter command...";
        InputField.text = string.Empty;
        CommandLine command = new CommandLine(line);
        StartCoroutine(theGame.ExecuteCommand(command));
    }
}