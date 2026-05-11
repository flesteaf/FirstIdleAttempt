using System.IO;
using NUnit.Framework;
using HackYourWay.Core;
using HackYourWay.Interfaces;
using HackYourWay.Models;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    public class DelsaveCommandTests
    {
        private string        _testDir;
        private SlotSaveSystem _slotSystem;
        private int           _resetSlot;

        [SetUp]
        public void SetUp()
        {
            _testDir    = Path.Combine(Path.GetTempPath(), "hyw_del_cmd_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
            _slotSystem = new SlotSaveSystem(_testDir);
            _resetSlot  = -999;
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, recursive: true);
        }

        private DelsaveCommand MakeCommand() =>
            new DelsaveCommand(_slotSystem, slot => _resetSlot = slot);

        [Test]
        public void Execute_OccupiedSlot_DeletesAndReturnsSuccess()
        {
            _slotSystem.SaveSlot(4, new SaveData());
            CommandResult result = MakeCommand().Execute(new[] { "4" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Save slot 4 removed.", result.Message);
            Assert.IsNull(_slotSystem.LoadSlot(4));
        }

        [Test]
        public void Execute_OccupiedSlot_FiresResetCallback()
        {
            _slotSystem.SaveSlot(4, new SaveData());
            MakeCommand().Execute(new[] { "4" });
            Assert.AreEqual(4, _resetSlot);
        }

        [Test]
        public void Execute_EmptySlot_ReturnsError()
        {
            CommandResult result = MakeCommand().Execute(new[] { "1" });
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Slot 1 has no save to remove.", result.Message);
        }

        [Test]
        public void Execute_EmptySlot_DoesNotFireResetCallback()
        {
            _resetSlot = -999;
            MakeCommand().Execute(new[] { "1" });
            Assert.AreEqual(-999, _resetSlot);
        }

        [Test]
        public void Execute_Slot0_ReturnsRangeError()
        {
            CommandResult result = MakeCommand().Execute(new[] { "0" });
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Slot must be between 1 and 7.", result.Message);
        }

        [Test]
        public void Execute_Slot8_ReturnsRangeError()
        {
            CommandResult result = MakeCommand().Execute(new[] { "8" });
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Slot must be between 1 and 7.", result.Message);
        }

        [Test]
        public void Execute_MissingArg_ReturnsUsageError()
        {
            CommandResult result = MakeCommand().Execute(System.Array.Empty<string>());
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Usage: delsave <slot>", result.Message);
        }

        [Test]
        public void Execute_NonIntegerArg_ReturnsRangeError()
        {
            CommandResult result = MakeCommand().Execute(new[] { "xyz" });
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error: Slot must be between 1 and 7.", result.Message);
        }
    }
}
