using Assets.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleText : MonoBehaviour
{
    public Text Console;
    private readonly Queue<string> ConsoleMessages = new Queue<string>();
    // Start is called before the first frame update
    void Start()
    {
        Console.text = string.Empty;
    }

    public void AddMessage(string text)
    {
        //make the test change based on different message types: Info, Warning, Error
        if (ConsoleMessages.Count == 12)
        {
            ConsoleMessages.Dequeue();
        }

        ConsoleMessages.Enqueue(text);
        Console.GetComponent<Text>().text = ConsoleMessages.ToText();
    }
}
