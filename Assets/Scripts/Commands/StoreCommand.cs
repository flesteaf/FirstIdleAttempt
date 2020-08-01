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
        private readonly Dictionary<CommandOptions, Action<GameData>> storeTypes;
        public override CommandNames Name => CommandNames.store;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.software,
                            CommandOptions.components };
        }

        public StoreCommand()
        {
            storeTypes = new Dictionary<CommandOptions, Action<GameData>>
            {
                { CommandOptions.software, PresentSoftware },
                { CommandOptions.components, PresentComponents }
            };
        }

        public override void Execute(GameData game, CommandLine command)
        {
            if (!command.HasArgument())
            {
                PresentStoreOptions(game);
                return;
            }

            if (!command.HasArgumentAndNoOption())
            {
                SendMessage("Invalid command call, the store command takes 1 parameter at most. Check help command for store", MessageType.Error);
                return;
            }

            if (!storeTypes.ContainsKey(command.ArgumentAsOption()))
            {
                SendMessage("Invalid parameter as input for store command, accepted are 'software' and 'components'", MessageType.Error);
                return;
            }

            storeTypes[command.ArgumentAsOption()](game);
        }

        #region StoreOptions

        private void PresentComponents(GameData game)
        {
            ShowCPUs(game);
            ShowRAMs(game);
            ShowGPUs(game);
            ShowHards(game);
            ShowMotherboards(game);
            ShowSources(game);
            ShowNetworkBoards(game);
        }

        private void ShowCPUs(GameData game)
        {
            IEnumerable<CpuStore> cpus = game.GetStoreCpus();

            SendMessage("CPUs:", MessageType.Info);
            foreach (var item in cpus)
            {
                Cpu cpu = item.CPU;
                SendMessage($"{cpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowRAMs(GameData game)
        {
            IEnumerable<RamStore> rams = game.GetStoreRams();

            SendMessage("RAMs:", MessageType.Info);
            foreach (var item in rams)
            {
                Ram ram = item.RAM;
                SendMessage($"{ram.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowGPUs(GameData game)
        {
            IEnumerable<GpuStore> gpus = game.GetStoreGpus();

            SendMessage("GPUs:", MessageType.Info);
            foreach (var item in gpus)
            {
                Gpu gpu = item.GPU;
                SendMessage($"{gpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowHards(GameData game)
        {
            IEnumerable<HardStore> hards = game.GetStoreHards();

            SendMessage("Hards:", MessageType.Info);
            foreach (var item in hards)
            {
                Hard hard = item.Hard;
                SendMessage($"{hard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowMotherboards(GameData game)
        {
            IEnumerable<MotherboardStore> motherboards = game.GetStoreMotherboards();

            SendMessage("Motherboards:", MessageType.Info);
            foreach (var item in motherboards)
            {
                Motherboard motherboard = item.Motherboard;
                SendMessage($"{motherboard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowSources(GameData game)
        {
            IEnumerable<SourceStore> sources = game.GetStoreSources();

            SendMessage("Sources:", MessageType.Info);
            foreach (var item in sources)
            {
                Source source = item.Source;
                SendMessage($"{source.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowNetworkBoards(GameData game)
        {
            IEnumerable<NetworkBoardStore> networkBoards = game.GetStoreNetworkBoards();

            SendMessage("NetworkBoards:", MessageType.Info);
            foreach (var item in networkBoards)
            {
                NetworkBoard networkBoard = item.Network;
                SendMessage($"{networkBoard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void PresentSoftware(GameData game)
        {
            IEnumerable<Software> softwares = game.GetAllSoftwares();

            foreach (var item in softwares)
            {
                string price = item.WasBought ? "Bought" : item.Price.ToString();
                string text = $"{item.Name} - ${price} - {item.Description}";

                SendMessage(text, MessageType.Info);
            }
        }

        private void PresentStoreOptions(GameData game)
        {
            SendMessage("store software - for buying new software", MessageType.Info);
            SendMessage("store components - for upgrading your computer", MessageType.Info);
        }

        #endregion StoreOptions
    }
}