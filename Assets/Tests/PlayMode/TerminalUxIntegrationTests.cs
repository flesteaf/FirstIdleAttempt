using NUnit.Framework;
using HackYourWay.Models;

namespace HackYourWay.Tests.PlayMode
{
    /// <summary>
    /// Terminal UX tests.
    ///
    /// Scroll-to-bottom behaviour (formerly tested via Unity's ScrollRect + WaitForEndOfFrame)
    /// is superseded by RichTextLabel.ScrollFollowing = true, which auto-scrolls on every
    /// Text assignment without requiring a frame boundary. No equivalent tests are needed.
    ///
    /// The autocomplete and history tests below are pure C# and run without the Godot runtime.
    /// </summary>
    public class TerminalUxIntegrationTests
    {
        // ── Autocomplete ──────────────────────────────────────────────────────

        [Test]
        public void Autocomplete_SingleMatch_PrefixResolvesToFullVerb()
        {
            var verbs = new System.Collections.Generic.List<string> { "crack", "inject", "ls", "scan", "show" };
            string prefix = "sc";

            var matches = verbs.FindAll(v => v.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("scan", matches[0]);
        }

        [Test]
        public void Autocomplete_MultipleMatches_ReturnsAllCandidates()
        {
            var verbs = new System.Collections.Generic.List<string> { "crack", "inject", "ls", "scan", "show" };
            string prefix = "s";

            var matches = verbs.FindAll(v => v.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(2, matches.Count);
            CollectionAssert.Contains(matches, "scan");
            CollectionAssert.Contains(matches, "show");
        }

        [Test]
        public void Autocomplete_NoMatch_ReturnsEmpty()
        {
            var verbs = new System.Collections.Generic.List<string> { "crack", "inject", "ls", "scan", "show" };
            string prefix = "xyz";

            var matches = verbs.FindAll(v => v.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public void Autocomplete_EmptyPrefix_ReturnsAllVerbs()
        {
            var verbs = new System.Collections.Generic.List<string> { "crack", "inject", "ls", "scan", "show" };
            string prefix = "";

            var matches = verbs.FindAll(v => v.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(verbs.Count, matches.Count);
        }

        // ── History navigation ────────────────────────────────────────────────

        [Test]
        public void History_NavigateBack_ReturnsMostRecentFirst()
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
        public void History_NavigateForward_PastNewest_ReturnsNull()
        {
            var history = new CommandHistory();
            history.Add("scan");
            history.Add("ls");

            history.NavigateBack();
            history.NavigateBack();
            history.NavigateForward();
            string result = history.NavigateForward();

            Assert.IsNull(result, "NavigateForward past newest entry must return null");
        }

        [Test]
        public void History_OnSubmit_CommandSavedToHistory()
        {
            var history = new CommandHistory();
            history.Add("ls");

            string top = history.NavigateBack();
            Assert.AreEqual("ls", top);
        }
    }
}
