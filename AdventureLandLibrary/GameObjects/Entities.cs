using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureLandLibrary.GameObjects
{
    public class Entities
    {
        public static ConcurrentDictionary<string, Entity[]> PlayerEntities = new ConcurrentDictionary<string, Entity[]>();

    }
}
