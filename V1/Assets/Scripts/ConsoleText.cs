using Assets.Scripts;
using Assets.Scripts.Extensions;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ConsoleText : MonoBehaviour
{
    public TextMeshProUGUI Console;

    private readonly List<string> consoleMessages = new List<string>();
    private int acceptedNoOfLines;
    private readonly int lineWidth = 70;
    private readonly List<string> commandHistory = new List<string>();
    private readonly int totalNoOfLines = 100;

    // Start is called before the first frame update
    private void Start()
    {
        Console.text = string.Empty;

        float lineHeight = Console.fontSize + 2 * (Console.lineSpacing + 1);
        Rect consoleRect = GetComponent<RectTransform>().rect;
        acceptedNoOfLines = (int)(consoleRect.height / lineHeight);
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
        if (text.Length > lineWidth)
        {
            HandleMessage(text.Substring(0, lineWidth), type);
            HandleMessage(text.Substring(lineWidth), type);
        }
        else
        {
            HandleMessage(text, type);
        }

        while (consoleMessages.Count > totalNoOfLines)
        {
            consoleMessages.RemoveAt(0);
        }

        Console.text = consoleMessages.ToTextConsole(acceptedNoOfLines);
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