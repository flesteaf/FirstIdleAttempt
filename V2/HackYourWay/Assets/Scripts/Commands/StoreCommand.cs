using Assets.Scripts.Computers;
using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class StoreCommand : Command
    {
        private readonly Dictionary<CommandOptions, Func<IGameData, IEnumerator>> storeTypes;
        public override CommandNames Name => CommandNames.store;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.software,
                            CommandOptions.components };
        }

        public StoreCommand()
        {
            storeTypes = new Dictionary<CommandOptions, Func<IGameData, IEnumerator>>
            {
                { CommandOptions.software, PresentSoftware },
                { CommandOptions.components, PresentComponents }
            };
        }

        public override IEnumerator Execute(IGameData game, CommandLine command)
        {
            if (!command.HasArgument())
            {
                PresentStoreOptions(game);
                yield break;
            }

            if (!command.HasArgumentAndNoOption())
            {
                SendMessage("Invalid command call, the store command takes 1 parameter at most. Check help command for store", MessageType.Error);
                yield break;
            }

            if (!storeTypes.ContainsKey(command.ArgumentAsOption()))
            {
                SendMessage("Invalid parameter as input for store command, accepted are 'software' and 'components'", MessageType.Error);
                yield break;
            }

            yield return storeTypes[command.ArgumentAsOption()](game);
        }

        #region StoreOptions

        private IEnumerator PresentComponents(IGameData game)
        {
            ShowCPUs(game);
            ShowRAMs(game);
            ShowGPUs(game);
            ShowHards(game);
            ShowMotherboards(game);
            ShowSources(game);
            ShowNetworkBoards(game);

            yield break;
        }

        private void ShowCPUs(IGameData game)
        {
            IEnumerable<CpuStore> cpus = game.Store.CPUs;

            SendMessage("CPUs:", MessageType.Info);
            foreach (var item in cpus)
            {
                Cpu cpu = item.CPU;
                SendMessage($"{cpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowRAMs(IGameData game)
        {
            IEnumerable<RamStore> rams = game.Store.RAMs;

            SendMessage("RAMs:", MessageType.Info);
            foreach (var item in rams)
            {
                Ram ram = item.RAM;
                SendMessage($"{ram.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowGPUs(IGameData game)
        {
            IEnumerable<GpuStore> gpus = game.Store.GPUs;

            SendMessage("GPUs:", MessageType.Info);
            foreach (var item in gpus)
            {
                Gpu gpu = item.GPU;
                SendMessage($"{gpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowHards(IGameData game)
        {
            IEnumerable<HardStore> hards = game.Store.Hards;

            SendMessage("Hards:", MessageType.Info);
            foreach (var item in hards)
            {
                Hard hard = item.Hard;
                SendMessage($"{hard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowMotherboards(IGameData game)
        {
            IEnumerable<MotherboardStore> motherboards = game.Store.Motherboards;

            SendMessage("Motherboards:", MessageType.Info);
            foreach (var item in motherboards)
            {
                Motherboard motherboard = item.Motherboard;
                SendMessage($"{motherboard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowSources(IGameData game)
        {
            IEnumerable<SourceStore> sources = game.Store.Sources;

            SendMessage("Sources:", MessageType.Info);
            foreach (var item in sources)
            {
                Source source = item.Source;
                SendMessage($"{source.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowNetworkBoards(IGameData game)
        {
            IEnumerable<NetworkBoardStore> networkBoards = game.Store.Networks;

            SendMessage("NetworkBoards:", MessageType.Info);
            foreach (var item in networkBoards)
            {
                NetworkBoard networkBoard = item.Network;
                SendMessage($"{networkBoard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private IEnumerator PresentSoftware(IGameData game)
        {
            IEnumerable<Software> softwares = game.Store.Softwares;

            foreach (var item in softwares)
            {
                string price = item.WasBought ? "Bought" : item.Price.ToString();
                string text = $"{item.Name} - ${price} - {item.Description}";

                SendMessage(text, MessageType.Info);
            }

            yield break;
        }

        private void PresentStoreOptions(IGameData game)
        {
            SendMessage("store software - for buying new software", MessageType.Info);
            SendMessage("store components - for upgrading your computer", MessageType.Info);
        }

        #endregion StoreOptions
    }
}