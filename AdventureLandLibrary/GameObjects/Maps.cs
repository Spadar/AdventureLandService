using System;
using System.Collections.Generic;
using System.Text;

namespace AdventureLandLibrary.GameObjects
{
    public static class Maps
    {
        public static Dictionary<string, Map> MapDictionary;

        static Maps()
        {
            MapDictionary = new Dictionary<string, Map>();


        }
    }
}
