namespace Assets.Scripts.Computers.ComponentTypes
{
    public enum Sizes : long
    {
        None = -1,
        b = 1,
        B = b * 8,
        KB = B * 1024,
        MB = KB * 1024,
        GB = MB * 1024,
        TB = GB * 1024
    }
}