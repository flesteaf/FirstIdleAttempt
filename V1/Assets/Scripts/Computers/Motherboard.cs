namespace Assets.Scripts.Computers
{
    public class Motherboard : ComputerComponent
    {
        private int allowedCpus = -1;
        private int allowedRams = -1;
        private int allowedHards = -1;
        private int allowedGpus = -1;
        private int allowedNetworks = -1;

        public int AllowedCpus
        {
            get => allowedCpus;
            set
            {
                if (allowedCpus == -1)
                    allowedCpus = value;
            }
        }

        public int AllowedRams
        {
            get => allowedRams;
            set
            {
                if (allowedRams == -1)
                    allowedRams = value;
            }
        }

        public int AllowedHards
        {
            get => allowedHards;
            set
            {
                if (allowedHards == -1)
                    allowedHards = value;
            }
        }

        public int AllowedGpus
        {
            get => allowedGpus;
            set
            {
                if (allowedGpus == -1)
                    allowedGpus = value;
            }
        }

        public int AllowedNetworks
        {
            get => allowedNetworks;
            set
            {
                if (allowedNetworks == -1)
                    allowedNetworks = value;
            }
        }

        public override string ToString()
        {
            return $"{Name} accepting {AllowedCpus} CPUs, {AllowedRams} RAMs, {AllowedHards} Storages, {AllowedGpus} GPUs, {AllowedNetworks} networks";
        }

        #region Operators

        private static int Compare(Motherboard obj1, Motherboard obj2)
        {
            if (obj1 is null && obj2 is null)
            {
                return 0;
            }

            if (obj1 is null)
            {
                return -1;
            }

            if (obj2 is null)
            {
                return 1;
            }

            int result = obj1.AllowedCpus - obj2.AllowedCpus;
            if (result != 0)
                return result;

            result = obj1.AllowedGpus - obj2.AllowedGpus;
            if (result != 0)
                return result;

            result = obj1.AllowedRams - obj2.AllowedRams;
            if (result != 0)
                return result;

            result = obj1.AllowedHards - obj2.AllowedHards;
            if (result != 0)
                return result;

            result = obj1.AllowedNetworks - obj2.AllowedNetworks;
            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is Motherboard motherboard &&
                   allowedCpus == motherboard.allowedCpus &&
                   allowedRams == motherboard.allowedRams &&
                   allowedHards == motherboard.allowedHards &&
                   allowedGpus == motherboard.allowedGpus &&
                   allowedNetworks == motherboard.allowedNetworks;
        }

        public override int GetHashCode()
        {
            int hashCode = -324259551;
            hashCode = hashCode * -1521134295 + allowedCpus.GetHashCode();
            hashCode = hashCode * -1521134295 + allowedRams.GetHashCode();
            hashCode = hashCode * -1521134295 + allowedHards.GetHashCode();
            hashCode = hashCode * -1521134295 + allowedGpus.GetHashCode();
            hashCode = hashCode * -1521134295 + allowedNetworks.GetHashCode();
            return hashCode;
        }

        public static bool operator > (Motherboard obj1, Motherboard obj2)
        {
            if (Compare(obj1, obj2) > 0)
                return true;

            return false;
        }

        public static bool operator < (Motherboard obj1, Motherboard obj2)
        {
            if (Compare(obj1, obj2) < 0)
                return true;

            return false;
        }

        public static bool operator == (Motherboard obj1, Motherboard obj2)
        {
            if (Compare(obj1, obj2) == 0)
                return true;

            return false;
        }

        public static bool operator !=(Motherboard obj1, Motherboard obj2)
        {
            if (Compare(obj1, obj2) != 0)
                return true;

            return false;
        }

        #endregion Operators
    }
}