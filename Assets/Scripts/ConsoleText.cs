using Assets.Scripts.Extensions;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ConsoleText : MonoBehaviour
{
    public TextMeshProUGUI Console;
    private readonly Queue<string> ConsoleMessages = new Queue<string>();
    private int acceptedNoOfLines;
    private readonly int lineWidth = 70;

    // Start is called before the first frame update
    private void Start()
    {
        Console.text = string.Empty;

        float lineHeight = Console.fontSize + 2 * (Console.lineSpacing + 1);
        Rect consoleRect = ((RectTransform)transform).rect;
        acceptedNoOfLines = (int)(consoleRect.height / lineHeight);
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

        while (ConsoleMessages.Count > acceptedNoOfLines)
        {
            ConsoleMessages.Dequeue();
        }

        Console.text = ConsoleMessages.ToTextConsole();
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

        ConsoleMessages.Enqueue(message);
    }
}