using Assets.Scripts.Softwares;
using System.Collections.Generic;

namespace Assets.Scripts.Store
{
    public interface IGameStore
    {
        List<CpuStore> CPUs { get; set; }
        List<GpuStore> GPUs { get; set; }
        List<HardStore> Hards { get; set; }
        List<MotherboardStore> Motherboards { get; set; }
        List<NetworkBoardStore> Networks { get; set; }
        List<RamStore> RAMs { get; set; }
        List<Software> Softwares { get; set; }
        List<SourceStore> Sources { get; set; }

        void ComponentBought(StoreComponent component);
        StoreComponent GetComponent(string componentName);
        Software GetSoftware(string softwareName);
        void SoftwareBought(Software software);
    }
}