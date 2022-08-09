using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace TTBlockersFriend
{
    class GatheringSpot
    {
        public static readonly GatheringSpot Amber = new GatheringSpot("Amber", 90, new Vector2(670.07f, -606.34f), true, 100);
        public static readonly GatheringSpot Crimson = new GatheringSpot("Crimson", 90, new Vector2(198.5f, -438.5f), true, 100);
        public static readonly GatheringSpot Cobalt = new GatheringSpot("Cobalt", 80, new Vector2(-277.5f, -878.2f), true, 150);
        public static readonly GatheringSpot General = new GatheringSpot("Gathering", 0, new Vector2(185f, -83f), false, 100);
        public static readonly IEnumerable<GatheringSpot> All = new List<GatheringSpot> { Amber, Crimson, Cobalt, General };

        public string Name { get;  }
        public int HuskTime { get;  }
        public Vector2 Position { get; }
        public bool IsWurm { get; }
        public float SpotRadius { get; }

        private GatheringSpot(string name, int huskTime, Vector2 position, bool isWurm, float spotRadius)
        {
            Name = name;
            HuskTime = huskTime;
            Position = position;
            IsWurm = isWurm;
            SpotRadius = spotRadius;
        }

        public static GatheringSpot FromPosition(Vector2 position)
        {
            return All.FirstOrDefault(x => Vector2.Distance(x.Position, position) < x.SpotRadius);
        }
    }
}
