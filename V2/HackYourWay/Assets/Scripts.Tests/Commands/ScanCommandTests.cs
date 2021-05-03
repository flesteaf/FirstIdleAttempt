using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Commands;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ScanCommandTests : MonoBehaviour
{
    private ScanCommand scanCommand;
    private GameLogicMock gameLogic;

    [SetUp]
    public void TestInit()
    {
        scanCommand = new ScanCommand();
        gameLogic = new GameLogicMock();
    }

    [TearDown]
    public void Cleanup()
    {
        scanCommand = null;
    }

    [Test]
    public void ScanCommandWithScanRequested()
    {
        CommandLine commandLine = new CommandLine("scan");

        scanCommand.Execute(gameLogic, commandLine, 1);

        Assert.AreEqual(nameof(gameLogic.RefreshNetworks), gameLogic.RequestedMethod);
        Assert.IsNull(gameLogic.InputParameters);
        Assert.IsNull(gameLogic.OutputParameter);
    }
}
