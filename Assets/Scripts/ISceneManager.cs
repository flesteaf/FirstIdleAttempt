using Assets.Scripts.Commands;
using Assets.Scripts.Networks;
using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using System.Collections.Generic;

public interface ISceneManager
{
    List<HackableNetwork> FoundNetworks { get; }
    IConsoleText Console { get; }
    IStore Store { get; }

    void ExecuteCommand(CommandLine command);
    HackableNetwork GetNetworkBySSID(string ssid);
    void RefreshNetworks();
    bool TryBuyComponent(StoreComponent component);
    bool TryBuySoftware(Software software);
}