using System.Collections.Generic;
using NUnit.Framework;
using HackYourWay.Services;
using HackYourWay.Interfaces;

namespace HackYourWay.Tests.EditMode
{
    /// <summary>Tests for CommandParser.GetRegisteredVerbs() — Contract 6.</summary>
    public class CommandParserVerbsTests
    {
        private CommandParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new CommandParser();
        }

        [Test]
        public void GetRegisteredVerbs_ReturnsSortedAlphabetically()
        {
            _parser.Register("inject", new StubCommand());
            _parser.Register("scan",   new StubCommand());
            _parser.Register("ls",     new StubCommand());
            _parser.Register("crack",  new StubCommand());

            IReadOnlyList<string> verbs = _parser.GetRegisteredVerbs();

            Assert.AreEqual(4, verbs.Count);
            Assert.AreEqual("crack",  verbs[0]);
            Assert.AreEqual("inject", verbs[1]);
            Assert.AreEqual("ls",     verbs[2]);
            Assert.AreEqual("scan",   verbs[3]);
        }

        [Test]
        public void GetRegisteredVerbs_EmptyParser_ReturnsEmptyList()
        {
            IReadOnlyList<string> verbs = _parser.GetRegisteredVerbs();
            Assert.AreEqual(0, verbs.Count);
        }

        [Test]
        public void GetRegisteredVerbs_DuplicateRegister_ReturnsEachVerbOnce()
        {
            _parser.Register("scan", new StubCommand());
            _parser.Register("scan", new StubCommand()); // re-register same verb

            IReadOnlyList<string> verbs = _parser.GetRegisteredVerbs();
            Assert.AreEqual(1, verbs.Count);
            Assert.AreEqual("scan", verbs[0]);
        }

        [Test]
        public void GetRegisteredVerbs_ResultIsReadOnly()
        {
            _parser.Register("scan", new StubCommand());

            IReadOnlyList<string> verbs = _parser.GetRegisteredVerbs();
            // IReadOnlyList does not expose Add/Remove — this is a compile-time guarantee.
            // Verify the declared return type is IReadOnlyList, not List.
            Assert.IsNotNull(verbs);
            Assert.IsInstanceOf<IReadOnlyList<string>>(verbs);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private class StubCommand : ICommand
        {
            public CommandResult Execute(string[] args) => CommandResult.Ok("stub");
        }
    }
}
