using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    /*
    Cosmic Encounter: Totals by deck: 
    20 destiny, 72 cosmic, 20 tech, 51 flare. During gameplay, a full cosmic deck has 72 cards plus 10 flares for a total of 82 cards (if there are more than five players, add 2 flares per additional player).
    20 Destiny: Blue x3, Green x3, Purple x3, Red x3, Yellow x3, Wild x2, Special x3
    15 Negotiates
    1 Morph
    6 Reinforcements: +2 x2, +3 x3, +5
    11 Artifacts: Card Zap x2, Cosmic Zap x2, Emotion Control, Force Field, Ionic Gas, Mobius Tubes x2, Plague, Quash
    20 Techs: Coldsleep Ship, Collapsium Hulls, Cosmic Field Generator, Delta Scanners, Energy Cloak, Enigma Device, Genesis Bomb, Gluon Mines, Infinity Drive, Lunar Cannon, Omega Missile, Plasma Thrusters, Precursor Seed, The Prometheus, The Qax, Quark Battery, Tech Scrambler, Vacuum Turbines, Warpspace Key, Xenon Lasers
    51 Flares: Amoeba, Anti-Matter, Barbarian, Calculator, Chosen, Citadel, Clone, Cudgel, Dictator, Fido, Filch, Filch (Classic Edition), Fodder, Gambler, Grudge, Hacker, Hate, Healer, Human, Kamikaze, Loser, Machine, Macron, Masochist, Mind, Mirror, Miser, Mite, Mutant, Observer, Oracle, Pacifist, Parasite, Philanthropist, Reincarnator, Remora, Reserve, Shadow, Sorcerer, Spiff, Tick-Tock, Trader, Tripler, Vacuum, Virus, Void, Vulch, Warpish, Warrior, Will, Zombie
    */

    public interface ISelection<T>
    {
        IEnumerable<T> Selection { get; }
    }

    public interface IHand : IEnumerable<ICard>
    {
        void Add(ICard card);
        void Remove(ICard card);
    }

    public interface IShipContainer
    {
        void AddShip(IShip ship);
        IEnumerable<IShip> GetShips();
        IEnumerable<IShip> GetShips(IPlayer player);
        IShip RemoveShip(IPlayer player);
    }

    public interface IHyperspaceGate : IShipContainer
    {
    }

    public interface IWarp : IShipContainer
    {
    }

    public interface IPlanet : IShipContainer
    {
        IPlayer Owner { get; }
    }

    public class GameState
    {
        private int activePlayerIndex;
        private int defensePlayerIndex;
        private readonly IHyperspaceGate gate;
        private readonly IWarp warp;
        private DestinyDeck destinyDeck;
        private readonly List<ICard> discardPile;

        private IPlayer[] players;
        private Alien[] aliens;
        private IHand[] hands;
        private List<IPlanet> planets;

        private NormalDeck encounterDeck;

        public GameState()
        {
            this.warp = new Warp();
            this.gate = new HyperspaceGate();
        }

        public IPlayer ActivePlayer { get { return this.players[this.activePlayerIndex]; } }

        public IWarp Warp { get { return this.warp; } }

        public IHyperspaceGate HyperspaceGate { get { return this.gate; } }

        public IPlayer DefensePlayer { get { return this.players[this.defensePlayerIndex]; } }

        public IPlayer[] Players { get { return this.players; } }
        public IPlayer[] NonOffensePlayers { get { return this.players.Where(player => player != this.ActivePlayer).ToArray(); } }

        public void SetActivePlayer(IPlayer player)
        {
            this.activePlayerIndex = this.GetPlayerIndex(player);
        }

        public IDestinyCard DrawDestinyCard()
        {
            return (IDestinyCard)this.destinyDeck.Draw();
        }

        public void SetEncounterDeck(NormalDeck deck)
        {
            this.encounterDeck = deck;
        }

        public void SetPlayers(params IPlayer[] players)
        {
            if (this.players != null)
            {
                throw new InvalidOperationException("Cannot set players more than once");
            }
            this.players = players;
            this.hands = new IHand[players.Length];
            this.planets = new List<IPlanet>();
            for (int i = 0; i < players.Length; i++)
            {
                this.hands[i] = new Hand();
            }
        }

        public void SetAliens(params Alien[] aliens)
        {
            this.aliens = aliens;
        }

        public void SetDestinyDeck(DestinyDeck deck)
        {
            this.destinyDeck = deck;
        }

        public ICard DrawCard()
        {
            return this.encounterDeck.Draw();
        }

        public void DrawCardToHand(IPlayer player)
        {
            int index = this.GetPlayerIndex(player);
            this.hands[index].Add(this.DrawCard());
        }

        public void AddCardToHand(IPlayer player, ICard card)
        {
            int index = this.GetPlayerIndex(player);
            this.hands[index].Add(card);
        }

        public void RemoveCardFromHand(IPlayer player, ICard card)
        {
            int index = this.GetPlayerIndex(player);
            this.hands[index].Remove(card);
        }

        public void DiscardCard(ICard card)
        {
            this.discardPile.Add(card);
        }

        public void RemoveFromDiscards(ICard card)
        {
            this.discardPile.Remove(card);
        }

        public void AddPlanet(IPlanet planet)
        {
            this.planets.Add(planet);
        }

        public void RemovePlanet(IPlayer player, IPlanet planet)
        {
            this.planets.Remove(planet);
        }

        public void AddShipToHyperspaceGate(IShip ship)
        {

        }

        public IShip RemoveShipFromHyperspaceGate(IPlayer player)
        {
            return null;
        }

        public IHand GetHand(IPlayer player)
        {
            int index = this.GetPlayerIndex(player);
            return this.hands[index];
        }

        private int GetPlayerIndex(IPlayer player)
        {
            for (int i = 0; i < this.players.Length; i++)
            {
                if (this.players[i] == player)
                {
                    return i;
                }
            }
            return -1;
        }
        
        public IEnumerable<IPlanet> GetPlanets()
        {
            return this.planets;
        }

        public IEnumerable<IPlanet> GetPlanets(IPlayer player)
        {
            return this.planets.Where(planet => planet.Owner == player);
        }

        public Alien GetAlien(IPlayer player)
        {
            int playerIndex = this.GetPlayerIndex(player);
            return this.aliens[playerIndex];
        }

        public IEnumerable<IPlanet> GetPlanetsWithColony(params IPlayer[] players)
        {
            return players.SelectMany(player => this.planets.Where(planet => planet.GetShips(player).Any())).Distinct();
        }

        public void SetDefensivePlayer(IPlayer player)
        {
            this.defensePlayerIndex = this.GetPlayerIndex(player);
        }

        internal object GetPlanetsWithColony(object nonOffensePlayers)
        {
            throw new NotImplementedException();
        }
    }

    internal class Hand : List<ICard>, IHand
    {
        void IHand.Remove(ICard card)
        {
            this.Remove(card);
        }
    }

    abstract class ShipContainer : IShipContainer
    {
        private readonly List<IShip> ships = new List<IShip>();

        public void AddShip(IShip ship)
        {
            this.ships.Add(ship);
        }

        public IEnumerable<IShip> GetShips()
        {
            return this.ships;
        }

        public IEnumerable<IShip> GetShips(IPlayer activePlayer)
        {
            return ships.Where(ship => ship.Owner == activePlayer);
        }

        public IShip RemoveShip(IPlayer activePlayer)
        {
            var ship = this.GetShips(activePlayer).First();
            this.ships.Remove(ship);
            return ship;
        }
    }

    internal class Warp : ShipContainer, IWarp
    {
    }

    internal class HyperspaceGate : ShipContainer, IHyperspaceGate
    {
    }

    public interface IEncounterCardSelection : ISelection<IEncounterCard> { }

    public class PlanningPhaseParticipant
    {
        public IEncounterCardChoose Chooser { get; }
        public IEncounterCard SelectedCard { get; set; }
        public IEncounterCardSelection Selection { get; set; }
    }

    //public class DefaultPlanningPhase
    //{
    //    public void Execute(PlanningPhaseParticipant participants)
    //    {
    //        var offense = Task.Run(() => participants.Offense.Choose(offenseSelection.Selection));
    //        var defense = Task.Run(() => participants.Defense.Choose(defenseSelection.Selection));
    //        Task.WaitAll(offense, defense);
    //        encounter.OffenseCard = offense.Result;
    //        encounter.DefenseCard = defense.Result;
    //    }
    //}

    public interface IRandom
    {
        int Next(int max);
        int Next(int min, int max);
        T PickOne<T>(T[] items);

    }

    class Rand : IRandom
    {
        private readonly Random rand;

        public Rand(int seed)
        {
            this.rand = new Random(seed);
        }

        public int Next(int max)
        {
            return this.rand.Next(max);
        }

        public int Next(int min, int max)
        {
            return this.rand.Next(min, max);
        }

        public T PickOne<T>(T[] items)
        {
            int i = this.Next(items.Length);
            return items[i];
        }
    }

    public interface ICard
    {
    }

    public interface IEncounterCard : ICard
    {
        EncounterCardType Type { get; }
    }

    interface IArtifactCard : ICard
    {
    }

    interface IReinforcementCard : ICard
    {
    }

    class AttackEncounterCard : ICard
    {
        private readonly int value;

        public AttackEncounterCard(int value)
        {
            this.value = value;
        }

        public int Value { get { return this.value; } }
    }

    public class NegotiateEncounterCard : IEncounterCard
    {
        public EncounterCardType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class MorphEncounterCard : IEncounterCard
    {
        public EncounterCardType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    class ReinforcementCard : ICard
    {
        private readonly int modifier;

        public ReinforcementCard(int modifier)
        {
            this.modifier = modifier;
        }

        public int Modifier { get { return this.modifier; } }
    }

    class CardZapCard : ICard
    {
    }

    class CosmicZapCard : ICard
    {
    }

    class EmotionControlCard : ICard
    {
    }

    class ForceFieldCard : ICard
    {
    }

    class IonicGasCard : ICard
    {
    }

    class MobiusTubesCard : ICard
    {
    }

    class PlagueCard : ICard
    {
    }

    class QuashCard : ICard
    {
    }

    enum Victor
    {
        Offense,
        Defense,
        Both,
        Neither
    }

    interface IAttackResolutionRule
    {
        Victor Resolve(int offenseTotal, int defenseTotal);
    }

    interface IResolutionResult
    {

    }

    class DefaultOffenseWinResolutionResult : IResolutionResult
    {
        public void Resolve(IEncounter encounter)
        {
            encounter.EstablishOffenseColonies();
            encounter.DestroyDefenders();
        }
    }

    class DefaultDefensiveWinResolutionResult : IResolutionResult
    {

    }

    class DefaultAttackResolutionRule : IAttackResolutionRule
    {
        public Victor Resolve(int offenseTotal, int defenseTotal)
        {
            return (offenseTotal - defenseTotal) > 0 ? Victor.Offense : Victor.Defense;
        }
    }

    interface ICompensatedResolutionRule
    {
        Victor Resolve(bool offenseNegotiated, bool defenseNegotiated);
    }

    interface INegotiatedResolutionRule
    {
        Victor Resolve(bool dealReached);
    }

    class DefaultNegotiationResolutionRule : INegotiatedResolutionRule
    {
        public Victor Resolve(bool dealReached)
        {
            return dealReached ? Victor.Both : Victor.Neither;
        }
    }

    class DefaultCompensatedResolutionRule : ICompensatedResolutionRule
    {
        public Victor Resolve(bool offenseNegotiated, bool defenseNegotiated)
        {
            return offenseNegotiated ? Victor.Defense : Victor.Offense;
        }
    }

    //class Encounter
    //{
    //    public void Run()
    //    {
    //        IGame game = null;
    //        // start turn
    //        // regroup
    //        IPlayer offensePlayer = game.CurrentPlayer;
    //        IShip recoveredShip;
    //        if ((recoveredShip = offensePlayer.ShipsInWarp.FirstOrDefault()) != null)
    //        {
    //            IEnumerable<IColony> availableColonies = offensePlayer.Colonies;
    //            IColony colony = offensePlayer.PlaceShip(recoveredShip, availableColonies);
    //            game.MoveShipFromWarpToColony(recoveredShip, colony);
    //        }
    //        // destiny
    //        IPlayer defensePlayer = null;
    //        IColony targetColony = null;
    //        while (true)
    //        {
    //            IDestinyCard destinyCard = game.Destiny.Draw();
    //            defensePlayer = destinyCard.SelectPlayer(game);
    //            if (defensePlayer == offensePlayer)
    //            {
    //                if (offensePlayer.ShouldRedrawDestiny(game))
    //                {
    //                    // discard destiny
    //                    continue;
    //                }
    //                // if targeting own system, needs to choose a colony in system to target
    //                IEnumerable<IColony> intrudingColonies = offensePlayer.HomePlanets.SelectMany(planet => planet.Colonies).Where(colony => colony.Owner != offensePlayer);
    //                targetColony = offensePlayer.ChooseTargetColony(intrudingColonies, game);
    //                defensePlayer = targetColony.Owner;
    //                break;
    //            }
    //            //IPlanet defense.HomePlanets
    //        }
    //        // launch
    //        if (targetColony == null)
    //        {
    //            IPlanet targetPlanet = offensePlayer.ChooseTargetPlanet(defensePlayer.HomePlanets, game);
    //            targetColony = targetPlanet.Colonies.SingleOrDefault(colony => colony.Owner == defensePlayer) ?? new Colony(defensePlayer, 0);
    //        }
    //        IEnumerable<IColony> colonies = offensePlayer.ChooseShips(game);
    //        foreach (IShip colony in colonies)
    //        {
    //            game.MoveShipFromColonyToHyperspaceGate(colony);
    //        }
    //        // alliance
    //        // planning
    //        IEncounterCard offenseCard = offensePlayer.SelectEncounterCard();
    //        IEncounterCard defenseCard = defensePlayer.SelectEncounterCard();
    //        // reveal

    //        // resolution
    //        var t1 = offenseCard.Type;
    //        var t2 = defenseCard.Type;
    //        if (t1 == t2 || t1 == EncounterCardType.Morph || t2 == EncounterCardType.Morph)
    //        {
    //            if (t1 == EncounterCardType.Attack || t2 == EncounterCardType.Attack)
    //            {
    //                // resolve an attack
    //            }
    //            else if (t1 == EncounterCardType.Negotiate || t2 == EncounterCardType.Negotiate)
    //            {
    //                // handle negotiation
    //            }
    //            else
    //            {
    //                // two morphs, both sides lose
    //            }
    //        }
    //        else if (t1 == EncounterCardType.Negotiate)
    //        {
    //            // offense loses
    //        }
    //        else
    //        {
    //            Debug.Assert(t2 == EncounterCardType.Negotiate);
    //            // defense loses
    //        }
    //    }
    //}

    public class NormalDeck : Deck<ICard>
    {
        // 39 Attacks: 00, 01, 04 x4, 05, 06 x7, 07, 08 x7, 09, 10 x4, 11, 12 x2, 13, 14 x2, 15, 20 x2, 23, 30, 40
        private static readonly ICard[] DefaultCards = new ICard[] {
            new AttackEncounterCard(00),
            new AttackEncounterCard(01),
            new AttackEncounterCard(04), new AttackEncounterCard(04),
            new AttackEncounterCard(04), new AttackEncounterCard(04),
            new AttackEncounterCard(05),
            new AttackEncounterCard(06), new AttackEncounterCard(06),
            new AttackEncounterCard(06), new AttackEncounterCard(06),
            new AttackEncounterCard(06), new AttackEncounterCard(06), new AttackEncounterCard(06),
            new AttackEncounterCard(07),
            new AttackEncounterCard(08), new AttackEncounterCard(08),
            new AttackEncounterCard(08), new AttackEncounterCard(08),
            new AttackEncounterCard(08), new AttackEncounterCard(08), new AttackEncounterCard(08),
            new AttackEncounterCard(09),
            new AttackEncounterCard(10), new AttackEncounterCard(10),
            new AttackEncounterCard(10), new AttackEncounterCard(10),
            new AttackEncounterCard(11),
            new AttackEncounterCard(12), new AttackEncounterCard(12),
            new AttackEncounterCard(13),
            new AttackEncounterCard(14), new AttackEncounterCard(14),
            new AttackEncounterCard(15),
            new AttackEncounterCard(20), new AttackEncounterCard(20),
            new AttackEncounterCard(23),
            new AttackEncounterCard(30),
            new AttackEncounterCard(40),
            // 15 Negotiate cards
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            // 1 Morph card
            new MorphEncounterCard(),
            // 6 Reinforcements: +2 x2, +3 x3, +5
            new ReinforcementCard(2), new ReinforcementCard(2),
            new ReinforcementCard(3), new ReinforcementCard(3), new ReinforcementCard(3),
            new ReinforcementCard(5),
            // 11 Artifacts: Card Zap x2, Cosmic Zap x2, Emotion Control, Force Field, Ionic Gas, Mobius Tubes x2, Plague, Quash
            new CardZapCard(), new CardZapCard(),
            new CosmicZapCard(), new CosmicZapCard(),
            new EmotionControlCard(),
            new ForceFieldCard(),
            new IonicGasCard(),
            new MobiusTubesCard(), new MobiusTubesCard(),
            new PlagueCard(),
            new QuashCard(),
        };


        public NormalDeck(params ICard[] flares)
            : base(DefaultCards.Concat(flares).ToArray())
        {
        }
    }

    public class DefaultDestinyPhase
    {
        public void Do(GameState state)
        {
            while (true)
            {
                var destinyCard = state.DrawDestinyCard();
                var player = destinyCard.SelectPlayer(state);
                if (player == state.ActivePlayer)
                {
                    var externalColonies = state.GetPlanetsWithColony(state.NonOffensePlayers);
                    var playerPlanets = state.GetPlanets(state.ActivePlayer);
                    if (!playerPlanets.Any(planet => externalColonies.Contains(planet)) || !player.AcceptEncounterInHomeSystem())
                    {
                        continue;
                    }
                }
                state.SetDefensivePlayer(player);
                break;
            }
        }
    }

    public class DefaultRegroupPhase
    {
        public void Do(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var shipsInWarp = state.Warp.GetShips(activePlayer);
            if (shipsInWarp.Any())
            {
                var ship = state.Warp.RemoveShip(activePlayer);
                var colonies = state.GetPlanetsWithColony(activePlayer);
                IShipContainer target = state.HyperspaceGate;
                if (colonies.Any())
                {
                    target = activePlayer.SelectPlanetToPlaceShip(ship, colonies);
                }
                target.AddShip(ship);
            }
        }
    }

    public class GameSetup
    {
        public GameState Setup(params IPlayer[] players)
        {
            GameState state = new GameState();
            state.SetPlayers(players);
            Rand r = new Rand(0);
            // set up planets
            for (int i = 0; i < players.Length; i++)
            {
                for (int p = 0; p < 5; p++)
                {
                    var planet = new Planet(players[i]);
                    for (int s = 0; s < 4; s++)
                    {
                        planet.AddShip(new Ship(players[i]));
                    }
                    state.AddPlanet(planet);
                }
            }

            // choose aliens
            FlareDeck deck = new FlareDeck();
            deck.Shuffle(r);
            ICard[] flares = new ICard[10];
            for (int i = 0; i < flares.Length; i++)
            {
                flares[i] = deck.Draw();
            }
            Task<Alien>[] selectionTasks = new Task<Alien>[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                var x = i;
                Alien alien1 = ((NoOpFlareCard)flares[2 * i]).Alien;
                Alien alien2 = ((NoOpFlareCard)flares[2 * i + 1]).Alien;
                selectionTasks[i] = Task.Run(() => players[x].SelectAlien(alien1, alien2));
            }
            Task.WaitAll(selectionTasks);
            var aliens = selectionTasks.Select(task => task.Result).ToArray();
            state.SetAliens(aliens);
            // create deck
            NormalDeck normalDeck = new NormalDeck(flares);
            normalDeck.Shuffle(r);
            state.SetEncounterDeck(normalDeck);
            for (int i = 0; i < players.Length; i++)
            {
                for (int c = 0; c < 8; c++)
                {
                    state.DrawCardToHand(players[i]);
                }
            }
            // prepare destiny deck
            var destinyDeck = new DestinyDeck(players);
            destinyDeck.Shuffle(r);
            state.SetDestinyDeck(destinyDeck);
            // choose starting player
            IPlayer starting = r.PickOne(players);
            return state;
        }
    }

    internal class Ship : IShip
    {
        private IPlayer owner;

        public Ship(IPlayer owner)
        {
            this.owner = owner;
        }

        public IPlayer Owner { get { return this.owner; } }
    }

    internal class Planet : ShipContainer, IPlanet
    {
        private IPlayer owner;

        public IPlayer Owner { get { return this.owner; } }

        public Planet(IPlayer owner)
        {
            this.owner = owner;
        }
    }

    public enum EncounterCardType
    {
        Attack,
        Negotiate,
        Morph
    }

    //internal interface IEncounterCard
    //{
    //    EncounterCardType Type { get; set; }
    //}

    internal class Colony : IColony
    {
        private readonly IPlayer owner;
        private int v;

        public Colony(IPlayer defense, int v)
        {
            this.owner = defense;
            this.v = v;
        }

        public IPlayer Owner
        {
            get
            {
                return this.owner;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IShip> Ships
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public interface IDeck<T> where T : ICard
    {
        T Draw();
    }

    public interface IDestinyCard : ICard
    {
        IPlayer SelectPlayer(GameState game);
    }

    class ColorDestinyCard : IDestinyCard
    {
        private readonly IPlayer player;

        public ColorDestinyCard(IPlayer player)
        {
            this.player = player;
        }

        public IPlayer SelectPlayer(GameState game)
        {
            return this.player;
        }
    }

    public class DestinyDeck : Deck<IDestinyCard>
    {
        public DestinyDeck(params IPlayer[] players)
            : base(players.SelectMany(player => new IDestinyCard[]
            {
            new ColorDestinyCard(player),
            new ColorDestinyCard(player),
            new ColorDestinyCard(player)
            }).ToArray())
        {
        }
    }

    public interface IGame
    {
        IPlayer CurrentPlayer { get; set; }
        DestinyDeck Destiny { get; set; }
        int CurrentPlayerIndex { get; set; }

        void MoveShipFromWarpToColony(IShip ship, IColony colony);
        IEnumerable<IShip> Warp(IPlayer player);
        void MoveShipFromColonyToHyperspaceGate(IShip colony);
    }

    public interface IChoose<T>
    {
        T Choose(IEnumerable<T> choices);
    }

    public interface IEncounterCardChoose : IChoose<IEncounterCard>
    {
    }

    public interface IPlayer
    {
        Alien SelectAlien(params Alien[] aliens);
        IPlanet SelectPlanetToPlaceShip(IShip ship, IEnumerable<IPlanet> planets);

        //IEnumerable<IColony> Colonies { get; }
        //IEnumerable<IPlanet> HomePlanets { get; }
        //IEnumerable<IShip> ShipsInWarp { get; }

        //IEnumerable<IColony> ChooseShips(IGame game);
        //IColony ChooseTargetColony(IEnumerable<IColony> intrudingColonies, IGame game);
        //IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> homePlanets, IGame game);
        //void LoseShipToWarp(IShip ship);
        //IColony PlaceShip(IShip ship, IEnumerable<IColony> colonies);
        IEncounterCard SelectEncounterCard();
        bool AcceptEncounterInHomeSystem();
        //bool ShouldRedrawDestiny(IGame game);
    }

    public interface IShip
    {
        IPlayer Owner { get; }
    }

    public interface IColony
    {
        IPlayer Owner { get; set; }
        IEnumerable<IShip> Ships { get; set; }
    }

    interface IEncounter
    {
        void DestroyDefenders();
        void EstablishOffenseColonies();
    }
}
