using Assets.Scripts.Computers.CPUs;
using Assets.Scripts.Computers.GPUs;
using Assets.Scripts.Computers.HARDs;
using Assets.Scripts.Computers.Motherboards;
using Assets.Scripts.Computers.Networks;
using Assets.Scripts.Computers.Rams;
using Assets.Scripts.Computers.Sources;
using Assets.Scripts.Computers.Wirelesses;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Computers
{
    internal class InitialComputer : Computer
    {
        private List<Ram> rams;
        private List<Hard> hards;
        private List<Cpu> cpus;
        private List<Gpu> gpus;
        private List<Network> networks;
        private List<Wireless> wirelesses;
        private List<Source> sources;
        private Motherboard motherboard;

        public override List<Ram> Rams { get => rams; protected set => rams=value; }
        public override List<Hard> Hards { get => hards; protected set => hards=value; }
        public override List<Cpu> Cpus { get => cpus; protected set => cpus=value; }
        public override List<Gpu> Gpus { get => gpus; protected set => gpus=value; }
        public override List<Network> Networks { get => networks; protected set => networks=value; }
        public override List<Wireless> Wirelesses { get => wirelesses; protected set => wirelesses=value; }
        public override List<Source> Sources { get => sources; protected set => sources=value; }
        public override Motherboard Motherboard { get => motherboard; protected set => motherboard=value; }
        internal override string Name { get => "Beginner"; }

        protected override void SetupComputer()
        {
            rams = new List<Ram> { new DDR116MBRam() };
            hards = new List<Hard> { new HardDisk1GB() };
            Cpus = new List<Cpu> { new Core1Speed200MHz() };
            Gpus = new List<Gpu> { new AtariGpu() };
            Networks = new List<Network> { new DialupNetwork() };
            Wirelesses = new List<Wireless>();
            Sources = new List<Source> { new Wats100() };
            Motherboard = new SmallMotherboard();
        }
    }
}
