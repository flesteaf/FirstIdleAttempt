using Assets.Scripts.Computers;
using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class StoreCommand : Command
    {
        private readonly Dictionary<string, Action<GameManager>> storeTypes;
        public override string Name => "store";

        public StoreCommand()
        {
            storeTypes = new Dictionary<string, Action<GameManager>>
            {
                { CommandOptions.software.ToString(), PresentSoftware },
                { CommandOptions.components.ToString(), PresentComponents }
            };
        }

        public override void Execute(GameManager game, string command)
        {
            string[] components = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (components.Length == 1)
            {
                PresentStoreOptions(game);
                return;
            }

            if (components.Length != 2)
            {
                game.Console.AddMessage("Invalid command call, the store command takes 1 parameter at most. Check help command for store", MessageType.Error);
                return;
            }

            if (!storeTypes.ContainsKey(components[1]))
            {
                game.Console.AddMessage("Invalid parameter as input for store command, accepted are 'software' and 'components'", MessageType.Error);
                return;
            }

            storeTypes[components[1]](game);
        }

        #region StoreOptions

        private void PresentComponents(GameManager game)
        {
            ConsoleText console = game.Console;
            ShowCPUs(game, console);
            ShowRAMs(game, console);
            ShowGPUs(game, console);
            ShowHards(game, console);
            ShowMotherboards(game, console);
            ShowSources(game, console);
            ShowNetworkBoards(game, console);
        }

        private static void ShowCPUs(GameManager game, ConsoleText console)
        {
            IEnumerable<CpuStore> cpus = game.GetStoreCpus();

            console.AddMessage("CPUs:", MessageType.Info);
            foreach (var item in cpus)
            {
                Cpu cpu = item.CPU;
                console.AddMessage($"{cpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowRAMs(GameManager game, ConsoleText console)
        {
            IEnumerable<RamStore> rams = game.GetStoreRams();

            console.AddMessage("RAMs:", MessageType.Info);
            foreach (var item in rams)
            {
                Ram ram = item.RAM;
                console.AddMessage($"{ram.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowGPUs(GameManager game, ConsoleText console)
        {
            IEnumerable<GpuStore> gpus = game.GetStoreGpus();

            console.AddMessage("GPUs:", MessageType.Info);
            foreach (var item in gpus)
            {
                Gpu gpu = item.GPU;
                console.AddMessage($"{gpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowHards(GameManager game, ConsoleText console)
        {
            IEnumerable<HardStore> hards = game.GetStoreHards();

            console.AddMessage("Hards:", MessageType.Info);
            foreach (var item in hards)
            {
                Hard hard = item.Hard;
                console.AddMessage($"{hard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowMotherboards(GameManager game, ConsoleText console)
        {
            IEnumerable<MotherboardStore> motherboards = game.GetStoreMotherboards();

            console.AddMessage("Motherboards:", MessageType.Info);
            foreach (var item in motherboards)
            {
                Motherboard motherboard = item.Motherboard;
                console.AddMessage($"{motherboard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowSources(GameManager game, ConsoleText console)
        {
            IEnumerable<SourceStore> sources = game.GetStoreSources();

            console.AddMessage("Sources:", MessageType.Info);
            foreach (var item in sources)
            {
                Source source = item.Source;
                console.AddMessage($"{source.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowNetworkBoards(GameManager game, ConsoleText console)
        {
            IEnumerable<NetworkBoardStore> networkBoards = game.GetStoreNetworkBoards();

            console.AddMessage("NetworkBoards:", MessageType.Info);
            foreach (var item in networkBoards)
            {
                NetworkBoard networkBoard = item.Network;
                console.AddMessage($"{networkBoard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void PresentSoftware(GameManager game)
        {
            IEnumerable<Software> softwares = game.GetAllSoftwares();

            foreach (var item in softwares)
            {
                string text = $"{item.Name} - ${item.Price} - {item.Description}";
                if (item.WasBought)
                {
                    text += " | Bought";
                }

                game.Console.AddMessage(text, MessageType.Info);
            }
        }

        private void PresentStoreOptions(GameManager game)
        {
            game.Console.AddMessage("store software - for buying new software", MessageType.Info);
            game.Console.AddMessage("store components - for upgrading your computer", MessageType.Info);
        }

        #endregion StoreOptions
    }
}