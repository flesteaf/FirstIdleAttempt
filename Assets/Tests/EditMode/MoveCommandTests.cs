using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;
using HackYourWay.Data;

namespace HackYourWay.Tests.EditMode
{
    public class MoveCommandTests
    {
        private static Services.LocationService BuildLocationService()
        {
            var config = new LocationConfigSO();
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

        [Test]
        public void Move_NoArgs_Succeeds()
        {
            var svc    = BuildLocationService();
            var result = new MoveCommand(svc).Execute(new string[0]);

            Assert.IsTrue(result.Success);
            StringAssert.Contains("Moved to", result.Message);
        }

        [Test]
        public void Move_NoArgs_ChangesLocation()
        {
            var svc = BuildLocationService();
            var cmd = new MoveCommand(svc);

            cmd.Execute(new string[0]);           // move to location 1
            string first = svc.GetCurrentLocationName();

            cmd.Execute(new string[0]);           // move to location 2
            string second = svc.GetCurrentLocationName();

            Assert.AreNotEqual(first, second);
        }

        [Test]
        public void Move_KnownName_Succeeds()
        {
            var svc = BuildLocationService();
            var cmd = new MoveCommand(svc);

            cmd.Execute(new string[0]); // move to "node_77" (seed 1)
            cmd.Execute(new string[0]); // move to "node_154" (seed 2)

            var result = cmd.Execute(new[] { "node_77" });

            Assert.IsTrue(result.Success);
            Assert.AreEqual("node_77", svc.GetCurrentLocationName());
        }

        [Test]
        public void Move_KnownName_MessageContainsName()
        {
            var svc = BuildLocationService();
            var cmd = new MoveCommand(svc);

            cmd.Execute(new string[0]);           // seed 1 → node_77
            cmd.Execute(new string[0]);           // seed 2 → node_154

            var result = cmd.Execute(new[] { "node_77" });
            StringAssert.Contains("node_77", result.Message);
        }

        [Test]
        public void Move_SameLocation_ReturnsAlreadyAt()
        {
            var svc = BuildLocationService();
            var cmd = new MoveCommand(svc);

            cmd.Execute(new string[0]); // move to node_77

            var result = cmd.Execute(new[] { "node_77" });

            Assert.IsTrue(result.Success);
            StringAssert.Contains("Already at", result.Message);
        }

        [Test]
        public void Move_UnknownName_ReturnsFail()
        {
            var svc    = BuildLocationService();
            var result = new MoveCommand(svc).Execute(new[] { "unknown_location" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("not found", result.Message);
        }

        [Test]
        public void Move_UnknownName_LocationUnchanged()
        {
            var svc = BuildLocationService();
            var cmd = new MoveCommand(svc);

            cmd.Execute(new string[0]); // move to node_77
            string before = svc.GetCurrentLocationName();
            cmd.Execute(new[] { "does_not_exist" });

            Assert.AreEqual(before, svc.GetCurrentLocationName());
        }
    }
}
