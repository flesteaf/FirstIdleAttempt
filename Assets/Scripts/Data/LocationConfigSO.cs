using Godot;
using HackYourWay.Models;

namespace HackYourWay.Data
{
    /// <summary>
    /// Designer-editable configuration for procedural location generation.
    /// The integer location ID is used as the PRNG seed for deterministic output.
    /// </summary>
    [GlobalClass]
    public partial class LocationConfigSO : Resource
    {
        [Export(PropertyHint.Range, "1,")] public int MinNetworks { get; set; } = 2;
        [Export(PropertyHint.Range, "1,")] public int MaxNetworks { get; set; } = 5;

        [Export] public float[] SecurityDistribution { get; set; } = { 0.15f, 0.25f, 0.35f, 0.25f };

        [Export(PropertyHint.Range, "1,")] public int MinDevicesPerNetwork { get; set; } = 1;
        [Export(PropertyHint.Range, "1,")] public int MaxDevicesPerNetwork { get; set; } = 4;

        [Export(PropertyHint.Range, "1,")] public int MinFilesPerDevice { get; set; } = 1;
        [Export(PropertyHint.Range, "1,")] public int MaxFilesPerDevice { get; set; } = 8;

        [Export] public float MinRansomAmount { get; set; } = 0.01f;
        [Export] public float MaxRansomAmount { get; set; } = 0.10f;
    }
}
