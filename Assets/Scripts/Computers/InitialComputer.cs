using System.Collections.Generic;

namespace Assets.Scripts.Computers
{
    internal class InitialComputer : Computer
    {
        private List<Ram> rams;
        private List<Hard> hards;
        private List<Cpu> cpus;
        private List<Gpu> gpus;
        private List<NetworkBoard> networks;
        private List<Source> sources;
        private Motherboard motherboard;

        public override List<Ram> Rams { get => rams; protected set => rams = value; }
        public override List<Hard> Hards { get => hards; protected set => hards = value; }
        public override List<Cpu> Cpus { get => cpus; protected set => cpus = value; }
        public override List<Gpu> Gpus { get => gpus; protected set => gpus = value; }
        public override List<NetworkBoard> Networks { get => networks; protected set => networks = value; }
        public override List<Source> Sources { get => sources; protected set => sources = value; }
        public override Motherboard Motherboard { get => motherboard; protected set => motherboard = value; }
        internal override string Name { get => "Beginner"; }

        protected override void SetupComputer()
        {
            rams = new List<Ram> { new Ram() };
            hards = new List<Hard> { new Hard() };
            Cpus = new List<Cpu> { new Cpu() };
            Gpus = new List<Gpu> { new Gpu() };
            Networks = new List<NetworkBoard> { new NetworkBoard() };
            Sources = new List<Source> { new Source() };
            Motherboard = new Motherboard();
        }
    }
}