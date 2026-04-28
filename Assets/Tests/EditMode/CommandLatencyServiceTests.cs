using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;

namespace HackYourWay.Tests.EditMode
{
    public class CommandLatencyServiceTests
    {
        private static Player BasePlayer() => new Player { CpuTier = 1, BandwidthTier = 1, GpuTier = 0 };
        private static Device DeviceWithBw(int bwTier, int cpuTier = 1) => new Device { BandwidthTier = bwTier, CpuTier = cpuTier };

        // (a) base player returns each command's named base time

        [Test]
        public void CalculateLatency_BaseCrackWep_ReturnsBaseTime()
        {
            var svc = new CommandLatencyService(BasePlayer());
            float result = svc.CalculateLatency(new CommandLatencyContext("crack", SecurityLevel.WEP));
            Assert.AreEqual(3.0f, result, 0.0001f);
        }

        [Test]
        public void CalculateLatency_BaseCrackWpa_ReturnsBaseTime()
        {
            var svc = new CommandLatencyService(BasePlayer());
            float result = svc.CalculateLatency(new CommandLatencyContext("crack", SecurityLevel.WPA));
            Assert.AreEqual(6.0f, result, 0.0001f);
        }

        [Test]
        public void CalculateLatency_BaseCrackWpa2_ReturnsBaseTime()
        {
            var svc = new CommandLatencyService(BasePlayer());
            float result = svc.CalculateLatency(new CommandLatencyContext("crack", SecurityLevel.WPA2));
            Assert.AreEqual(10.0f, result, 0.0001f);
        }

        [Test]
        public void CalculateLatency_BaseAreaScan_ReturnsBaseTime()
        {
            var svc = new CommandLatencyService(BasePlayer());
            float result = svc.CalculateLatency(new CommandLatencyContext("scan"));
            Assert.AreEqual(1.5f, result, 0.0001f);
        }

        [Test]
        public void CalculateLatency_BaseInject_ReturnsBaseTime()
        {
            var svc = new CommandLatencyService(BasePlayer());
            float result = svc.CalculateLatency(new CommandLatencyContext("inject", target: DeviceWithBw(1)));
            Assert.AreEqual(4.0f, result, 0.0001f);
        }

        [Test]
        public void CalculateLatency_BaseLs_ReturnsBaseTime()
        {
            var svc = new CommandLatencyService(BasePlayer());
            float result = svc.CalculateLatency(new CommandLatencyContext("ls", target: DeviceWithBw(1)));
            Assert.AreEqual(0.5f, result, 0.0001f);
        }

        [Test]
        public void CalculateLatency_BaseCopy_ReturnsBaseTime()
        {
            var svc = new CommandLatencyService(BasePlayer());
            float result = svc.CalculateLatency(new CommandLatencyContext("copy", target: DeviceWithBw(1), fileSize: 0));
            Assert.AreEqual(3.0f, result, 0.0001f);
        }

        // (b) effective time never drops below MinFloor

        [Test]
        public void CalculateLatency_AllMaxTiers_NeverBelowFloor()
        {
            var player  = new Player { CpuTier = 5, BandwidthTier = 5, GpuTier = 3 };
            var svc     = new CommandLatencyService(player);
            var target  = DeviceWithBw(5, 5);

            string[] verbs = { "crack", "scan", "inject", "firewall", "ls", "copy" };
            foreach (string verb in verbs)
            {
                var ctx    = new CommandLatencyContext(verb, SecurityLevel.WPA2, target, 0);
                float lat  = svc.CalculateLatency(ctx);
                Assert.GreaterOrEqual(lat, CommandLatencyService.MinFloor, $"Verb '{verb}' produced latency below MinFloor");
            }
        }

        // (c) CPU5 + GPU3 crack = base / 20

        [Test]
        public void CalculateLatency_Cpu5Gpu3_CrackWpa2_Is_BaseDiv20()
        {
            var player = new Player { CpuTier = 5, BandwidthTier = 1, GpuTier = 3 };
            var svc    = new CommandLatencyService(player);
            float result = svc.CalculateLatency(new CommandLatencyContext("crack", SecurityLevel.WPA2));
            Assert.AreEqual(10.0f / 20.0f, result, 0.0001f);
        }

        // (d) player BW is the bottleneck — inject player-BW5 vs target-BW1 == player-BW1 vs target-BW1

        [Test]
        public void CalculateLatency_Inject_PlayerBw5TargetBw1_EqualsPlayerBw1TargetBw1()
        {
            float slow = new CommandLatencyService(new Player { CpuTier = 1, BandwidthTier = 1, GpuTier = 0 })
                .CalculateLatency(new CommandLatencyContext("inject", target: DeviceWithBw(1)));

            float bottlenecked = new CommandLatencyService(new Player { CpuTier = 1, BandwidthTier = 5, GpuTier = 0 })
                .CalculateLatency(new CommandLatencyContext("inject", target: DeviceWithBw(1)));

            Assert.AreEqual(slow, bottlenecked, 0.0001f);
        }

        // (e) inject player-BW5 vs target-BW5 is faster than vs target-BW1

        [Test]
        public void CalculateLatency_Inject_PlayerBw5TargetBw5_FasterThanTargetBw1()
        {
            var player = new Player { CpuTier = 1, BandwidthTier = 5, GpuTier = 0 };
            var svc    = new CommandLatencyService(player);

            float fast = svc.CalculateLatency(new CommandLatencyContext("inject", target: DeviceWithBw(5)));
            float slow = svc.CalculateLatency(new CommandLatencyContext("inject", target: DeviceWithBw(1)));

            Assert.Less(fast, slow);
        }

        // (f) target CPU tier does NOT affect inject time — only bandwidth matters

        [Test]
        public void CalculateLatency_Inject_TargetCpu1VsCpu5_SameTimeGivenIdenticalBw()
        {
            var player = new Player { CpuTier = 1, BandwidthTier = 3, GpuTier = 0 };
            var svc    = new CommandLatencyService(player);

            float withCpu1 = svc.CalculateLatency(new CommandLatencyContext("inject", target: DeviceWithBw(3, cpuTier: 1)));
            float withCpu5 = svc.CalculateLatency(new CommandLatencyContext("inject", target: DeviceWithBw(3, cpuTier: 5)));

            Assert.AreEqual(withCpu1, withCpu5, 0.0001f);
        }

        // (g) copy scales with file size

        [Test]
        public void CalculateLatency_Copy_ScalesWithFileSize()
        {
            var svc = new CommandLatencyService(BasePlayer());

            float small = svc.CalculateLatency(new CommandLatencyContext("copy", target: DeviceWithBw(1), fileSize: 0));
            float large = svc.CalculateLatency(new CommandLatencyContext("copy", target: DeviceWithBw(1), fileSize: 10_000_000L));

            Assert.Less(small, large);
        }

        [Test]
        public void CalculateLatency_Copy_ReferenceSize_IsThreeTimesBase()
        {
            var svc    = new CommandLatencyService(BasePlayer());
            // fileSize = CopyReferenceSizeBytes → scale = 1 + 1 * 2 = 3 → adjusted = 3 * 3 = 9
            float result = svc.CalculateLatency(new CommandLatencyContext("copy", target: DeviceWithBw(1), fileSize: 10_000_000L));
            Assert.AreEqual(9.0f, result, 0.0001f);
        }
    }
}
