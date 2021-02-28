using Assets.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Console;
    [SerializeField] private Scrollbar scrollbar;

    private readonly List<string> consoleMessages = new List<string>();
    private readonly List<string> commandHistory = new List<string>();
    private readonly int totalNoOfLines = 100;
    [SerializeField] private int textChanged = 0;

    // Start is called before the first frame update
    private void Start()
    {
        Console.text = string.Empty;
    }

    public void PresentProgress(int progressPercent)
    {
        if(progressPercent == -1)
        {
            if (consoleMessages[consoleMessages.Count - 1].Contains("%"))
                consoleMessages.RemoveAt(consoleMessages.Count - 1);

            return;
        }

        int points = progressPercent / 10;
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < points; ++i)
            builder.Append(".");

        builder.Append(progressPercent);
        builder.Append("%");

        if (consoleMessages[consoleMessages.Count - 1].Contains("%"))
            consoleMessages.RemoveAt(consoleMessages.Count - 1);

        AddMessage(builder.ToString(), MessageType.Info);
    }

    public void AddCommand(string text, MessageType type)
    {
        commandHistory.Add(text);
        if (commandHistory.Count > totalNoOfLines)
        {
            commandHistory.RemoveAt(0);
        }

        HandleMessage(text, type);
    }

    public void AddMessage(string text, MessageType type)
    {
        HandleMessage(text, type);

        while (consoleMessages.Count > totalNoOfLines)
        {
            consoleMessages.RemoveAt(0);
        }

        Console.text = consoleMessages.ToTextConsole();
        textChanged++;
    }

    private void LateUpdate()
    {
        if (scrollbar.IsActive() && textChanged!=0)
        {
            scrollbar.value = 0;
            textChanged--;
        }
    }

    private void HandleMessage(string message, MessageType type)
    {
        //make the test change based on different message types: Info, Warning, Error

        if (type == MessageType.Warning)
        {
            message = $"<color=\"orange\">{message}</color>";
        }

        if (type == MessageType.Error)
        {
            message = $"<color=\"red\">{message}</color>";
        }

        consoleMessages.Add(message);
    }

    internal void ClearConsole()
    {
        consoleMessages.Clear();
        Console.text = string.Empty;
    }
}