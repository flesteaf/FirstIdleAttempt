using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public CommandLineField InputText;
    public ConsoleText Console;
    public MissionText Missions;
    public MoneyText Money;

    private GameManager manager;

    private void Awake()
    {
        //This can be done only in the main scene
        manager = FindObjectOfType<GameManager>();

        InputText = FindObjectOfType<CommandLineField>();
        Console = FindObjectOfType<ConsoleText>();
        Missions = FindObjectOfType<MissionText>();
        Money = FindObjectOfType<MoneyText>();
    }

    private void Start()
    {
        Money.UpdateText(manager.MoneyAmmount);
    }
}

