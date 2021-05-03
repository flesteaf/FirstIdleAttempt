using Assets.Scripts.Softwares;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Computers
{
    internal class SpaceManagement
    {
        //TODO: finalize usage of storage
        private readonly Dictionary<Hard, List<Software>> storage;

        public SpaceManagement(List<Hard> hards)
        {
            storage = new Dictionary<Hard, List<Software>>();
            foreach (var hard in hards)
            {
                storage.Add(hard, new List<Software>());
            }
        }

        internal void UpdateStorage(Hard hard, bool hardRemoved)
        {
            storage.Add(hard, new List<Software>());

            if (!hardRemoved)
            {
                return;
            }

            Hard hardToBeRemoved = storage.Keys.First();
            foreach (var software in storage[hardToBeRemoved])
            {
                TryStoreSoftware(1, software);
            }

            storage.Remove(hardToBeRemoved);
        }

        internal bool TryStoreSoftware(Software software)
        {
            if (storage.Count == 0)
            {
                return false;
            }

            return TryStoreSoftware(0, software);
        }

        private bool TryStoreSoftware(int pos, Software software)
        {
            if (pos + 1 > storage.Count)
            {
                return false;
            }

            Hard hard = storage.Keys.ToArray()[pos];
            if (hard.CanSaveData(software.Size))
            {
                hard.SaveData(software.Size);
                storage[hard].Add(software);
                return true;
            }

            return TryStoreSoftware(pos + 1, software);
        }
    }
}