using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Data;

namespace HackYourWay.Tests.PlayMode
{
    /// <summary>
    /// Integration test: purchase bot software → verify contract available
    /// → accept DDOS contract → complete objectives → verify reward credited.
    /// Runs within GDUnit4 (Godot runtime required for LocationConfigSO instantiation).
    /// </summary>
    public class StoreContractIntegrationTests
    {
        private static Services.LocationService BuildLocationService()
        {
            var config = new LocationConfigSO
            {
                MinNetworks          = 1,
                MaxNetworks          = 1,
                MinDevicesPerNetwork = 1,
                MaxDevicesPerNetwork = 1,
                MinFilesPerDevice    = 0,
                MaxFilesPerDevice    = 0,
                SecurityDistribution = new float[] { 0f, 0f, 0f, 1f },
                MinRansomAmount      = 0.01f,
                MaxRansomAmount      = 0.10f,
            };
            return new Services.LocationService(config);
        }

        [Test]
        public void PurchaseBotSoftware_UnlocksDdosContract()
        {
            // --- Arrange ---
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 100.0);

            var botItem = new StoreItem
            {
                Id               = "software_bot",
                DisplayName      = "Bot Injection Software",
                Category         = StoreItemCategory.Software,
                Price            = 5.0,
                PriceCurrency    = CurrencyType.Bitcoin,
                HasToolUnlock    = true,
                UnlocksToolType  = ToolType.BotSoftware,
                IncomeMultiplier = 1.0
            };

            var storeService    = new StoreService(player);
            var contractService = new ContractService(player);

            var ddosContract = new Contract
            {
                Id                    = "ddos_01",
                Type                  = ContractType.DDOS,
                Description           = "Take down a target server.",
                HasMalwareRequirement = true,
                RequiredMalware       = MalwareType.Bot,
                Reward                = 2.0,
                RewardCurrency        = CurrencyType.Bitcoin,
                Objectives            = new System.Collections.Generic.List<Objective>
                {
                    new Objective { Description = "Infect device with bot." }
                }
            };

            // --- Act: Purchase bot software ---
            var purchaseResult = storeService.Purchase(botItem);
            Assert.IsTrue(purchaseResult.Success, "Bot software purchase should succeed.");
            Assert.IsTrue(player.HasTool(ToolType.BotSoftware), "Player should own BotSoftware tool.");

            contractService.RegisterActiveMalware(MalwareType.Bot);

            // --- Assert: DDOS contract available ---
            bool available = contractService.IsAvailable(ddosContract);
            Assert.IsTrue(available, "DDOS contract should be available after bot injection.");

            // --- Accept contract ---
            var acceptResult = contractService.Accept(ddosContract);
            Assert.IsTrue(acceptResult.Success, "Should be able to accept the DDOS contract.");
            Assert.IsTrue(player.ActiveContracts.Contains(ddosContract));

            // --- Complete and collect reward ---
            ddosContract.Objectives[0].IsComplete = true;
            double balanceBefore = player.GetBalance(CurrencyType.Bitcoin);

            var completeResult = contractService.TryComplete(ddosContract);
            Assert.IsTrue(completeResult.Success, "Contract should complete successfully.");
            Assert.IsTrue(ddosContract.IsCompleted);
            Assert.AreEqual(
                balanceBefore + ddosContract.Reward,
                player.GetBalance(CurrencyType.Bitcoin),
                delta: 0.0001,
                "Reward should be credited after contract completion.");
        }
    }
}
