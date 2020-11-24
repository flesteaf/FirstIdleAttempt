using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Commands;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Store;
using Moq;
using NUnit.Framework;
using UnityEditor;

namespace Assets.Scripts.UnitTests.Commands
{
    public class ScanTests
    {
        private IGameData data;
        private GameData actualData;
        private Mock<IGameData> mockData;
        private readonly ScanCommand command = new ScanCommand();
        private List<Tuple<string, MessageType>> console;

        [SetUp]
        public void Init()
        {
            mockData = new Mock<IGameData>();
            data = mockData.Object;

            Mock<GameStore> mockStore = new Mock<GameStore>();
            actualData = new GameData
            {
                Store = mockStore.Object
            };
        }

        [Test]
        public void ScanWithoutParametersTriggersARefreshOfNetworks()
        {
            ScanCommand command = new ScanCommand();
            command.Execute(data, new CommandLine("scan"));

            mockData.Verify(x => x.RefreshNetworks(), Times.Once);
        }

        [Test]
        public void ScanWithoutParametersGeneratesNewNetworksAndRemovesTheUnhackedOnes()
        {
            actualData.RefreshNetworks();
            actualData.MessageSender += ConsoleGathering;
            command.MessageNotification += ConsoleGathering;
            List<HackableNetwork> currentNetworks = actualData.FoundNetworks.ToList();
            int currentNetworksCount = currentNetworks.Count;
            for (int i = 0; i < currentNetworksCount; i += 2)
            {
                var network = currentNetworks.ElementAt(i);
                network.HackNetwork(network.Protection);
            }

            List<HackableNetwork> foundNetworks = actualData.FoundNetworks.ToList();
            command.Execute(actualData, new CommandLine("scan"));

            List<HackableNetwork> newNetworks = actualData.FoundNetworks;

            CollectionAssert.AreEquivalent(foundNetworks.Where(n => n.WasHacked), newNetworks.Where(n => n.WasHacked));
            CollectionAssert.AreNotEquivalent(foundNetworks.Where(n => !n.WasHacked), newNetworks.Where(n => !n.WasHacked));
        }

        [Test]
        public void ScanNetworkPresentsAllDevicesInThatNetwork()
        {
            console = new List<Tuple<string, MessageType>>();
            actualData.RefreshNetworks();
            HackableNetwork network = actualData.FoundNetworks.First();
            network.HackNetwork(network.Protection);
            command.MessageNotification += ConsoleGathering;
            command.Execute(actualData, new CommandLine($"scan network {network.SSID}"));

            string consoleText = string.Join("", console.Select(x => x.Item1));

            for (int i = 0; i < network.Devices.Count; i++)
            {
                Device item = network.Devices[i];
                Assert.IsTrue(consoleText.Contains(item.ToString()), $"Device {i + 1}/{network.Devices.Count} should have been present");
            }
        }

        [Test]
        public void ScanNetworkWithWrongNameShowsAnError()
        {
            console = new List<Tuple<string, MessageType>>();
            command.MessageNotification += ConsoleGathering;
            command.Execute(actualData, new CommandLine($"scan network SomeNetwork"));

            string consoleText = string.Join("", console.Select(x => x.Item1));

            Assert.IsTrue(consoleText.Contains("not found"));
        }

        [Test]
        public void ScanNetworkThatHasProtectionShowsAnError()
        {
            HackableNetwork protectedNetwork = null;
            do
            {
                actualData.RefreshNetworks();
                protectedNetwork = actualData.FoundNetworks.FirstOrDefault(n => n.Protection != ProtectionType.None);
            } while (protectedNetwork == null);

            console = new List<Tuple<string, MessageType>>();
            command.MessageNotification += ConsoleGathering;
            command.Execute(actualData, new CommandLine($"scan network {protectedNetwork.SSID}"));

            string consoleText = string.Join("", console.Select(x => x.Item1));

            Assert.IsTrue(consoleText.Contains("is protected"));
        }

        [Test]
        public void ScanIpThatIsNotPresentShowsAnError()
        {
            console = new List<Tuple<string, MessageType>>();
            command.MessageNotification += ConsoleGathering;
            command.Execute(actualData, new CommandLine("scan ip 123.123.123.123"));

            string consoleText = string.Join("", console.Select(x => x.Item1));

            Assert.IsTrue(consoleText.Contains("not found"));
        }

        [Test]
        public void ScanIpPresentsProperDeviceDetails()
        {
            Phone phone = new Phone { IP = "123.123.123.123", MAC = "12:34:56:78:90" };
            HackableNetwork network = new HackableNetwork { 
                                                    SSID = "test", 
                                                    Devices = new List<Device> { phone }, 
                                                    Protection = ProtectionType.None, 
                                                    NetworkSize = NetworkType.Home };
            
            actualData.AddNetwork(network);
            console = new List<Tuple<string, MessageType>>();
            command.MessageNotification += ConsoleGathering;
            command.Execute(actualData, new CommandLine($"scan ip {phone.IP}"));

            string consoleText = string.Join("", console.Select(x => x.Item1));

            Assert.IsTrue(consoleText.Contains($"firewall {phone.HasFirewall}"));
        }

        [Test]
        public void ScanMacThatIsNotPresentShowsAnError()
        {
            console = new List<Tuple<string, MessageType>>();
            command.MessageNotification += ConsoleGathering;
            command.Execute(actualData, new CommandLine("scan mac 123.123.123.123"));

            string consoleText = string.Join("", console.Select(x => x.Item1));

            Assert.IsTrue(consoleText.Contains("not found"));
        }

        [Test]
        public void ScanMacPresentsProperDeviceDetails()
        {
            Phone phone = new Phone { IP = "123.123.123.123", MAC = "12:34:56:78:90" };
            HackableNetwork network = new HackableNetwork
            {
                SSID = "test",
                Devices = new List<Device> { phone },
                Protection = ProtectionType.None,
                NetworkSize = NetworkType.Home
            };

            actualData.AddNetwork(network);
            console = new List<Tuple<string, MessageType>>();
            command.MessageNotification += ConsoleGathering;
            command.Execute(actualData, new CommandLine($"scan mac {phone.MAC}"));

            string consoleText = string.Join("", console.Select(x => x.Item1));

            Assert.IsTrue(consoleText.Contains($"firewall {phone.HasFirewall}"));
        }

        private void ConsoleGathering(string message, MessageType type)
        {
            if (console == null)
                return;

            console.Add(new Tuple<string, MessageType>(message, type));
        }
    }
}
