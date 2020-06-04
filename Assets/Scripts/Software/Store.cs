using System.Collections.Generic;
using Newtonsoft;
using UnityEngine;

namespace Assets.Scripts.Software
{
    internal class Store
    {
        private readonly string dataFile = $@"{Application.dataPath}\Scripts\AppData\StoreData.json";
        private readonly string schemaFile = $@"{Application.dataPath}\Scripts\AppData\data-schema.json";

        internal readonly IEnumerable<StoreComponent> Components;
        internal readonly IEnumerable<Software> Softwares;

        public Store(GameManager game)
        {
            

            //dataAsset = (TextAsset)Resources.Load(dataFile);
            //schemaAsset = (TextAsset)Resources.Load(schemaFile);
        }
    }
}
