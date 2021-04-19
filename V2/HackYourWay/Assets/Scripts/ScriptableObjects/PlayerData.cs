using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
using Assets.Scripts.Networks;
using Assets.Scripts.Store;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : ScriptableObject
{
    public List<CommandNames> AvailableSoftware { get; set; }
    public List<CommandOptions> AvailableSoftwareOptions { get; set; }
    public float CurrentProduction { get; set; } = 0;
    public float MoneyAmmount { get; set; } = 0;
    public List<HackableNetwork> FoundNetworks { get; set; }
    public Computer Computer { get; set; }
    public GameStore Store { get; set; }
}
