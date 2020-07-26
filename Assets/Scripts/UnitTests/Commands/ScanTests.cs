using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Commands;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Scripts.UnitTests.Commands
{
    public class ScanTests
    {
        readonly Mock<ISceneManager> mockManager = new Mock<ISceneManager>();
        readonly Mock<IConsoleText> mockConsole = new Mock<IConsoleText>();
        private ISceneManager manager;
        private IConsoleText console;

        [SetUp]
        public void Init()
        {
            console = mockConsole.Object;

            mockManager.Setup(x => x.Console).Returns(console);

            manager = mockManager.Object;
        }


        [Test]
        public void ScanWithoutParametersTriggersARefreshOfNetworks()
        {
            ScanCommand command = new ScanCommand();
            command.Execute(manager, new CommandLine("scan"));

            mockManager.Verify(x => x.RefreshNetworks(), Times.Once);
        }

        [Test]
        public void ScanWithoutParametersGeneratesNewNetworksAndRemovesTheUnhackedOnes()
        {
            AddHackedNetworks();

            List<HackableNetwork> foundNetworks = manager.FoundNetworks;
            manager.ExecuteCommand(new CommandLine("scan"));

            List<HackableNetwork> newNetworks = manager.FoundNetworks;

            CollectionAssert.AreEquivalent(foundNetworks.Where(n => n.WasHacked), newNetworks.Where(n => n.WasHacked));
            CollectionAssert.AreNotEquivalent(foundNetworks.Where(n => !n.WasHacked), newNetworks.Where(n => !n.WasHacked));
        }

        [Test]
        public void ScanNetworkPresentsAllDevicesInThatNetwork()
        {
            manager.RefreshNetworks();
            HackableNetwork network = manager.FoundNetworks.First();
            manager.ExecuteCommand(new CommandLine($"scan network {network.SSID}"));

            string consoleText = string.Join("", console.ConsoleMessages);

            for (int i = 0; i < network.Devices.Count; i++)
            {
                Device item = network.Devices[i];
                Assert.IsTrue(consoleText.Contains(item.ToString()), $"Device {i+1}/{network.Devices.Count} should have been present");
            }
        }

        [Test]
        public void ScanNetworkWithWrongNameShowsAnError()
        {
            manager.ExecuteCommand(new CommandLine($"scan network SomeNetwork"));

            string consoleText = string.Join("", console.ConsoleMessages);

            Assert.IsTrue(consoleText.Contains("not found"));
        }

        [Test]
        public void ScanNetworkThatHasProtectionShowsAnError()
        {
            HackableNetwork protectedNetwork = null;
            do
            {
                manager.RefreshNetworks();
                protectedNetwork = manager.FoundNetworks.FirstOrDefault(n => n.Protection != ProtectionType.None);
            } while (protectedNetwork == null);


            manager.ExecuteCommand(new CommandLine($"scan network {protectedNetwork.SSID}"));

            string consoleText = string.Join("", console.ConsoleMessages);

            Assert.IsTrue(consoleText.Contains("is protected"));
        }

        private void AddHackedNetworks()
        {
            manager.RefreshNetworks();
            List<HackableNetwork> currentNetworks = manager.FoundNetworks;
            int currentNetworksCount = currentNetworks.Count;
            for (int i = 0; i < currentNetworksCount; i += 2)
            {
                var network = currentNetworks.ElementAt(i);
                network.HackNetwork(network.Protection);
            }
        }
    }
}
