﻿using System.Collections.Generic;

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
                new ComponentLoad { Name = "Networks", CurrentNumber = computer.Networks.Count, MaximumNumber = motherboard.AllowedNetworks }
            };

            return components;
        }
    }
}