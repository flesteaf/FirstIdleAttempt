using Assets.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleText : MonoBehaviour
{
    public Text Console;
    private readonly Queue<string> ConsoleMessages = new Queue<string>();
    private int acceptedNoOfLines;
    // Start is called before the first frame update
    void Start()
    {
        Console.text = string.Empty;
        acceptedNoOfLines = (int)(((RectTransform)transform).rect.height / (Console.fontSize + 2* Console.lineSpacing));
    }

    public void AddMessage(string text)
    {
        //make the test change based on different message types: Info, Warning, Error
        if (ConsoleMessages.Count == acceptedNoOfLines)
        {
            ConsoleMessages.Dequeue();
        }

        ConsoleMessages.Enqueue(text);
        Console.GetComponent<Text>().text = ConsoleMessages.ToText();
    }
}
