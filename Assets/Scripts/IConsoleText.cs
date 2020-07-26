using System.Collections.Generic;
using UnityEditor;

public interface IConsoleText
{
    List<string> ConsoleMessages { get; }

    void AddMessage(string text, MessageType type);
}