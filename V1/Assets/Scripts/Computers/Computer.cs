using Assets.Scripts.Computers.Motherboards;
using Assets.Scripts.Extensions;
using Assets.Scripts.Softwares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Computers
{
    public class Computer
    {
        private List<Ram> rams;
        private List<Hard> hards;
        private List<Cpu> cpus;
        private List<Gpu> gpus;
        private List<NetworkBoard> networks;
        private List<Source> sources;
        private Motherboard motherboard;
        private SpaceManagement spaceManagement;
        private string name;

        #region PublicProperties

        public List<Ram> Rams
        {
            get => rams;
            set
            {
                if (rams == null)
                    rams = value;
            }
        }
        public List<Hard> Hards
        {
            get => hards;
            set
            {
                if (hards == null)
                    hards = value;
            }
        }
        public List<Cpu> Cpus
        {
            get => cpus;
            set
            {
                if (cpus == null)
                    cpus = value;
            }
        }
        public List<Gpu> Gpus
        {
            get => gpus;
            set
            {
                if (gpus == null)
                    gpus = value;
            }
        }
        public List<NetworkBoard> Networks
        {
            get => networks;
            set
            {
                if (networks == null)
                    networks = value;
            }
        }
        public List<Source> Sources
        {
            get => sources;
            set
            {
                if (sources == null)
                    sources = value;
            }
        }
        public Motherboard Motherboard
        {
            get => motherboard;
            set
            {
                if (motherboard == null)
                    motherboard = value;
            }
        }
        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrEmpty(name))
                    name = value;
            }
        }

        #endregion PublicProperties

        public Computer()
        {
            SetupComputer();
            hards.Sort();
            spaceManagement = new SpaceManagement(hards);
        }

        protected virtual void SetupComputer()
        {
            Rams = new List<Ram>();
            Hards = new List<Hard>();
            Cpus = new List<Cpu>();
            Gpus = new List<Gpu>();
            Networks = new List<NetworkBoard>();
            Sources = new List<Source>();
            Motherboard = null;
        }

        #region ToString()

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
                   Motherboard.LoadUsage;
        }

        #endregion ToString()

        #region ComponentUpdates

        internal bool UpdateComponent(ComputerComponent component, out string message)
        {
            message = string.Empty;
            if (component is Cpu cpu)
            {
                UpdateCpuComponent(cpu);
                return true;
            }

            if (component is Gpu gpu)
            {
                UpdateGpuComponent(gpu);
                return true;
            }

            if (component is Ram ram)
            {
                UpdateRamComponent(ram);
                return true;
            }

            if (component is NetworkBoard network)
            {
                UpdateNetworkComponent(network);
                return true;
            }

            if (component is Hard hard)
            {
                return UpdateHardComponent(hard, out message);

            }

            if (component is Source source)
            {
                UpdateSourceComponent(source);
                return true;
            }

            if (component is Motherboard motherboard)
            {
                bool result = UpdateMotherboardComponent(motherboard);
                if (!result)
                {
                    message = "Cannot upgrade motherboard, provided component is weaker than the current one";
                }
                return result;
            }

            message = "Unidentified component! try again";
            return false;
        }

        internal void UpdateCpuComponent(Cpu cpu)
        {
            if (Motherboard.AllowedCpus == Cpus.Count)
            {
                Cpus.RemoveAt(0);
            }

            Cpus.Add(cpu);
        }

        internal void UpdateGpuComponent(Gpu gpu)
        {
            if (Motherboard.AllowedGpus == Gpus.Count)
            {
                Gpus.RemoveAt(0);
            }

            Gpus.Add(gpu);
        }

        internal bool UpdateHardComponent(Hard hard, out string message)
        {
            bool hardRemoved = false;
            message = string.Empty;
            if (Motherboard.AllowedHards == Hards.Count)
            {
                if (hards[0] > hard)
                {
                    message = "You have reached the maximum ammount of disks and the disk you want to add cannot replace a bigger hard";
                }

                Hards.RemoveAt(0);
                hardRemoved = true;
            }

            Hards.Add(hard);
            hards.Sort();
            spaceManagement.UpdateStorage(hard, hardRemoved);
            return true;
        }

        public bool StoreSoftware(Software software)
        {
            return spaceManagement.TryStoreSoftware(software);
        }

        internal void UpdateNetworkComponent(NetworkBoard network)
        {
            if (Motherboard.AllowedNetworks == Networks.Count)
            {
                Networks.RemoveAt(0);
            }

            Networks.Add(network);
        }

        internal void UpdateRamComponent(Ram ram)
        {
            if (Motherboard.AllowedRams == Rams.Count)
            {
                Rams.RemoveAt(0);
            }

            Rams.Add(ram);
        }

        internal void UpdateSourceComponent(Source source)
        {
            Sources.Add(source);
        }

        internal bool UpdateMotherboardComponent(Motherboard motherboard)
        {
            if (motherboard < Motherboard)
            {
                return false;
            }

            Motherboard = motherboard;
            return true;
        }

        internal int GetExecutionSpeed()
        {
            return cpus.Sum(c => c.TotalSpeed());
        }

        internal long GetNetworkSpeed()
        {
            return networks.Sum(n => n.TotalSpeed());
        }

        #endregion ComponentUpdates
    }
}