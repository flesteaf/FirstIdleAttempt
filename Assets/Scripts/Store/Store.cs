using Assets.Scripts.Softwares;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Store
{
    public class Store : IStore
    {
        public List<Software> Softwares { get; set; }
        public List<CpuStore> CPUs { get; set; }
        public List<GpuStore> GPUs { get; set; }
        public List<HardStore> Hards { get; set; }
        public List<NetworkBoardStore> Networks { get; set; }
        public List<RamStore> RAMs { get; set; }
        public List<SourceStore> Sources { get; set; }
        public List<MotherboardStore> Motherboards { get; set; }

        public StoreComponent GetComponent(string componentName)
        {
            StoreComponent component;
            component = CPUs.Find(com => com.CPU.Name.Equals(componentName, StringComparison.InvariantCultureIgnoreCase));
            if (component != null) return component;

            component = GPUs.Find(com => com.GPU.Name.Equals(componentName, StringComparison.InvariantCultureIgnoreCase));
            if (component != null) return component;

            component = Hards.Find(com => com.Hard.Name.Equals(componentName, StringComparison.InvariantCultureIgnoreCase));
            if (component != null) return component;

            component = Networks.Find(com => com.Network.Name.Equals(componentName, StringComparison.InvariantCultureIgnoreCase));
            if (component != null) return component;

            component = RAMs.Find(com => com.RAM.Name.Equals(componentName, StringComparison.InvariantCultureIgnoreCase));
            if (component != null) return component;

            component = Sources.Find(com => com.Source.Name.Equals(componentName, StringComparison.InvariantCultureIgnoreCase));
            if (component != null) return component;

            component = Motherboards.Find(com => com.Motherboard.Name.Equals(componentName, StringComparison.InvariantCultureIgnoreCase));
            return component;
        }

        public Software GetSoftware(string softwareName)
        {
            return Softwares.Find(s => s.Name.Equals(softwareName, StringComparison.InvariantCultureIgnoreCase));
        }

        internal void SoftwareBought(Software software)
        {
            software.WasBought = true;
        }

        internal void ComponentBought(StoreComponent component)
        {
            component.WasBought = true;
        }
    }
}