using NUnit.Framework;
using HackYourWay.Services;

namespace HackYourWay.Tests.EditMode
{
    public class ConfirmationServiceTests
    {
        private ConfirmationService _svc;

        [SetUp]
        public void SetUp() => _svc = new ConfirmationService();

        [Test]
        public void IsPending_StartsAsFalse()
        {
            Assert.IsFalse(_svc.IsPending);
        }

        [Test]
        public void PendingPrompt_StartsAsNull()
        {
            Assert.IsNull(_svc.PendingPrompt);
        }

        [Test]
        public void RequestConfirmation_SetsPendingTrue()
        {
            _svc.RequestConfirmation("Prompt (y/n)", () => {}, () => {});
            Assert.IsTrue(_svc.IsPending);
        }

        [Test]
        public void RequestConfirmation_StoresPendingPrompt()
        {
            _svc.RequestConfirmation("Test prompt (y/n)", () => {}, () => {});
            Assert.AreEqual("Test prompt (y/n)", _svc.PendingPrompt);
        }

        [Test]
        public void Resolve_True_FiresOnConfirm_ClearsPending()
        {
            bool confirmed = false;
            _svc.RequestConfirmation("Prompt (y/n)", () => confirmed = true, () => {});
            _svc.Resolve(true);
            Assert.IsTrue(confirmed);
            Assert.IsFalse(_svc.IsPending);
        }

        [Test]
        public void Resolve_False_FiresOnCancel_ClearsPending()
        {
            bool cancelled = false;
            _svc.RequestConfirmation("Prompt (y/n)", () => {}, () => cancelled = true);
            _svc.Resolve(false);
            Assert.IsTrue(cancelled);
            Assert.IsFalse(_svc.IsPending);
        }

        [Test]
        public void Cancel_ClearsPendingWithoutFiringCallbacks()
        {
            bool confirmFired = false;
            bool cancelFired  = false;
            _svc.RequestConfirmation("Prompt (y/n)", () => confirmFired = true, () => cancelFired = true);
            _svc.Cancel();
            Assert.IsFalse(_svc.IsPending);
            Assert.IsFalse(confirmFired);
            Assert.IsFalse(cancelFired);
        }

        [Test]
        public void RequestConfirmation_WhenAlreadyPending_ReplacesPrevious_WithoutFiringCancel()
        {
            bool firstCancelFired   = false;
            bool secondConfirmFired = false;
            _svc.RequestConfirmation("First (y/n)",  () => {},                     () => firstCancelFired = true);
            _svc.RequestConfirmation("Second (y/n)", () => secondConfirmFired = true, () => {});

            Assert.IsFalse(firstCancelFired, "Replaced confirmation must not fire onCancel");
            Assert.AreEqual("Second (y/n)", _svc.PendingPrompt);
            _svc.Resolve(true);
            Assert.IsTrue(secondConfirmFired);
        }
    }
}
