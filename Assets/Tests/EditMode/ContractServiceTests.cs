using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;

namespace HackYourWay.Tests.EditMode
{
    public class ContractServiceTests
    {
        private static Contract MakeBotContract() => new Contract
        {
            Id                   = "ddos_01",
            Type                 = ContractType.DDOS,
            Description          = "Take down a target server.",
            HasMalwareRequirement = true,
            RequiredMalware      = MalwareType.Bot,
            Reward               = 0.5,
            RewardCurrency       = CurrencyType.Bitcoin,
            Objectives           = new System.Collections.Generic.List<Objective>
            {
                new Objective { Description = "Install bot on one device." }
            }
        };

        private static Contract MakeOpenContract() => new Contract
        {
            Id                   = "file_01",
            Type                 = ContractType.FileRetrieval,
            Description          = "Retrieve a file.",
            HasMalwareRequirement = false,
            Reward               = 0.2,
            RewardCurrency       = CurrencyType.Bitcoin,
            Objectives           = new System.Collections.Generic.List<Objective>
            {
                new Objective { Description = "Copy the file." }
            }
        };

        // ── Availability ──────────────────────────────────────────────────────

        [Test]
        public void Contracts_WithoutBot_BotContractUnavailable()
        {
            var player  = new Player(); // no bot
            var service = new ContractService(player);

            bool available = service.IsAvailable(MakeBotContract());

            Assert.IsFalse(available);
        }

        [Test]
        public void Contracts_WithBot_BotContractAvailable()
        {
            var player = new Player();
            // Simulate bot injection by adding a dummy malware record.
            player.ActiveContracts.Clear(); // ensure clean state
            var service = new ContractService(player);

            // Mark player as having an active bot (tracked via service helper).
            service.RegisterActiveMalware(MalwareType.Bot);

            bool available = service.IsAvailable(MakeBotContract());

            Assert.IsTrue(available);
        }

        // ── Accept / complete ─────────────────────────────────────────────────

        [Test]
        public void Accept_AddsContractToActiveContracts()
        {
            var player  = new Player();
            var service = new ContractService(player);
            var contract = MakeOpenContract();

            var result = service.Accept(contract);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(player.ActiveContracts.Contains(contract));
        }

        [Test]
        public void CompleteAllObjectives_MarksContractCompleteAndCreditsReward()
        {
            var player  = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);

            var service  = new ContractService(player);
            var contract = MakeOpenContract();

            service.Accept(contract);

            // Complete all objectives.
            contract.Objectives[0].IsComplete = true;
            var result = service.TryComplete(contract);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(contract.IsCompleted);
            Assert.IsFalse(player.ActiveContracts.Contains(contract));
            Assert.Contains(contract.Id, player.CompletedContracts);
            Assert.AreEqual(contract.Reward, player.GetBalance(CurrencyType.Bitcoin), delta: 0.0001);
        }

        [Test]
        public void TryComplete_IncompleteObjectives_ReturnsError()
        {
            var player  = new Player();
            var service  = new ContractService(player);
            var contract = MakeOpenContract();

            service.Accept(contract);
            // Objectives NOT completed.
            var result = service.TryComplete(contract);

            Assert.IsFalse(result.Success);
            Assert.IsFalse(contract.IsCompleted);
        }
    }
}
