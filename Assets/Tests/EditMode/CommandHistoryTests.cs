using NUnit.Framework;
using HackYourWay.Models;

namespace HackYourWay.Tests.EditMode
{
    /// <summary>
    /// Edit-mode unit tests for <see cref="CommandHistory"/>.
    /// Covers Contracts 1–5 from specs/003-terminal-ux/contracts/terminal-ux-contracts.md.
    /// </summary>
    public class CommandHistoryTests
    {
        // ── Contract 1: Add and cap ───────────────────────────────────────────

        [Test]
        public void Add_FiftyEntries_CountIsFifty()
        {
            var history = new CommandHistory();
            for (int i = 0; i < CommandHistory.MaxEntries; i++)
                history.Add($"cmd_{i}");

            // Navigate all the way back to count entries.
            int count = 0;
            while (history.NavigateBack() != null)
                count++;

            Assert.AreEqual(CommandHistory.MaxEntries, count);
        }

        [Test]
        public void Add_FiftyFirstEntry_OldestIsDropped()
        {
            var history = new CommandHistory();
            for (int i = 0; i < CommandHistory.MaxEntries; i++)
                history.Add($"cmd_{i}");

            history.Add("cmd_newest");

            // Navigate all the way back; oldest should be cmd_1 (cmd_0 was dropped).
            string oldest = null;
            string current;
            while ((current = history.NavigateBack()) != null)
                oldest = current;

            Assert.AreEqual("cmd_1", oldest, "cmd_0 should have been dropped; cmd_1 is now oldest");
        }

        [Test]
        public void Add_FiftyFirstEntry_NewestIsRetained()
        {
            var history = new CommandHistory();
            for (int i = 0; i < CommandHistory.MaxEntries; i++)
                history.Add($"cmd_{i}");

            history.Add("cmd_newest");

            string newest = history.NavigateBack();
            Assert.AreEqual("cmd_newest", newest);
        }

        // ── Contract 2: NavigateBack ──────────────────────────────────────────

        [Test]
        public void NavigateBack_ReturnsNewestFirst()
        {
            var history = new CommandHistory();
            history.Add("scan");
            history.Add("ls");
            history.Add("inject miner");

            Assert.AreEqual("inject miner", history.NavigateBack());
            Assert.AreEqual("ls",           history.NavigateBack());
            Assert.AreEqual("scan",         history.NavigateBack());
        }

        [Test]
        public void NavigateBack_AtOldest_DoesNotGoBelow()
        {
            var history = new CommandHistory();
            history.Add("scan");

            history.NavigateBack(); // "scan"
            string result = history.NavigateBack(); // should still be "scan"

            Assert.AreEqual("scan", result, "NavigateBack at oldest entry should return the oldest, not go below");
        }

        [Test]
        public void NavigateBack_EmptyHistory_ReturnsNull()
        {
            var history = new CommandHistory();
            Assert.IsNull(history.NavigateBack());
        }

        // ── Contract 3: NavigateForward ───────────────────────────────────────

        [Test]
        public void NavigateForward_PastNewest_ReturnsNull()
        {
            var history = new CommandHistory();
            history.Add("scan");
            history.Add("ls");
            history.Add("inject miner");

            history.NavigateBack(); // "inject miner"
            history.NavigateBack(); // "ls"
            history.NavigateBack(); // "scan"

            history.NavigateForward(); // "ls"
            history.NavigateForward(); // "inject miner"
            string result = history.NavigateForward(); // past newest

            Assert.IsNull(result, "NavigateForward past newest must return null");
        }

        [Test]
        public void NavigateForward_AfterNullResult_NavIndexIsReset()
        {
            var history = new CommandHistory();
            history.Add("scan");

            history.NavigateBack();       // "scan"
            history.NavigateForward();    // past newest → null, navIndex reset

            // After reset, NavigateBack should return "scan" again (fresh browse)
            string result = history.NavigateBack();
            Assert.AreEqual("scan", result);
        }

        // ── Contract 4: ResetNavigation ───────────────────────────────────────

        [Test]
        public void ResetNavigation_MidBrowse_ResetsIndex()
        {
            var history = new CommandHistory();
            history.Add("scan");
            history.Add("ls");

            history.NavigateBack(); // "ls"
            history.ResetNavigation();

            // After reset, navigating back starts from newest again.
            string result = history.NavigateBack();
            Assert.AreEqual("ls", result, "After ResetNavigation, NavigateBack should return the newest entry");
        }

        [Test]
        public void ResetNavigation_DoesNotClearEntries()
        {
            var history = new CommandHistory();
            history.Add("scan");
            history.Add("ls");

            history.NavigateBack();
            history.ResetNavigation();

            // Both entries should still be navigable.
            Assert.AreEqual("ls",   history.NavigateBack());
            Assert.AreEqual("scan", history.NavigateBack());
        }

        // ── Contract 5: Add resets navigation ────────────────────────────────

        [Test]
        public void Add_WhileBrowsing_ResetsNavIndex()
        {
            var history = new CommandHistory();
            history.Add("scan");
            history.Add("ls");

            history.NavigateBack(); // now at "ls"

            history.Add("new command"); // should reset navIndex

            // NavigateBack from fresh state → "new command" (newest)
            string newest = history.NavigateBack();
            Assert.AreEqual("new command", newest);
        }
    }
}
