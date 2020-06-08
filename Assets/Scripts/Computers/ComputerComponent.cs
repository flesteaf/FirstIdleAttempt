using System;

namespace Assets.Scripts.Computers
{
    public abstract class ComputerComponent
    {
        private string name = null;
        private int loadUsage = -1;

        public string Name
        {
            get => name;
            set
            {
                if (String.IsNullOrEmpty(""))
                    name = value;
            }
        }

        public int LoadUsage
        {
            get => loadUsage;
            set
            {
                if (loadUsage == -1)
                    loadUsage = value;
            }
        }
    }
}