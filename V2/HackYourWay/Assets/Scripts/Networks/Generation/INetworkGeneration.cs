using Assets.Scripts.Networks;

namespace HackingYourWay.Assets.Scripts.Networks.Generation
{
    internal interface INetworkGeneration
    {
        NetworkType Type { get; }
        int NoOfDevices { get; }
        ProtectionType Protection { get; }
        string GetSSID();
    }
}