using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Data;

namespace HackYourWay.Tests.PlayMode
{
    /// <summary>
    /// Play-mode integration test: purchase bot software → verify contract available
    /// → accept DDOS contract → complete objectives → verify reward credited.
    /// </summary>
    public class StoreContractIntegrationTests
    {
        private static Services.LocationService BuildLocationService()
        {
            var config = ScriptableObject.CreateInstance<LocationConfigSO>();
            config.MinNetworks          = 1;
            config.MaxNetworks          = 1;
            config.MinDevicesPerNetwork = 1;
            config.MaxDevicesPerNetwork = 1;
            config.MinFilesPerDevice    = 0;
            config.MaxFilesPerDevice    = 0;
            config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f };
            config.MinRansomAmount      = 0.01f;
            config.MaxRansomAmount      = 0.10f;
            return new Services.LocationService(config);
        }

        [UnityTest]
        public IEnumerator PurchaseBotSoftware_UnlocksDdosContract()
        {
            // --- Arrange ---
            var player  = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 100.0);

            var botItem = new StoreItem
            {
                Id              = "software_bot",
                DisplayName     = "Bot Injection Software",
                Category        = StoreItemCategory.Software,
                Price           = 5.0,
                PriceCurrency   = CurrencyType.Bitcoin,
                HasToolUnlock   = true,
                UnlocksToolType = ToolType.BotSoftware,
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

            // Register bot type with ContractService (simulates inject bot command).
            contractService.RegisterActiveMalware(MalwareType.Bot);

            // --- Assert: DDOS contract is now available ---
            bool available = contractService.IsAvailable(ddosContract);
            Assert.IsTrue(available, "DDOS contract should be available after bot injection.");

            // --- Accept contract ---
            var acceptResult = contractService.Accept(ddosContract);
            Assert.IsTrue(acceptResult.Success, "Should be able to accept the DDOS contract.");
            Assert.IsTrue(player.ActiveContracts.Contains(ddosContract));

            // --- Complete objective and finish contract ---
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

            yield return null;
        }
    }
}
