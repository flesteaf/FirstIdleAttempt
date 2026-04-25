using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using HackYourWay.UI;

namespace HackYourWay.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for <see cref="TerminalController.AwaitSelection"/> state machine.
    /// Arrow-key navigation and Enter-key confirmation require play-mode with an active
    /// InputSystem device; those paths are covered by PlayMode tests.
    /// </summary>
    public class TerminalControllerTests
    {
        private GameObject         _go;
        private TerminalController _tc;

        // Reflection handles — SelectionState is a private struct
        private static readonly FieldInfo  SelectionStateField =
            typeof(TerminalController).GetField("_selectionState",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo HandleSelectionInputMethod =
            typeof(TerminalController).GetMethod("HandleSelectionInput",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo ConfirmSelectionMethod =
            typeof(TerminalController).GetMethod("ConfirmSelection",
                BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TerminalController");
            _tc = _go.AddComponent<TerminalController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // ── AwaitSelection setup ──────────────────────────────────────────────

        [Test]
        public void AwaitSelection_SetsSelectionState()
        {
            _tc.AwaitSelection(new[] { "opt1", "opt2" }, _ => { });

            var state = SelectionStateField.GetValue(_tc);
            Assert.IsNotNull(state, "Selection state should be set after AwaitSelection.");
        }

        [Test]
        public void AwaitSelection_AppendsCancelAsLastOption()
        {
            string[] captured = null;
            _tc.AwaitSelection(new[] { "Alpha", "Beta" }, _ => { });

            // Read the Options array from the private SelectionState struct
            var state   = SelectionStateField.GetValue(_tc);
            var options = (string[]) state.GetType()
                .GetField("Options", BindingFlags.Public | BindingFlags.Instance)
                .GetValue(state);

            captured = options;
            Assert.AreEqual(3, captured.Length);
            Assert.AreEqual("Cancel", captured[2]);
        }

        [Test]
        public void AwaitSelection_NullOutputView_DoesNotThrow()
        {
            // _outputView is null in EditMode — AwaitSelection must be safe
            Assert.DoesNotThrow(() => _tc.AwaitSelection(new[] { "x" }, _ => { }));
        }

        // ── Digit input: jump + confirm ───────────────────────────────────────

        [Test]
        public void HandleSelectionInput_ValidDigit_ConfirmsWithCorrectIndex()
        {
            int confirmed = int.MinValue;
            _tc.AwaitSelection(new[] { "Alpha", "Beta" }, idx => confirmed = idx);

            // Typing "2" + Enter → jump to Beta (index 1) and confirm
            HandleSelectionInputMethod.Invoke(_tc, new object[] { "2" });

            Assert.AreEqual(1, confirmed, "Digit '2' should confirm index 1 (Beta).");
        }

        [Test]
        public void HandleSelectionInput_CancelDigit_ReturnsMinusOne()
        {
            int confirmed = int.MinValue;
            _tc.AwaitSelection(new[] { "Alpha", "Beta" }, idx => confirmed = idx);

            // "3" maps to Cancel (last option, index 2)
            HandleSelectionInputMethod.Invoke(_tc, new object[] { "3" });

            Assert.AreEqual(-1, confirmed, "Selecting Cancel should invoke callback with -1.");
        }

        [Test]
        public void HandleSelectionInput_DigitOne_ConfirmsFirstOption()
        {
            int confirmed = int.MinValue;
            _tc.AwaitSelection(new[] { "Alpha" }, idx => confirmed = idx);

            HandleSelectionInputMethod.Invoke(_tc, new object[] { "1" });

            Assert.AreEqual(0, confirmed);
        }

        // ── Free-text and out-of-range → cancel ───────────────────────────────

        [Test]
        public void HandleSelectionInput_FreeText_CancelsWithMinusOne()
        {
            int confirmed = int.MinValue;
            _tc.AwaitSelection(new[] { "Alpha", "Beta" }, idx => confirmed = idx);

            HandleSelectionInputMethod.Invoke(_tc, new object[] { "some typed text" });

            Assert.AreEqual(-1, confirmed, "Free text should cancel selection.");
        }

        [Test]
        public void HandleSelectionInput_OutOfRangeDigit_CancelsWithMinusOne()
        {
            int confirmed = int.MinValue;
            _tc.AwaitSelection(new[] { "Alpha" }, idx => confirmed = idx);

            // Options are Alpha + Cancel = 2 entries; "99" is out of range
            HandleSelectionInputMethod.Invoke(_tc, new object[] { "99" });

            Assert.AreEqual(-1, confirmed, "Out-of-range digit should cancel selection.");
        }

        [Test]
        public void HandleSelectionInput_OutOfRange_DoesNotThrow()
        {
            _tc.AwaitSelection(new[] { "Alpha" }, _ => { });

            Assert.DoesNotThrow(() =>
                HandleSelectionInputMethod.Invoke(_tc, new object[] { "9999" }));
        }

        // ── State cleared after confirm ───────────────────────────────────────

        [Test]
        public void AfterConfirm_SelectionStateIsNull()
        {
            _tc.AwaitSelection(new[] { "Alpha" }, _ => { });
            HandleSelectionInputMethod.Invoke(_tc, new object[] { "1" });

            var state = SelectionStateField.GetValue(_tc);
            Assert.IsNull(state, "Selection state should be cleared after confirmation.");
        }
    }
}
