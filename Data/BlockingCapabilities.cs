using Gw2Sharp.Models;

namespace Lorf.BH.TTBlockersStuff.Data
{
    /// <summary>
    /// Data class for the different capabilities of the different professions and specializations .. WIP based on a list from Tany
    /// </summary>
    class BlockingCapabilities
    {
        // Mesmer
        public static readonly BlockingCapabilities Mesmer = new BlockingCapabilities(ProfessionType.Mesmer, 0, BlockingType.All, BlockingType.All, BlockingType.All);
        public static readonly BlockingCapabilities Chronomancer = new BlockingCapabilities(ProfessionType.Mesmer, 40, BlockingType.All, BlockingType.AllNotRecommended, BlockingType.Some);
        public static readonly BlockingCapabilities Mirage = new BlockingCapabilities(ProfessionType.Mesmer, 59, BlockingType.All, BlockingType.All, BlockingType.All);
        public static readonly BlockingCapabilities Virtuoso = new BlockingCapabilities(ProfessionType.Mesmer, 66, BlockingType.All, BlockingType.AllNotRecommended, BlockingType.Some);
        // Elementalist
        public static readonly BlockingCapabilities Elementalist = new BlockingCapabilities(ProfessionType.Elementalist, 0, BlockingType.All, BlockingType.All, BlockingType.Most);
        public static readonly BlockingCapabilities Tempest = new BlockingCapabilities(ProfessionType.Elementalist, 48, BlockingType.All, BlockingType.AllNotRecommended, BlockingType.Most);
        public static readonly BlockingCapabilities Weaver = new BlockingCapabilities(ProfessionType.Elementalist, 56, BlockingType.All, BlockingType.All, BlockingType.Most);
        public static readonly BlockingCapabilities Catalyst = new BlockingCapabilities(ProfessionType.Elementalist, 67, BlockingType.All, BlockingType.AllNotRecommended, BlockingType.Most);
        // Thief
        public static readonly BlockingCapabilities Thief = new BlockingCapabilities(ProfessionType.Thief, 0, BlockingType.All, BlockingType.All, BlockingType.Most);
        public static readonly BlockingCapabilities Daredevil = new BlockingCapabilities(ProfessionType.Thief, 7, BlockingType.All, BlockingType.AllNotRecommended, BlockingType.Most);
        public static readonly BlockingCapabilities Deadeye = new BlockingCapabilities(ProfessionType.Thief, 58, BlockingType.All, BlockingType.All, BlockingType.Most);
        public static readonly BlockingCapabilities Specter = new BlockingCapabilities(ProfessionType.Thief, 71, BlockingType.All, BlockingType.AllNotRecommended, BlockingType.Most);


        public ProfessionType Profession { get; }
        public int Specialization { get; }
        public BlockingType EggBlocking { get; }
        public BlockingType HuskBlocking { get; }
        public BlockingType AoEBlocking { get; }
        public bool SylvariRequired { get; }
        public bool SylvariRecommended { get; }

        public BlockingCapabilities(ProfessionType profession, int specialization, BlockingType eggBlocking, BlockingType huskBlocking, BlockingType aoEBlocking)
        {

        }
    }
}
