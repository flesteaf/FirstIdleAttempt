using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Computers.Motherboards
{
    internal static class MotherboardExtensions
    {
        internal static List<ComponentLoad> GetLoad(this Motherboard motherboard, Computer computer)
        {
            var components = new List<ComponentLoad>
            {
                new ComponentLoad { Name = "CPUs", CurrentNumber = computer.Cpus.Count, MaximumNumber = motherboard.AllowedCpus },
                new ComponentLoad { Name = "Rams", CurrentNumber = computer.Rams.Count, MaximumNumber = motherboard.AllowedRams },
                new ComponentLoad { Name = "Storage drives", CurrentNumber = computer.Hards.Count, MaximumNumber = motherboard.AllowedHards },
                new ComponentLoad { Name = "Gpus", CurrentNumber = computer.Gpus.Count, MaximumNumber = motherboard.AllowedGpus },
                new ComponentLoad { Name = "Networks", CurrentNumber = computer.Networks.Count, MaximumNumber = motherboard.AllowedNetworks },
                //new ComponentLoad { Name = "Wirelesses", CurrentNumber = computer.Wirelesses.Count, MaximumNumber = motherboard.AllowedWirelesses }
            };

            return components;
        }
    }
}
