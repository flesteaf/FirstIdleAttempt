using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using HackYourWay.Core;
using HackYourWay.Interfaces;
using HackYourWay.Models;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    public class SavesCommandTests
    {
        private string        _testDir;
        private SlotSaveSystem _slotSystem;

        [SetUp]
        public void SetUp()
        {
            _testDir    = Path.Combine(Path.GetTempPath(), "hyw_saves_cmd_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
            _slotSystem = new SlotSaveSystem(_testDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, recursive: true);
        }

        private SavesCommand MakeCommand() => new SavesCommand(_slotSystem);

        private static string[] SlotLines(string message)
        {
            string[] raw = message.Split('\n');
            var lines = new System.Collections.Generic.List<string>();
            for (int i = 0; i < raw.Length; i++)
            {
                string line = raw[i].Trim();
                if (line.StartsWith("Slot"))
                    lines.Add(line);
            }
            return lines.ToArray();
        }

        [Test]
        public void Execute_AllEmpty_Returns7LinesWithEmptyMarker()
        {
            CommandResult result = MakeCommand().Execute(System.Array.Empty<string>());
            Assert.IsTrue(result.Success);
            string[] lines = SlotLines(result.Message);
            Assert.AreEqual(7, lines.Length, "Expected exactly 7 slot lines");
            for (int i = 0; i < lines.Length; i++)
                Assert.IsTrue(lines[i].Contains("[empty]"), $"Expected [empty] in: {lines[i]}");
        }

        [Test]
        public void Execute_OccupiedSlot_ShowsTimestampInCorrectFormat()
        {
            _slotSystem.SaveSlot(1, new SaveData());
            CommandResult result = MakeCommand().Execute(System.Array.Empty<string>());
            Assert.IsTrue(result.Success);

            string[] lines = SlotLines(result.Message);
            string   slot1 = null;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("Slot 1:"))
                {
                    slot1 = lines[i];
                    break;
                }
            }

            Assert.IsNotNull(slot1, "Slot 1 line not found");
            Assert.IsFalse(slot1.Contains("[empty]"), "Slot 1 should show a timestamp");
            Assert.IsTrue(Regex.IsMatch(slot1, @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}"),
                $"Expected yyyy-MM-dd HH:mm pattern in: {slot1}");
        }

        [Test]
        public void Execute_AllOccupied_Shows7TimestampLines()
        {
            for (int s = 1; s <= 7; s++)
                _slotSystem.SaveSlot(s, new SaveData());

            CommandResult result = MakeCommand().Execute(System.Array.Empty<string>());
            Assert.IsTrue(result.Success);

            string[] lines   = SlotLines(result.Message);
            int      withTs  = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains("[empty]"))
                    withTs++;
            }

            Assert.AreEqual(7, withTs, "Expected 7 timestamp lines");
        }

        [Test]
        public void Execute_MixedOccupancy_OccupiedSlotsShowTimestamp_EmptySlotsShowEmpty()
        {
            _slotSystem.SaveSlot(1, new SaveData());
            _slotSystem.SaveSlot(3, new SaveData());

            CommandResult result = MakeCommand().Execute(System.Array.Empty<string>());
            string[] lines = SlotLines(result.Message);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("Slot 1:") || line.StartsWith("Slot 3:"))
                    Assert.IsFalse(line.Contains("[empty]"), $"Should be occupied: {line}");
                else
                    Assert.IsTrue(line.Contains("[empty]"), $"Should be empty: {line}");
            }
        }

        [Test]
        public void Execute_Returns7SlotLinesRegardlessOfArgs()
        {
            CommandResult result = MakeCommand().Execute(new[] { "ignored" });
            Assert.AreEqual(7, SlotLines(result.Message).Length);
        }
    }
}
