using Assets.Scripts.Computers.ComponentTypes;
using System;

namespace Assets.Scripts.Computers
{
    public class Hard : ComputerComponent
    {
        private HardType hardType = HardType.None;
        private float size = -1;
        private Sizes sizeType = Sizes.None;
        private long totalSize;
        private long availableSize;

        public HardType HardType
        {
            get => hardType;
            set
            {
                if (hardType == HardType.None)
                    hardType = value;
            }
        }

        public float Size
        {
            get => size;
            set
            {
                if (size == -1)
                {
                    size = value;
                    TryUpdateTotalsize();
                }
            }
        }

        public Sizes SizeType
        {
            get => sizeType;
            set
            {
                if (sizeType == Sizes.None)
                {
                    sizeType = value;
                    TryUpdateTotalsize();
                }
            }
        }

        private void TryUpdateTotalsize()
        {
            if (size == -1 || sizeType == Sizes.None)
            {
                return;
            }

            totalSize = availableSize = (long)(size * (long)sizeType);
        }

        public bool CanRemoveData(long fileSize)
        {
            if (fileSize > totalSize - availableSize)
            {
                return false;
            }
            return true;
        }

        public void RemoveData(long fileSize)
        {            
            availableSize += fileSize;
        }

        public bool CanSaveData(long fileSize)
        {
            if (fileSize > availableSize)
            {
                return false;
            }
            return true;
        }

        public void SaveData(long fileSize)
        {           
            availableSize -= fileSize;
        }

        public override string ToString()
        {
            return $"{Name} {Size}{SizeType} {HardType}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Hard))
            {
                return false;
            }

            Hard hard = (Hard)obj;
            return (totalSize == hard.totalSize) && hardType == hard.hardType;
        }

        public override int GetHashCode()
        {
            return totalSize.GetHashCode() | hardType.GetHashCode();
        }

        #region operators >,<,>=,<=,==,!=
        public static bool operator >(Hard A, Hard B)
        {
            if (A.totalSize > B.totalSize)
            {
                return true;
            }

            return false;
        }

        public static bool operator <(Hard A, Hard B)
        {
            if (A.totalSize < B.totalSize)
            {
                return true;
            }

            return false;
        }

        public static bool operator >=(Hard A, Hard B)
        {
            if (A.totalSize >= B.totalSize)
            {
                return true;
            }

            return false;
        }

        public static bool operator <=(Hard A, Hard B)
        {
            if (A.totalSize <= B.totalSize)
            {
                return true;
            }

            return false;
        }

        public static bool operator ==(Hard A, Hard B)
        {
            if (A.totalSize == B.totalSize)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(Hard A, Hard B)
        {
            if (A.totalSize != B.totalSize)
            {
                return true;
            }

            return false;
        }
        #endregion operators

    }
}