using System.Linq;

namespace Cosmic.Model
{
    class NoOpFlareCard : ICard
    {
        private readonly int alienId;

        public int AlienId { get { return this.alienId; } }
        public Alien Alien { get { return AlienDefinitions.Aliens[this.alienId]; } }

        public NoOpFlareCard(int alienId)
        {
            this.alienId = alienId;
        }
    }

    public class FlareDeck : Deck<ICard>
    {
        public FlareDeck() : base(Cards.FlareCards)
        {
        }
    }

    public static class AlienDefinitions
    {
        public static readonly Alien[] Aliens = new Alien[] {
            new Alien(00, "Amoeba"),
            new Alien(01, "Anti-Matter"),
            new Alien(02, "Barbarian"),
            new Alien(03, "Calculator"),
            new Alien(04, "Chosen"),
            new Alien(05, "Citadel"),
            new Alien(06, "Clone"),
            new Alien(07, "Cudgel"),
            new Alien(08, "Dictator"),
            new Alien(09, "Fido"),
            new Alien(10, "Filch"),
            new Alien(11, "Fodder"),
            new Alien(12, "Gambler"),
            new Alien(13, "Grudge"),
            new Alien(14, "Hacker"),
            new Alien(15, "Hate"),
            new Alien(16, "Healer"),
            new Alien(17, "Human"),
            new Alien(18, "Kamikaze"),
            new Alien(19, "Loser"),
            new Alien(20, "Machine"),
            new Alien(21, "Macron"),
            new Alien(22, "Masochist"),
            new Alien(23, "Mind"),
            new Alien(24, "Mirror"),
            new Alien(25, "Miser"),
            new Alien(26, "Mite"),
            new Alien(27, "Mutant"),
            new Alien(28, "Observer"),
            new Alien(29, "Oracle"),
            new Alien(30, "Pacifist"),
            new Alien(31, "Parasite"),
            new Alien(32, "Philanthropist"),
            new Alien(33, "Reincarnator"),
            new Alien(34, "Remora"),
            new Alien(35, "Reserve"),
            new Alien(36, "Shadow"),
            new Alien(37, "Sorcerer"),
            new Alien(38, "Spiff"),
            new Alien(39, "Tick-Tock"),
            new Alien(40, "Trader"),
            new Alien(41, "Tripler"),
            new Alien(42, "Vacuum"),
            new Alien(43, "Virus"),
            new Alien(44, "Void"),
            new Alien(45, "Vulch"),
            new Alien(46, "Warpish"),
            new Alien(47, "Warrior"),
            new Alien(48, "Will"),
            new Alien(49, "Zombie"),
        };
    }

    public class Alien
    {
        private readonly int id;
        private readonly string name;

        public Alien(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public int Id { get; }
        public string Name { get; }

    }

    public static class Cards
    {
        // 50 Flares
        public readonly static ICard[] FlareCards = new ICard[] {
            new NoOpFlareCard(00),
            new NoOpFlareCard(01),
            new NoOpFlareCard(02),
            new NoOpFlareCard(03),
            new NoOpFlareCard(04),
            new NoOpFlareCard(05),
            new NoOpFlareCard(06),
            new NoOpFlareCard(07),
            new NoOpFlareCard(08),
            new NoOpFlareCard(09),
            new NoOpFlareCard(10),
            new NoOpFlareCard(11),
            new NoOpFlareCard(12),
            new NoOpFlareCard(13),
            new NoOpFlareCard(14),
            new NoOpFlareCard(15),
            new NoOpFlareCard(16),
            new NoOpFlareCard(17),
            new NoOpFlareCard(18),
            new NoOpFlareCard(19),
            new NoOpFlareCard(20),
            new NoOpFlareCard(21),
            new NoOpFlareCard(22),
            new NoOpFlareCard(23),
            new NoOpFlareCard(24),
            new NoOpFlareCard(25),
            new NoOpFlareCard(26),
            new NoOpFlareCard(27),
            new NoOpFlareCard(28),
            new NoOpFlareCard(29),
            new NoOpFlareCard(30),
            new NoOpFlareCard(31),
            new NoOpFlareCard(32),
            new NoOpFlareCard(33),
            new NoOpFlareCard(34),
            new NoOpFlareCard(35),
            new NoOpFlareCard(36),
            new NoOpFlareCard(37),
            new NoOpFlareCard(38),
            new NoOpFlareCard(39),
            new NoOpFlareCard(40),
            new NoOpFlareCard(41),
            new NoOpFlareCard(42),
            new NoOpFlareCard(43),
            new NoOpFlareCard(44),
            new NoOpFlareCard(45),
            new NoOpFlareCard(46),
            new NoOpFlareCard(47),
            new NoOpFlareCard(48),
            new NoOpFlareCard(49),
        };

        // 39 Attacks: 00, 01, 04 x4, 05, 06 x7, 07, 08 x7, 09, 10 x4, 11, 12 x2, 13, 14 x2, 15, 20 x2, 23, 30, 40
        public static readonly ICard[] AttackCards = new ICard[] {
            new AttackEncounterCard(00),
            new AttackEncounterCard(01),
            new AttackEncounterCard(04),
            new AttackEncounterCard(04),
            new AttackEncounterCard(04),
            new AttackEncounterCard(04),
            new AttackEncounterCard(05),
            new AttackEncounterCard(06),
            new AttackEncounterCard(06),
            new AttackEncounterCard(06),
            new AttackEncounterCard(06),
            new AttackEncounterCard(06),
            new AttackEncounterCard(06),
            new AttackEncounterCard(06),
            new AttackEncounterCard(07),
            new AttackEncounterCard(08),
            new AttackEncounterCard(08),
            new AttackEncounterCard(08),
            new AttackEncounterCard(08),
            new AttackEncounterCard(08),
            new AttackEncounterCard(08),
            new AttackEncounterCard(08),
            new AttackEncounterCard(09),
            new AttackEncounterCard(10),
            new AttackEncounterCard(10),
            new AttackEncounterCard(10),
            new AttackEncounterCard(10),
            new AttackEncounterCard(11),
            new AttackEncounterCard(12),
            new AttackEncounterCard(12),
            new AttackEncounterCard(13),
            new AttackEncounterCard(14),
            new AttackEncounterCard(14),
            new AttackEncounterCard(15),
            new AttackEncounterCard(20),
            new AttackEncounterCard(20),
            new AttackEncounterCard(23),
            new AttackEncounterCard(30),
            new AttackEncounterCard(40)
        };

        public static readonly ICard[] NegotiateCards = Enumerable.Repeat(1, 15).Select(i => new NegotiateEncounterCard()).ToArray();
    }
}
