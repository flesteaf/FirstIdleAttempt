using System.Collections.Generic;
using Newtonsoft;
using UnityEngine;

namespace Assets.Scripts.Software
{
    public class Store
    {
        public List<StoreComponent> Components { get; set; }
        public List<Software> Softwares { get; set; }

        public Store()
        {
        }
    }
}
