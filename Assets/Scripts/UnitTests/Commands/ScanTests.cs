using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Scripts.UnitTests.Commands
{
    public class ScanTests
    {
        private GameManager manager;

        [SetUp]
        public void Init()
        {
            var rootObject = new GameObject();
            rootObject.AddComponent<RectTransform>();
            var moneyText = rootObject.AddComponent<MoneyText>();
            moneyText.SetupMoneyText();
            var consoleText = rootObject.AddComponent<ConsoleText>();
            consoleText.SetupConsoleText();
            manager = rootObject.AddComponent<GameManager>();
        }


        [Test]
        public void ScanWithoutParametersGeneratesNewNetworks()
        {
            int networksFound = manager.GetAllFoundNetworks().Count();
            manager.ExecuteCommand("scan");
            int newNetworksCount = manager.GetAllFoundNetworks().Count();

            Assert.IsTrue(networksFound < newNetworksCount);
        }

        [Test]
        public void ScanWithoutParametersGeneratesNewNetworksAndRemovesTheUnhackedOnes()
        {
            AddHackedNetworks();

            List<HackableNetwork> foundNetworks = manager.GetAllFoundNetworks().ToList();
            manager.ExecuteCommand("scan");

            List<HackableNetwork> newNetworks = manager.GetAllFoundNetworks().ToList();

            CollectionAssert.AreEquivalent(foundNetworks.Where(n => n.WasHacked), newNetworks.Where(n => n.WasHacked));
            CollectionAssert.AreNotEquivalent(foundNetworks.Where(n => !n.WasHacked), newNetworks.Where(n => !n.WasHacked));
        }

        [Test]
        public void ScanNetworkPresentsAllDevicesInThatNetwork()
        {
            manager.RefreshNetworks();
            HackableNetwork network = manager.GetAllFoundNetworks().First();
            manager.ExecuteCommand($"scan network {network.SSID}");

            string consoleText = string.Join("", manager.SceneManager.Console.ConsoleMessages);

            for (int i = 0; i < network.Devices.Count; i++)
            {
                Device item = network.Devices[i];
                Assert.IsTrue(consoleText.Contains(item.ToString()), $"Device {i+1}/{network.Devices.Count} should have been present");
            }
        }

        [Test]
        public void ScanNetworkWithWrongNameShowsAnError()
        {
            manager.ExecuteCommand($"scan network SomeNetwork");

            string consoleText = string.Join("", manager.SceneManager.Console.ConsoleMessages);

            Assert.IsTrue(consoleText.Contains("not found"));
        }

        [Test]
        public void ScanNetworkThatHasProtectionShowsAnError()
        {
            HackableNetwork protectedNetwork = null;
            do
            {
                manager.RefreshNetworks();
                protectedNetwork = manager.GetAllFoundNetworks().FirstOrDefault(n => n.Protection != ProtectionType.None);
            } while (protectedNetwork == null);


            manager.ExecuteCommand($"scan network {protectedNetwork.SSID}");

            string consoleText = string.Join("", manager.SceneManager.Console.ConsoleMessages);

            Assert.IsTrue(consoleText.Contains("is protected"));
        }

        private void AddHackedNetworks()
        {
            manager.RefreshNetworks();
            IEnumerable<HackableNetwork> currentNetworks = manager.GetAllFoundNetworks();
            int currentNetworksCount = currentNetworks.Count();
            for (int i = 0; i < currentNetworksCount; i += 2)
            {
                var network = currentNetworks.ElementAt(i);
                network.HackNetwork(network.Protection);
            }
        }
    }
}
