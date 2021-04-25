using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
using Assets.Scripts.Networks;
using System;
using System.Collections.Generic;

public class PlayerData
{
    public TimeSpan Timestamp { get; set; }
    public List<CommandNames> AvailableSoftware { get; set; }
    public List<CommandOptions> AvailableSoftwareOptions { get; set; }
    public float CurrentProduction { get; set; } = 0;
    public float MoneyAmmount { get; set; } = 0;
    public List<HackableNetwork> FoundNetworks { get; set; }
    public Computer Computer { get; set; }
    public object Store { get; internal set; }
}
