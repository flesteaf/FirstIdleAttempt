using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Softwares;
using UnityEditor;

namespace Assets.Scripts
{
    public static class HackerDelegates
    {
        public delegate void NetworkHackedEventHandler(HackableNetwork hackedNetwork);
        public delegate void DeviceInfectedEventHandler(Device infectedDevice, InfectionType infectionType);
        public delegate void SendMessageEventHandler(string message, MessageType type);
        public delegate void ActionProgressEventHandler(int progress);
        public delegate void ClearConsoleEventHandler();
    }
}
