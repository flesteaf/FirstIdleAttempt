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
        private readonly Dictionary<CommandOptions, Action<SceneManager>> storeTypes;
        public override CommandNames Name => CommandNames.store;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.software,
                            CommandOptions.components };
        }

        public StoreCommand()
        {
            storeTypes = new Dictionary<CommandOptions, Action<SceneManager>>
            {
                { CommandOptions.software, PresentSoftware },
                { CommandOptions.components, PresentComponents }
            };
        }

        public override void Execute(SceneManager game, CommandLine command)
        {
            IConsoleText console = game.Console;
            if (!command.HasArgument())
            {
                PresentStoreOptions(game);
                return;
            }

            if (!command.HasArgumentAndNoOption())
            {
                console.AddMessage("Invalid command call, the store command takes 1 parameter at most. Check help command for store", MessageType.Error);
                return;
            }

            if (!storeTypes.ContainsKey(command.ArgumentAsOption()))
            {
                console.AddMessage("Invalid parameter as input for store command, accepted are 'software' and 'components'", MessageType.Error);
                return;
            }

            storeTypes[command.ArgumentAsOption()](game);
        }

        #region StoreOptions

        private void PresentComponents(SceneManager game)
        {
            IConsoleText console = game.Console;
            ShowCPUs(game, console);
            ShowRAMs(game, console);
            ShowGPUs(game, console);
            ShowHards(game, console);
            ShowMotherboards(game, console);
            ShowSources(game, console);
            ShowNetworkBoards(game, console);
        }

        private static void ShowCPUs(SceneManager game, IConsoleText console)
        {
            IEnumerable<CpuStore> cpus = game.GetStoreCpus();

            console.AddMessage("CPUs:", MessageType.Info);
            foreach (var item in cpus)
            {
                Cpu cpu = item.CPU;
                console.AddMessage($"{cpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowRAMs(SceneManager game, IConsoleText console)
        {
            IEnumerable<RamStore> rams = game.GetStoreRams();

            console.AddMessage("RAMs:", MessageType.Info);
            foreach (var item in rams)
            {
                Ram ram = item.RAM;
                console.AddMessage($"{ram.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowGPUs(SceneManager game, IConsoleText console)
        {
            IEnumerable<GpuStore> gpus = game.GetStoreGpus();

            console.AddMessage("GPUs:", MessageType.Info);
            foreach (var item in gpus)
            {
                Gpu gpu = item.GPU;
                console.AddMessage($"{gpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowHards(SceneManager game, IConsoleText console)
        {
            IEnumerable<HardStore> hards = game.GetStoreHards();

            console.AddMessage("Hards:", MessageType.Info);
            foreach (var item in hards)
            {
                Hard hard = item.Hard;
                console.AddMessage($"{hard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowMotherboards(SceneManager game, IConsoleText console)
        {
            IEnumerable<MotherboardStore> motherboards = game.GetStoreMotherboards();

            console.AddMessage("Motherboards:", MessageType.Info);
            foreach (var item in motherboards)
            {
                Motherboard motherboard = item.Motherboard;
                console.AddMessage($"{motherboard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowSources(SceneManager game, IConsoleText console)
        {
            IEnumerable<SourceStore> sources = game.GetStoreSources();

            console.AddMessage("Sources:", MessageType.Info);
            foreach (var item in sources)
            {
                Source source = item.Source;
                console.AddMessage($"{source.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private static void ShowNetworkBoards(SceneManager game, IConsoleText console)
        {
            IEnumerable<NetworkBoardStore> networkBoards = game.GetStoreNetworkBoards();

            console.AddMessage("NetworkBoards:", MessageType.Info);
            foreach (var item in networkBoards)
            {
                NetworkBoard networkBoard = item.Network;
                console.AddMessage($"{networkBoard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void PresentSoftware(SceneManager game)
        {
            IEnumerable<Software> softwares = game.GetAllSoftwares();

            foreach (var item in softwares)
            {
                string price = item.WasBought ? "Bought" : item.Price.ToString();
                string text = $"{item.Name} - ${price} - {item.Description}";

                game.Console.AddMessage(text, MessageType.Info);
            }
        }

        private void PresentStoreOptions(SceneManager game)
        {
            game.Console.AddMessage("store software - for buying new software", MessageType.Info);
            game.Console.AddMessage("store components - for upgrading your computer", MessageType.Info);
        }

        #endregion StoreOptions
    }
}