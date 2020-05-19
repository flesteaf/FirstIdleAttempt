using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Computers.ComponentTypes
{
    internal enum Sizes : long
    {
        b = 1,
        B = b * 8,
        KB = B * 1024,
        MB = KB * 1024,
        GB = MB * 1024,
        TB = GB * 1024
    }
}
