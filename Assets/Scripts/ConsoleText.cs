using Assets.Scripts.Extensions;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ConsoleText : MonoBehaviour
{
    public TextMeshProUGUI Console;
    public List<string> ConsoleMessages { get => consoleMessages.ToList(); }

    private readonly Queue<string> consoleMessages = new Queue<string>();
    private int acceptedNoOfLines;
    private readonly int lineWidth = 70;
    private readonly int totalNoOfLines = 100;

    // Start is called before the first frame update
    private void Start()
    {
        Console.text = string.Empty;

        float lineHeight = Console.fontSize + 2 * (Console.lineSpacing + 1);
        Rect consoleRect = GetComponent<RectTransform>().rect;
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

        while (consoleMessages.Count > totalNoOfLines)
        {
            consoleMessages.Dequeue();
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

        consoleMessages.Enqueue(message);
    }
}