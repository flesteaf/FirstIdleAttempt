using Assets.Scripts.Computers.CPUs;
using Assets.Scripts.Computers.GPUs;
using Assets.Scripts.Computers.HARDs;
using Assets.Scripts.Computers.Motherboards;
using Assets.Scripts.Computers.Networks;
using Assets.Scripts.Computers.Rams;
using Assets.Scripts.Computers.Sources;
using Assets.Scripts.Computers.Wirelesses;
using Assets.Scripts.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Computers
{
    internal abstract class Computer
    {
        public abstract List<Ram> Rams { get; protected set; }
        public abstract List<Hard> Hards { get; protected set; }
        public abstract List<Cpu> Cpus { get; protected set; }
        public abstract List<Gpu> Gpus { get; protected set; }
        public abstract List<Network> Networks { get; protected set; }
        public abstract List<Wireless> Wirelesses { get; protected set; }
        public abstract List<Source> Sources { get; protected set; }
        public abstract Motherboard Motherboard { get; protected set; }

        internal abstract string Name { get; }

        public Computer()
        {
            SetupComputer();
        }

        protected abstract void SetupComputer();

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Computer type: {Name}");
            builder.AppendLine("Current configuration:");
            builder.AppendLine($"   CPU: {Cpus.ToStatus()}");
            builder.AppendLine($"   Storage: {Hards.ToStatus()}");
            builder.AppendLine($"   RAM: {Rams.ToStatus()}");
            builder.AppendLine($"   GPU: {Gpus.ToStatus()}");
            builder.AppendLine($"   Network: {Networks.ToStatus()}");
            builder.AppendLine($"   Wireless: {Wirelesses.ToStatus()}");
            builder.AppendLine($"   Sources: {Sources.ToStatus()}");
            builder.AppendLine($"   Motherboard: {Motherboard}");

            string motherboardLoad = GetMotherboardLoadString();
            builder.AppendLine($"Motherboard configuration: {motherboardLoad}");

            int currentLoad = GetSourceLoad();
            int maximumLoad = MaximumSourceLoad();
            float percentageLoad = (float)currentLoad / maximumLoad;
            builder.AppendLine($"Current source load: {currentLoad}/{maximumLoad} - {percentageLoad:00.##}");

            return builder.ToString();
        }

        private string GetMotherboardLoadString()
        {
            List<ComponentLoad> load = Motherboard.GetLoad(this);
            StringBuilder builder = new StringBuilder();
            foreach (var item in load)
            {
                builder.Append($"{item.Name}: {item.CurrentNumber}/{item.MaximumNumber}|");
            }
            builder.Append("\b");
            return builder.ToString();
        }

        private int MaximumSourceLoad()
        {
            return Sources.Select(x => x.ProvidedLoad).Sum();
        }

        private int GetSourceLoad()
        {
            return Rams.Select(x => x.LoadUsage).Sum() +
                   Hards.Select(x => x.LoadUsage).Sum() +
                   Cpus.Select(x => x.LoadUsage).Sum() +
                   Gpus.Select(x => x.LoadUsage).Sum() +
                   Networks.Select(x => x.LoadUsage).Sum() +
                   Wirelesses.Select(x => x.LoadUsage).Sum() +
                   Motherboard.LoadUsage;
        }
    }
}