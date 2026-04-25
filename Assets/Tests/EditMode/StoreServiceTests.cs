using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;

namespace HackYourWay.Tests.EditMode
{
    public class StoreServiceTests
    {
        private static Player MakePlayer(double btcBalance = 10.0)
        {
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, btcBalance);
            return player;
        }

        private static StoreItem MakeSoftwareItem(double price = 1.0, ToolType tool = ToolType.CrackWEP)
            => new StoreItem
            {
                Id              = "tool_crack_wep",
                DisplayName     = "WEP Cracker",
                Category        = StoreItemCategory.Software,
                Price           = price,
                PriceCurrency   = CurrencyType.Bitcoin,
                HasToolUnlock   = true,
                UnlocksToolType = tool,
                IncomeMultiplier = 1.0
            };

        private static StoreItem MakeComponentItem(double price = 2.0, double multiplier = 1.5)
            => new StoreItem
            {
                Id               = "pc_upgrade",
                DisplayName      = "RAM Upgrade",
                Category         = StoreItemCategory.PCComponent,
                Price            = price,
                PriceCurrency    = CurrencyType.Bitcoin,
                IncomeMultiplier = multiplier
            };

        private static StoreItem MakeCurrencyUnlockItem(double price = 5.0)
            => new StoreItem
            {
                Id                = "unlock_monero",
                DisplayName       = "Monero Miner",
                Category          = StoreItemCategory.Software,
                Price             = price,
                PriceCurrency     = CurrencyType.Bitcoin,
                HasCurrencyUnlock = true,
                UnlocksCurrency   = CurrencyType.Monero,
                IncomeMultiplier  = 1.0
            };

        // ── Purchase tests ────────────────────────────────────────────────────

        [Test]
        public void Purchase_AffordableItem_DeductsBalanceAndMarksPurchased()
        {
            var player  = MakePlayer(btcBalance: 5.0);
            var item    = MakeSoftwareItem(price: 2.0);
            var service = new StoreService(player);

            var result = service.Purchase(item);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3.0, player.GetBalance(CurrencyType.Bitcoin), delta: 0.0001);
            Assert.Contains(item.Id, player.PurchasedItems);
        }

        [Test]
        public void Purchase_UnaffordableItem_ReturnsInsufficientFundsError()
        {
            var player  = MakePlayer(btcBalance: 0.5);
            var item    = MakeSoftwareItem(price: 2.0);
            var service = new StoreService(player);

            var result = service.Purchase(item);

            Assert.IsFalse(result.Success);
            StringAssert.Contains("insufficient", result.Message.ToLowerInvariant());
        }

        [Test]
        public void Purchase_AlreadyOwned_ReturnsError()
        {
            var player  = MakePlayer(btcBalance: 10.0);
            var item    = MakeSoftwareItem();
            var service = new StoreService(player);

            service.Purchase(item); // first buy
            var result = service.Purchase(item); // second buy

            Assert.IsFalse(result.Success);
            StringAssert.Contains("already", result.Message.ToLowerInvariant());
        }

        // ── Effect application ────────────────────────────────────────────────

        [Test]
        public void Purchase_SoftwareItem_AddsToolToUnlockedTools()
        {
            var player  = MakePlayer();
            var item    = MakeSoftwareItem(tool: ToolType.CrackWPA2);
            var service = new StoreService(player);

            service.Purchase(item);

            Assert.IsTrue(player.HasTool(ToolType.CrackWPA2));
        }

        [Test]
        public void Purchase_ComponentItem_AppliesIncomeMultiplier()
        {
            var player  = MakePlayer();
            var item    = MakeComponentItem(multiplier: 2.0);
            var service = new StoreService(player);

            service.Purchase(item);

            Assert.AreEqual(2.0, service.CurrentIncomeMultiplier, delta: 0.0001);
        }

        [Test]
        public void Purchase_CurrencyUnlockItem_AddsCurrencyToUnlocked()
        {
            var player  = MakePlayer();
            var item    = MakeCurrencyUnlockItem();
            var service = new StoreService(player);

            service.Purchase(item);

            Assert.IsTrue(player.HasCurrency(CurrencyType.Monero));
        }

        // ── Hardware tier tests ───────────────────────────────────────────────

        private static StoreItem MakeHardwareItem(HardwareStat stat, int tier, double price = 1.0)
            => new StoreItem
            {
                Id                   = $"hw_{stat}_{tier}",
                DisplayName          = $"{stat} Tier {tier}",
                Category             = StoreItemCategory.PCComponent,
                Price                = price,
                PriceCurrency        = CurrencyType.Bitcoin,
                IncomeMultiplier     = 1.0,
                HasHardwareUpgrade   = true,
                HardwareStatAffected = stat,
                HardwareTierGranted  = tier
            };

        // T022 — CPU tier

        [Test]
        public void Purchase_CpuTier2_SetsCpuTierTo2()
        {
            var player  = MakePlayer();
            var item    = MakeHardwareItem(HardwareStat.CPU, 2);
            var service = new StoreService(player);

            service.Purchase(item);

            Assert.AreEqual(2, player.CpuTier);
        }

        [Test]
        public void Purchase_CpuLowerThanCurrent_TierUnchanged()
        {
            var player  = MakePlayer();
            player.CpuTier = 4;
            var item    = MakeHardwareItem(HardwareStat.CPU, 2, price: 0.01);
            var service = new StoreService(player);

            service.Purchase(item);

            Assert.AreEqual(4, player.CpuTier);
        }

        // T024 — Bandwidth tier

        [Test]
        public void Purchase_BandwidthTier2_SetsBandwidthTierTo2()
        {
            var player  = MakePlayer();
            var item    = MakeHardwareItem(HardwareStat.Bandwidth, 2);
            var service = new StoreService(player);

            service.Purchase(item);

            Assert.AreEqual(2, player.BandwidthTier);
        }

        [Test]
        public void Purchase_BandwidthLowerThanCurrent_TierUnchanged()
        {
            var player  = MakePlayer();
            player.BandwidthTier = 3;
            var item    = MakeHardwareItem(HardwareStat.Bandwidth, 2, price: 0.01);
            var service = new StoreService(player);

            service.Purchase(item);

            Assert.AreEqual(3, player.BandwidthTier);
        }

        // T026 — GPU tier

        [Test]
        public void Purchase_GpuTier1_SetsGpuTierTo1()
        {
            var player  = MakePlayer();
            var item    = MakeHardwareItem(HardwareStat.GPU, 1);
            var service = new StoreService(player);

            service.Purchase(item);

            Assert.AreEqual(1, player.GpuTier);
        }

        [Test]
        public void Purchase_GpuLowerThanCurrent_TierUnchanged()
        {
            var player  = MakePlayer();
            player.GpuTier = 2;
            var item    = MakeHardwareItem(HardwareStat.GPU, 1, price: 0.01);
            var service = new StoreService(player);

            service.Purchase(item);

            Assert.AreEqual(2, player.GpuTier);
        }
    }
}
