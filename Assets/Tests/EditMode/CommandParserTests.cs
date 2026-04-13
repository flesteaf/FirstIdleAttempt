using NUnit.Framework;
using HackYourWay.Services;
using HackYourWay.Interfaces;

namespace HackYourWay.Tests.EditMode
{
    public class CommandParserTests
    {
        private CommandParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new CommandParser();
        }

        [Test]
        public void UnknownCommand_ReturnsErrorMessage()
        {
            var result = _parser.Parse("hax0r");
            Assert.IsFalse(result.Success);
            StringAssert.Contains("Unknown command", result.Message);
            StringAssert.Contains("hax0r", result.Message);
        }

        [Test]
        public void KnownCommand_IsDispatched()
        {
            bool executed = false;
            _parser.Register("ping", new LambdaCommand(_ =>
            {
                executed = true;
                return CommandResult.Ok("pong");
            }));

            _parser.Parse("ping");
            Assert.IsTrue(executed);
        }

        [Test]
        public void Args_AreTokenisedCorrectly()
        {
            string[] capturedArgs = null;
            _parser.Register("echo", new LambdaCommand(args =>
            {
                capturedArgs = args;
                return CommandResult.Ok("");
            }));

            _parser.Parse("echo hello world");
            Assert.AreEqual(new[] { "hello", "world" }, capturedArgs);
        }

        [Test]
        public void Command_IsCaseInsensitive()
        {
            bool executed = false;
            _parser.Register("ping", new LambdaCommand(_ =>
            {
                executed = true;
                return CommandResult.Ok("pong");
            }));

            _parser.Parse("PING");
            Assert.IsTrue(executed);
        }

        [Test]
        public void EmptyInput_ReturnsError()
        {
            var result = _parser.Parse("");
            Assert.IsFalse(result.Success);
        }

        // Helper — inline ICommand for tests only.
        private class LambdaCommand : ICommand
        {
            private readonly System.Func<string[], CommandResult> _func;
            public LambdaCommand(System.Func<string[], CommandResult> func) => _func = func;
            public CommandResult Execute(string[] args) => _func(args);
        }
    }
}
