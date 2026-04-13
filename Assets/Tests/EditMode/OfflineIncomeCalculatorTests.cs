using System.Collections.Generic;
using NUnit.Framework;
using HackYourWay.Core;
using HackYourWay.Models;

namespace HackYourWay.Tests.EditMode
{
    public class OfflineIncomeCalculatorTests
    {
        private const double Tolerance = 0.0001;

        [Test]
        public void ZeroElapsedTime_YieldsZeroIncome()
        {
            long now = System.DateTime.UtcNow.Ticks;
            var malware = new List<Malware>
            {
                new Malware { Type = MalwareType.Miner, IncomeRate = 1.0, Currency = CurrencyType.Bitcoin,
                              InstalledAtUtcTicks = now }
            };
            long lastSave = now;

            var results = OfflineIncomeCalculator.Calculate(lastSave, malware, now);

            Assert.AreEqual(0, GetAmount(results, CurrencyType.Bitcoin), Tolerance);
        }

        [Test]
        public void SixtySecondsAtOnePerSecond_YieldsSixty()
        {
            long baselineTicks = System.DateTime.UtcNow.Ticks - System.TimeSpan.TicksPerSecond * 60;
            var malware = new List<Malware>
            {
                new Malware { Type = MalwareType.Miner, IncomeRate = 1.0, Currency = CurrencyType.Bitcoin,
                              InstalledAtUtcTicks = baselineTicks }
            };
            long nowTicks = baselineTicks + System.TimeSpan.TicksPerSecond * 60;

            var results = OfflineIncomeCalculator.Calculate(baselineTicks, malware, nowTicks);

            Assert.AreEqual(60.0, GetAmount(results, CurrencyType.Bitcoin), Tolerance);
        }

        [Test]
        public void MultipleMiners_AccumulateCorrectly()
        {
            long baseline = 0L;
            long nowTicks = System.TimeSpan.TicksPerSecond * 10;
            var malware = new List<Malware>
            {
                new Malware { IncomeRate = 1.0, Currency = CurrencyType.Bitcoin, InstalledAtUtcTicks = baseline },
                new Malware { IncomeRate = 2.0, Currency = CurrencyType.Bitcoin, InstalledAtUtcTicks = baseline }
            };

            var results = OfflineIncomeCalculator.Calculate(baseline, malware, nowTicks);

            Assert.AreEqual(30.0, GetAmount(results, CurrencyType.Bitcoin), Tolerance);
        }

        [Test]
        public void NoActiveMalware_YieldsZero()
        {
            long baseline = 0L;
            long nowTicks = System.TimeSpan.TicksPerSecond * 100;

            var results = OfflineIncomeCalculator.Calculate(baseline, new List<Malware>(), nowTicks);

            Assert.AreEqual(0, GetAmount(results, CurrencyType.Bitcoin), Tolerance);
        }

        [Test]
        public void MalwareInstalledAfterLastSave_UsesInstallTime()
        {
            // lastSave = 0, malware installed at t=5s, now = t=10s → should earn 5s, not 10s
            long lastSave   = 0L;
            long installAt  = System.TimeSpan.TicksPerSecond * 5;
            long nowTicks   = System.TimeSpan.TicksPerSecond * 10;
            var malware = new List<Malware>
            {
                new Malware { IncomeRate = 1.0, Currency = CurrencyType.Bitcoin, InstalledAtUtcTicks = installAt }
            };

            var results = OfflineIncomeCalculator.Calculate(lastSave, malware, nowTicks);

            Assert.AreEqual(5.0, GetAmount(results, CurrencyType.Bitcoin), Tolerance);
        }

        private static double GetAmount(List<CurrencyBalance> balances, CurrencyType currency)
        {
            for (int i = 0; i < balances.Count; i++)
                if (balances[i].Currency == currency) return balances[i].Amount;
            return 0;
        }
    }
}
