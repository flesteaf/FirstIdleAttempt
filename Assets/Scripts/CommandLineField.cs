using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommandLineField : MonoBehaviour
{
    public TMP_InputField InputField;
    public TextMeshProUGUI Placeholder;
    private GameManager theGame;
    private string command;

    private void Awake()
    {
        theGame = FindObjectOfType<GameManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Placeholder.text = "Enter command...";
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

        Placeholder.text = "Enter command...";
        InputField.text = string.Empty;
        InputField.ActivateInputField();
        theGame.ExecuteCommand(command);
    }
}
