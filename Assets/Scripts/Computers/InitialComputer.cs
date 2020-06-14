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
            Cpus = new List<Cpu> { new Cpu() };
            Gpus = new List<Gpu> { new Gpu() };
            Networks = new List<NetworkBoard> { new NetworkBoard() };
            Sources = new List<Source> { new Source() };
            Motherboard = new Motherboard();
        }
    }
}