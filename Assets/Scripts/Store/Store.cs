using Assets.Scripts.Softwares;
using System.Collections.Generic;

namespace Assets.Scripts.Store
{
    public class Store
    {
        public List<Software> Softwares { get; set; }
        public List<CpuStore> CPUs { get; set; }
        public List<GpuStore> GPUs { get; set; }
        public List<HardStore> Hards { get; set; }
        public List<NetworkBoardStore> Networks { get; set; }
        public List<RamStore> RAMs { get; set; }
        public List<SourceStore> Sources { get; set; }
        public List<MotherboardStore> Motherboards { get; set; }

        public Store()
        {
        }
    }
}