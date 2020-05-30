using Assets.Scripts.Computers.CPUs;
using Assets.Scripts.Computers.GPUs;
using Assets.Scripts.Computers.HARDs;
using Assets.Scripts.Computers.Networks;
using Assets.Scripts.Computers.Rams;
using Assets.Scripts.Computers.Sources;
using Assets.Scripts.Computers.Wirelesses;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Extensions
{
    internal static class ListExtensions
    {
        internal static string ToStatus(this List<Cpu> cpus)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < cpus.Count; i++)
            {
                builder.Append($"CPU{i + 1} - {cpus[i]} |");
            }
            return builder.ToString();
        }

        internal static string ToStatus(this List<Hard> hards)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hards.Count; i++)
            {
                builder.Append($"Storage{i + 1} - {hards[i]} |");
            }
            return builder.ToString();
        }

        internal static string ToStatus(this List<Ram> rams)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < rams.Count; i++)
            {
                builder.Append($"RAM{i + 1} - {rams[i]} |");
            }
            return builder.ToString();
        }

        internal static string ToStatus(this List<Gpu> gpus)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < gpus.Count; i++)
            {
                builder.Append($"GPU{i + 1} - {gpus[i]} |");
            }
            return builder.ToString();
        }

        internal static string ToStatus(this List<Network> networks)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < networks.Count; i++)
            {
                builder.Append($"CON{i + 1} - {networks[i]} |");
            }
            return builder.ToString();
        }

        internal static string ToStatus(this List<Wireless> wirelesses)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < wirelesses.Count; i++)
            {
                builder.Append($"Wireless{i + 1} - {wirelesses[i]} |");
            }
            return builder.ToString();
        }

        internal static string ToStatus(this List<Source> sources)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < sources.Count; i++)
            {
                builder.Append($"Source{i + 1} - {sources[i]} |");
            }
            return builder.ToString();
        }
    }
}