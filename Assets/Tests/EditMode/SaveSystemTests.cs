using System.IO;
using NUnit.Framework;
using HackYourWay.Core;
using HackYourWay.Models;

namespace HackYourWay.Tests.EditMode
{
    public class SaveSystemTests
    {
        private string _testPath;
        private SaveSystem _saveSystem;

        [SetUp]
        public void SetUp()
        {
            _testPath  = Path.Combine(Path.GetTempPath(), "hackyourway_test_save.json");
            _saveSystem = new SaveSystem(_testPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testPath))
                File.Delete(_testPath);
        }

        [Test]
        public void Load_WhenFileAbsent_ReturnsDefaultSaveData()
        {
            if (File.Exists(_testPath)) File.Delete(_testPath);
            var data = _saveSystem.Load();
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.Player);
            Assert.AreEqual(1, data.Version);
        }

        [Test]
        public void Save_ThenLoad_PreservesPlayerBalance()
        {
            var data = new SaveData();
            data.Player.AddBalance(CurrencyType.Bitcoin, 42.5);

            _saveSystem.Save(data);
            var loaded = _saveSystem.Load();

            Assert.AreEqual(42.5, loaded.Player.GetBalance(CurrencyType.Bitcoin), 0.0001);
        }

        [Test]
        public void Save_ThenLoad_PreservesUnlockedTools()
        {
            var data = new SaveData();
            data.Player.UnlockedTools.Add(ToolType.CrackWEP);

            _saveSystem.Save(data);
            var loaded = _saveSystem.Load();

            Assert.IsTrue(loaded.Player.HasTool(ToolType.CrackWEP));
        }

        [Test]
        public void Save_ThenLoad_PreservesLastSaveUtcTicks()
        {
            var data = new SaveData();
            data.Player.LastSaveUtcTicks = 637_000_000_000_000_000L;

            _saveSystem.Save(data);
            var loaded = _saveSystem.Load();

            Assert.AreEqual(637_000_000_000_000_000L, loaded.Player.LastSaveUtcTicks);
        }
    }
}
