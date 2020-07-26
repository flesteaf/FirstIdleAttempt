using Assets.Scripts.Softwares;
using System.Collections.Generic;

namespace Assets.Scripts.Store
{
    public interface IStore
    {
        List<Software> Softwares { get; set; }
        List<CpuStore> CPUs { get; set; }
        List<GpuStore> GPUs { get; set; }
        List<HardStore> Hards { get; set; }
        List<NetworkBoardStore> Networks { get; set; }
        List<RamStore> RAMs { get; set; }
        List<SourceStore> Sources { get; set; }
        List<MotherboardStore> Motherboards { get; set; }

        StoreComponent GetComponent(string componentName);
        Software GetSoftware(string softwareName);
    }
}