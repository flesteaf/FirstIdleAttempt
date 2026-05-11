using System.IO;
using NUnit.Framework;
using HackYourWay.Core;
using HackYourWay.Interfaces;
using HackYourWay.Models;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    public class LoadCommandTests
    {
        private string        _testDir;
        private SlotSaveSystem _slotSystem;
        private int           _loadedSlot;

        [SetUp]
        public void SetUp()
        {
            _testDir    = Path.Combine(Path.GetTempPath(), "hyw_load_cmd_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
            _slotSystem = new SlotSaveSystem(_testDir);
            _loadedSlot = -1;
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, recursive: true);
        }

        private LoadCommand MakeCommand() =>
            new LoadCommand(_slotSystem, slot => _loadedSlot = slot);

        [Test]
        public void Execute_OccupiedSlot_CallsCallbackAndReturnsSuccess()
        {
            _slotSystem.SaveSlot(2, new SaveData());
            CommandResult result = MakeCommand().Execute(new[] { "2" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Game loaded from slot 2.", result.Message);
            Assert.AreEqual(2, _loadedSlot);
        }

        [Test]
        public void Execute_EmptySlot_ReturnsError()
        {
            CommandResult result = MakeCommand().Execute(new[] { "6" });
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Slot 6 is empty.", result.Message);
        }

        [Test]
        public void Execute_EmptySlot_DoesNotFireCallback()
        {
            MakeCommand().Execute(new[] { "6" });
            Assert.AreEqual(-1, _loadedSlot);
        }

        [Test]
        public void Execute_Slot0_ReturnsRangeError()
        {
            CommandResult result = MakeCommand().Execute(new[] { "0" });
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Slot must be between 1 and 7.", result.Message);
        }

        [Test]
        public void Execute_Slot9_ReturnsRangeError()
        {
            CommandResult result = MakeCommand().Execute(new[] { "9" });
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Slot must be between 1 and 7.", result.Message);
        }

        [Test]
        public void Execute_MissingArg_ReturnsUsageError()
        {
            CommandResult result = MakeCommand().Execute(System.Array.Empty<string>());
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Usage: load <slot>", result.Message);
        }

        [Test]
        public void Execute_NonIntegerArg_ReturnsRangeError()
        {
            CommandResult result = MakeCommand().Execute(new[] { "abc" });
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Slot must be between 1 and 7.", result.Message);
        }
    }
}
