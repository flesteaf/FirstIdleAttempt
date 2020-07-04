using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Scripts.UnitTests.Commands
{
    public class BuyTests
    {
        private GameManager manager;

        [SetUp]
        public void Initialize()
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
        public void BuySoftware_EnablesTheProperOptionOfTheExistingCommand()
        {

        }
    }
}
