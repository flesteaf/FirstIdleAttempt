using UnityEngine;
using HackYourWay.Models;

namespace HackYourWay.Data
{
    /// <summary>
    /// Designer-editable configuration for procedural location generation.
    /// The integer location ID is used as the PRNG seed for deterministic output.
    /// </summary>
    [CreateAssetMenu(fileName = "LocationConfig", menuName = "HackYourWay/Location Config")]
    public class LocationConfigSO : ScriptableObject
    {
        [Min(1)] public int MinNetworks = 2;
        [Min(1)] public int MaxNetworks = 5;

        [Tooltip("Probability weights for each security level (index = SecurityLevel enum value)")]
        public float[] SecurityDistribution = { 0.15f, 0.25f, 0.35f, 0.25f };

        [Min(1)] public int MinDevicesPerNetwork = 1;
        [Min(1)] public int MaxDevicesPerNetwork = 4;

        [Min(1)] public int MinFilesPerDevice = 1;
        [Min(1)] public int MaxFilesPerDevice = 8;

        [Tooltip("Range for randomly-generated ransom demands")]
        public float MinRansomAmount = 0.01f;
        public float MaxRansomAmount = 0.10f;
    }
}
