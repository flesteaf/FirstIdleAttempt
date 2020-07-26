using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Scripts.UnitTests.Commands
{
    public class BuyTests
    {
        private ISceneManager manager;

        [SetUp]
        public void Initialize()
        {
            Mock<ISceneManager> mockManager = new Mock<ISceneManager>();
            Mock<IConsoleText> mockConsole = new Mock<IConsoleText>();
            mockManager.Setup(x => x.Console).Returns(mockConsole.Object);

            manager = mockManager.Object;
        }

        [Test]
        public void BuySoftware_EnablesTheProperOptionOfTheExistingCommand()
        {

        }
    }
}
