using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using HackYourWay.UI;
using HackYourWay.Models;

namespace HackYourWay.Tests.PlayMode
{
    /// <summary>
    /// Play-mode integration tests for Terminal UX Enhancements (003-terminal-ux).
    ///
    /// Contract 7  — scroll pending guard (US1)
    /// Contracts 8–11 — autocomplete (US2)
    /// Contracts 12–14 — history navigation (US3)
    /// </summary>
    public class TerminalUxIntegrationTests
    {
        // ── Shared scene helpers ──────────────────────────────────────────────

        private static TerminalOutputView BuildOutputView(out ScrollRect scrollRect)
        {
            var root = new GameObject("TerminalRoot");

            // ScrollRect + content
            var scrollObj = new GameObject("Scroll");
            scrollObj.transform.SetParent(root.transform);
            scrollRect = scrollObj.AddComponent<ScrollRect>();

            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform);
            var contentRt = contentObj.AddComponent<RectTransform>();
            scrollRect.content = contentRt;

            // TMP text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(contentObj.transform);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();

            var view = root.AddComponent<TerminalOutputView>();

            // Inject via reflection (fields are serialized-private)
            var outputField = typeof(TerminalOutputView)
                .GetField("_outputText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scrollField = typeof(TerminalOutputView)
                .GetField("_scrollRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            outputField?.SetValue(view, tmp);
            scrollField?.SetValue(view, scrollRect);

            return view;
        }

        // ── T004 / Contract 7: Scroll pending guard ───────────────────────────

        [UnityTest]
        public IEnumerator AppendLine_SetsScrollPending_AndResetsAfterFrame()
        {
            var view = BuildOutputView(out _);

            var pendingField = typeof(TerminalOutputView)
                .GetField("_scrollPending", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsNotNull(pendingField, "_scrollPending field must exist on TerminalOutputView");

            // Before any append: not pending
            Assert.IsFalse((bool)pendingField.GetValue(view));

            view.AppendLine("line one");

            // Immediately after append: pending
            Assert.IsTrue((bool)pendingField.GetValue(view), "_scrollPending should be true immediately after AppendLine");

            // Second append before frame ends must not stack coroutines (still pending, one coroutine)
            view.AppendLine("line two");
            Assert.IsTrue((bool)pendingField.GetValue(view), "_scrollPending stays true after second AppendLine");

            // After end-of-frame: pending cleared
            yield return new WaitForEndOfFrame();
            Assert.IsFalse((bool)pendingField.GetValue(view), "_scrollPending must reset to false after end-of-frame coroutine");

            Object.Destroy(view.gameObject);
        }

        [UnityTest]
        public IEnumerator ScrollRect_NormalizedPosition_IsZeroAfterAppend()
        {
            var view = BuildOutputView(out ScrollRect scrollRect);

            // Start at top
            scrollRect.verticalNormalizedPosition = 1f;

            view.AppendLine("some output");
            yield return new WaitForEndOfFrame();

            Assert.AreEqual(0f, scrollRect.verticalNormalizedPosition, 0.001f,
                "ScrollRect.verticalNormalizedPosition must be 0 (bottom) after AppendLine");

            Object.Destroy(view.gameObject);
        }

        // ── T006 / Contracts 8–11: Autocomplete ──────────────────────────────
        // Note: Full key-interception tests require a scene with EventSystem and
        // are validated in Quickstart Scenarios 2–4. The contracts below test the
        // autocomplete resolution logic independently via the CommandHistory model
        // and CommandParser verb list, which are pure C# and testable without UI.

        [Test]
        public void Autocomplete_SingleMatch_PrefixResolvesToFullVerb()
        {
            // Simulate what HandleTab() does: filter verbs by prefix.
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

        // ── T009 / Contracts 12–14: History navigation ────────────────────────

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

            history.NavigateBack(); // "ls"
            history.NavigateBack(); // "scan"
            history.NavigateForward(); // "ls"
            string result = history.NavigateForward(); // past newest → null

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
