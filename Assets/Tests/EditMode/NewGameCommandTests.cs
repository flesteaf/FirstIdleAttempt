using NUnit.Framework;
using HackYourWay.Interfaces;
using HackYourWay.Services;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    public class NewGameCommandTests
    {
        private ConfirmationService _confirmService;
        private bool                _newGameCalled;

        [SetUp]
        public void SetUp()
        {
            _confirmService = new ConfirmationService();
            _newGameCalled  = false;
        }

        private NewGameCommand MakeCommand() =>
            new NewGameCommand(_confirmService, () => _newGameCalled = true);

        [Test]
        public void Execute_AlwaysReturnsConfirmPrompt()
        {
            CommandResult result = MakeCommand().Execute(System.Array.Empty<string>());
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Unsaved progress will be lost. Continue? (y/n)", result.Message);
            Assert.IsTrue(_confirmService.IsPending);
        }

        [Test]
        public void Execute_OnConfirm_CallsNewGameCallback()
        {
            MakeCommand().Execute(System.Array.Empty<string>());
            _confirmService.Resolve(true);
            Assert.IsTrue(_newGameCalled);
        }

        [Test]
        public void Execute_OnCancel_DoesNotCallNewGameCallback()
        {
            MakeCommand().Execute(System.Array.Empty<string>());
            _confirmService.Resolve(false);
            Assert.IsFalse(_newGameCalled);
        }

        [Test]
        public void Execute_WithExtraArgs_StillReturnsConfirmPrompt()
        {
            CommandResult result = MakeCommand().Execute(new[] { "foo" });
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Unsaved progress will be lost. Continue? (y/n)", result.Message);
        }

        [Test]
        public void Execute_PendingPromptMatchesReturnedMessage()
        {
            CommandResult result = MakeCommand().Execute(System.Array.Empty<string>());
            Assert.AreEqual(result.Message, _confirmService.PendingPrompt);
        }
    }
}
