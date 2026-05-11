using System.IO;
using NUnit.Framework;
using HackYourWay.Core;
using HackYourWay.Interfaces;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    public class SaveCommandTests
    {
        private string              _testDir;
        private SlotSaveSystem      _slotSystem;
        private ConfirmationService _confirmService;
        private SaveData            _currentData;
        private int                 _activeSlot;

        [SetUp]
        public void SetUp()
        {
            _testDir        = Path.Combine(Path.GetTempPath(), "hyw_save_cmd_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
            _slotSystem     = new SlotSaveSystem(_testDir);
            _confirmService = new ConfirmationService();
            _currentData    = new SaveData();
            _activeSlot     = -1;
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, recursive: true);
        }

        private SaveCommand MakeCommand() =>
            new SaveCommand(_slotSystem, _confirmService, () => _currentData, slot => _activeSlot = slot);

        // ── Empty slot ──────────────────────────────────────────────────────

        [Test]
        public void Execute_EmptySlot_SavesAndReturnsSuccess()
        {
            CommandResult result = MakeCommand().Execute(new[] { "3" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Game saved to slot 3.", result.Message);
            Assert.AreEqual(3, _activeSlot);
        }

        [Test]
        public void Execute_EmptySlot_FileExistsAfterSave()
        {
            MakeCommand().Execute(new[] { "5" });
            Assert.IsTrue(_slotSystem.LoadIndex().Slots[4].IsOccupied);
        }

        // ── Occupied slot — confirmation flow ───────────────────────────────

        [Test]
        public void Execute_OccupiedSlot_ReturnsConfirmPrompt_AndSetsPending()
        {
            _slotSystem.SaveSlot(3, new SaveData());
            CommandResult result = MakeCommand().Execute(new[] { "3" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Slot 3 is occupied, overwrite? (y/n)", result.Message);
            Assert.IsTrue(_confirmService.IsPending);
        }

        [Test]
        public void Execute_OccupiedSlot_OnConfirm_SavesAndSetsActiveSlot()
        {
            _slotSystem.SaveSlot(3, new SaveData());
            MakeCommand().Execute(new[] { "3" });
            _confirmService.Resolve(true);
            Assert.IsTrue(_slotSystem.LoadIndex().Slots[2].IsOccupied);
            Assert.AreEqual(3, _activeSlot);
        }

        [Test]
        public void Execute_OccupiedSlot_OnCancel_ActiveSlotUnchanged()
        {
            _slotSystem.SaveSlot(3, new SaveData());
            _activeSlot = -1;
            MakeCommand().Execute(new[] { "3" });
            _confirmService.Resolve(false);
            Assert.AreEqual(-1, _activeSlot);
        }

        // ── Range errors ────────────────────────────────────────────────────

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
            Assert.AreEqual("Error: Usage: save <slot>", result.Message);
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
