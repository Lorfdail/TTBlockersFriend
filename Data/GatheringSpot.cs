using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TTBlockersStuff.Language;

namespace Lorf.BH.TTBlockersStuff
{
    /// <summary>
    /// Basically just the data backing the remaining logic .. short definitions of all the arenas
    /// </summary>
    class GatheringSpot
    {
        // at this point its not easy to get more precise than this
        public readonly static GatheringSpot Amber = new GatheringSpot(Translations.GatheringSpotTitleAmber, 90, new Vector2(670.4353339f, -606.3869562f), true, 100);
        public readonly static GatheringSpot Crimson = new GatheringSpot(Translations.GatheringSpotTitleCrimson, 90, new Vector2(198.4920996f, -438.1532447f), true, 100);
        public readonly static GatheringSpot Cobalt = new GatheringSpot(Translations.GatheringSpotTitleCobalt, 80, new Vector2(-277.4964237f, -878.2016061f), true, 150);
        public readonly static GatheringSpot General = new GatheringSpot(Translations.GatheringspotTitleMain, 90, new Vector2(185f, -83f), false, 100);
        public readonly static IEnumerable<GatheringSpot> All = new List<GatheringSpot> { Amber, Crimson, Cobalt, General };

        public string Name { get; }
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
