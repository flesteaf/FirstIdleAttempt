using Assets.Scripts.Computers.ComponentTypes;
using System.Collections.Generic;

namespace Assets.Scripts.Computers
{
    public class InitialComputer : Computer
    {
        protected override void SetupComputer()
        {
            //TODO: set initial computer
            Rams = new List<Ram> { new Ram() };
            Hards = new List<Hard> { new Hard() };
            Cpus = new List<Cpu> { new Cpu { Cores = 1, SpeedType = SpeedType.MHz, Speed = 1 } };
            Gpus = new List<Gpu> { new Gpu() };
            Networks = new List<NetworkBoard> { new NetworkBoard { SizeType = Sizes.B, Speed = 100 } };
            Sources = new List<Source> { new Source() };
            Motherboard = new Motherboard();
        }
    }
}