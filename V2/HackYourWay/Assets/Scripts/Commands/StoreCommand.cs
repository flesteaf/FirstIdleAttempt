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
        private readonly Dictionary<CommandOptions, Func<IGameLogic, IEnumerator>> storeTypes;
        private readonly GameStore store;

        public override CommandNames Name => CommandNames.store;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.software,
                            CommandOptions.components };
        }

        public StoreCommand(GameStore store)
        {
            storeTypes = new Dictionary<CommandOptions, Func<IGameLogic, IEnumerator>>
            {
                { CommandOptions.software, PresentSoftware },
                { CommandOptions.components, PresentComponents }
            };
            this.store = store;
        }

        public override IEnumerator Execute(IGameLogic game, CommandLine command)
        {
            if (!command.HasArgument())
            {
                PresentStoreOptions();
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

        private IEnumerator PresentComponents(IGameLogic game)
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

        private void ShowCPUs(IGameLogic game)
        {
            IEnumerable<CpuStore> cpus = store.CPUs;

            SendMessage("CPUs:", MessageType.Info);
            foreach (var item in cpus)
            {
                Cpu cpu = item.CPU;
                SendMessage($"{cpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowRAMs(IGameLogic game)
        {
            IEnumerable<RamStore> rams = store.RAMs;

            SendMessage("RAMs:", MessageType.Info);
            foreach (var item in rams)
            {
                Ram ram = item.RAM;
                SendMessage($"{ram.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowGPUs(IGameLogic game)
        {
            IEnumerable<GpuStore> gpus = store.GPUs;

            SendMessage("GPUs:", MessageType.Info);
            foreach (var item in gpus)
            {
                Gpu gpu = item.GPU;
                SendMessage($"{gpu.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowHards(IGameLogic game)
        {
            IEnumerable<HardStore> hards = store.Hards;

            SendMessage("Hards:", MessageType.Info);
            foreach (var item in hards)
            {
                Hard hard = item.Hard;
                SendMessage($"{hard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowMotherboards(IGameLogic game)
        {
            IEnumerable<MotherboardStore> motherboards = store.Motherboards;

            SendMessage("Motherboards:", MessageType.Info);
            foreach (var item in motherboards)
            {
                Motherboard motherboard = item.Motherboard;
                SendMessage($"{motherboard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowSources(IGameLogic game)
        {
            IEnumerable<SourceStore> sources = store.Sources;

            SendMessage("Sources:", MessageType.Info);
            foreach (var item in sources)
            {
                Source source = item.Source;
                SendMessage($"{source.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private void ShowNetworkBoards(IGameLogic game)
        {
            IEnumerable<NetworkBoardStore> networkBoards = store.Networks;

            SendMessage("NetworkBoards:", MessageType.Info);
            foreach (var item in networkBoards)
            {
                NetworkBoard networkBoard = item.Network;
                SendMessage($"{networkBoard.Name,-15} - {item.Price,4} - {item.Description}", MessageType.Info);
            }
        }

        private IEnumerator PresentSoftware(IGameLogic game)
        {
            IEnumerable<Software> softwares = store.Softwares;

            foreach (var item in softwares)
            {
                string price = item.WasBought ? "Bought" : item.Price.ToString();
                string text = $"{item.Name} - ${price} - {item.Description}";

                SendMessage(text, MessageType.Info);
            }

            yield break;
        }

        private void PresentStoreOptions()
        {
            SendMessage("store software - for buying new software", MessageType.Info);
            SendMessage("store components - for upgrading your computer", MessageType.Info);
        }

        #endregion StoreOptions
    }
}