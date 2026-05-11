using System.IO;
using NUnit.Framework;
using UnityEngine;
using HackYourWay.Core;
using HackYourWay.Models;

namespace HackYourWay.Tests.EditMode
{
    public class SlotSaveSystemTests
    {
        private string        _testDir;
        private SlotSaveSystem _system;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "hyw_slot_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
            _system  = new SlotSaveSystem(_testDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, recursive: true);
        }

        // ── Round-trip ──────────────────────────────────────────────────────

        [Test]
        public void SaveSlot_ThenLoadSlot_PreservesBalance()
        {
            var data = new SaveData();
            data.Player.AddBalance(CurrencyType.Bitcoin, 99.5);
            _system.SaveSlot(1, data);

            SaveData loaded = _system.LoadSlot(1);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(99.5, loaded.Player.GetBalance(CurrencyType.Bitcoin), 0.0001);
        }

        [Test]
        public void SaveSlot_ThenLoadSlot_AllSlotsIndependent()
        {
            for (int s = 1; s <= 7; s++)
            {
                var d = new SaveData();
                d.Player.AddBalance(CurrencyType.Bitcoin, s * 10.0);
                _system.SaveSlot(s, d);
            }

            for (int s = 1; s <= 7; s++)
            {
                SaveData loaded = _system.LoadSlot(s);
                Assert.AreEqual(s * 10.0, loaded.Player.GetBalance(CurrencyType.Bitcoin), 0.0001,
                    $"Slot {s} wrong balance");
            }
        }

        // ── LoadSlot edge cases ─────────────────────────────────────────────

        [Test]
        public void LoadSlot_WhenFileAbsent_ReturnsNull()
        {
            Assert.IsNull(_system.LoadSlot(3));
        }

        // ── Index consistency ───────────────────────────────────────────────

        [Test]
        public void SaveSlot_UpdatesIndex_IsOccupiedTrue()
        {
            _system.SaveSlot(2, new SaveData());
            SaveSlotIndex index = _system.LoadIndex();
            Assert.IsTrue(index.Slots[1].IsOccupied);
            Assert.Greater(index.Slots[1].SavedAtUtcTicks, 0L);
        }

        [Test]
        public void LoadIndex_WhenFileAbsent_Returns7EntryDefault()
        {
            SaveSlotIndex index = _system.LoadIndex();
            Assert.IsNotNull(index);
            Assert.AreEqual(7, index.Slots.Length);
            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(i + 1, index.Slots[i].SlotNumber);
                Assert.IsFalse(index.Slots[i].IsOccupied);
                Assert.AreEqual(0L, index.Slots[i].SavedAtUtcTicks);
            }
        }

        // ── Delete ──────────────────────────────────────────────────────────

        [Test]
        public void DeleteSlot_RemovesFile_AndClearsIndexEntry()
        {
            _system.SaveSlot(4, new SaveData());
            _system.DeleteSlot(4);

            Assert.IsNull(_system.LoadSlot(4));
            SaveSlotIndex index = _system.LoadIndex();
            Assert.IsFalse(index.Slots[3].IsOccupied);
            Assert.AreEqual(0L, index.Slots[3].SavedAtUtcTicks);
        }

        [Test]
        public void DeleteSlot_WhenFileAbsent_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _system.DeleteSlot(5));
        }

        // ── Slot range validation ───────────────────────────────────────────

        [Test]
        public void SaveSlot_Slot0_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _system.SaveSlot(0, new SaveData()));
        }

        [Test]
        public void SaveSlot_Slot8_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _system.SaveSlot(8, new SaveData()));
        }

        [Test]
        public void LoadSlot_Slot0_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _system.LoadSlot(0));
        }

        // ── Migration ───────────────────────────────────────────────────────

        [Test]
        public void MigrateLegacyIfNeeded_WhenLegacyExistsAndNoSlotFiles_CopiesLegacyToSlot1()
        {
            var data = new SaveData();
            data.Player.AddBalance(CurrencyType.Bitcoin, 77.0);
            File.WriteAllText(
                Path.Combine(_testDir, "save.json"),
                JsonUtility.ToJson(data));

            bool migrated = _system.MigrateLegacyIfNeeded();

            Assert.IsTrue(migrated);
            SaveData loaded = _system.LoadSlot(1);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(77.0, loaded.Player.GetBalance(CurrencyType.Bitcoin), 0.0001);
            Assert.IsTrue(File.Exists(Path.Combine(_testDir, "save.json")), "Legacy file must be preserved");
        }

        [Test]
        public void MigrateLegacyIfNeeded_WhenSlotFilesExist_DoesNotMigrate()
        {
            _system.SaveSlot(1, new SaveData());
            File.WriteAllText(Path.Combine(_testDir, "save.json"), JsonUtility.ToJson(new SaveData()));

            bool migrated = _system.MigrateLegacyIfNeeded();

            Assert.IsFalse(migrated);
        }

        [Test]
        public void MigrateLegacyIfNeeded_WhenNoLegacyFile_DoesNotMigrate()
        {
            Assert.IsFalse(_system.MigrateLegacyIfNeeded());
        }

        [Test]
        public void MigrateLegacyIfNeeded_UpdatesIndex_Slot1Occupied()
        {
            File.WriteAllText(Path.Combine(_testDir, "save.json"), JsonUtility.ToJson(new SaveData()));

            _system.MigrateLegacyIfNeeded();

            Assert.IsTrue(_system.LoadIndex().Slots[0].IsOccupied);
        }
    }
}
