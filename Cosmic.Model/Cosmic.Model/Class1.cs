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
        void RemoveShip(IShip player);
    }

    public interface IHyperspaceGate : IShipContainer
    {
        IPlanet TargetPlanet { get; set; }
    }

    public interface IWarp : IShipContainer
    {
    }

    public interface IPlanet : IShipContainer
    {
        IShipContainer AlliedDefenders { get; }
        IPlayer Owner { get; }
    }

    [Flags]
    public enum Alliance
    {
        Neither = 0x0,
        Offense = 0x1,
        Defense = 0x2,
    }

    public class DefaultAlliancePhase
    {
        public void Do(GameState state)
        {
            // First, the offense announces which players he or she wishes to
            // have as allies.The offense may not invite the defense as an ally.
            // These players should not respond to the offense’s invitation yet.
            var offense = state.ActivePlayer;
            var defense = state.DefensePlayer;
            var potentialAllies = state.GetPossibleAllies();
            var offenseInvites = new HashSet<IPlayer>(offense.InviteOffensiveAllies(potentialAllies));
            // Next, the defense invites allies. He or she may invite any players
            // (except the offense) to be allies, even those already invited
            // by the offense.
            var defenseInvites = new HashSet<IPlayer>(defense.InviteDefensiveAllies(potentialAllies));
            // Once allies are invited, players other than the offense and
            // defense choose sides.Starting with the player to the left of
            // the offense and continuing clockwise, each player accepts or
            // declines invitations to ally. A player may only ally with either
            // the offense or the defense – not both. A player may choose to
            // ally with neither side.
            foreach (var potentialAlly in potentialAllies)
            {
                List<Alliance> choices = new List<Alliance>();
                choices.Add(Alliance.Neither);
                if (offenseInvites.Contains(potentialAlly))
                {
                    choices.Add(Alliance.Offense);
                }
                if (defenseInvites.Contains(potentialAlly))
                {
                    choices.Add(Alliance.Defense);
                }
                if (choices.Count == 1)
                {
                    continue;
                }
                var allianceChoice = potentialAlly.ChooseAllianceSide(choices);
                if (allianceChoice.HasFlag(Alliance.Offense))
                {
                    // If a player allies with the offense, the allying player places one
                    // to four of his or her ships (taken from any colonies) on the
                    // hyperspace gate. A player allied with the offense is referred to
                    // as an offensive ally.
                    var action = new SelectShipsFromColoniesForHyperspaceGateAction(potentialAlly);
                    action.Execute(state);
                    state.AddOffensiveAlly(potentialAlly);
                }
                if (allianceChoice.HasFlag(Alliance.Defense))
                {
                    // If a player allies with the defense, the allying player places one
                    // to four of his or her ships (taken from any colonies) next to,
                    // but not on, the targeted planet. A player allied with the defense
                    // is referred to as a defensive ally.
                    var action = new SelectShipsFromColoniesToAidDefenseOfPlanet(potentialAlly, state.HyperspaceGate.TargetPlanet);
                    action.Execute(state);
                    state.AddDefensiveAlly(potentialAlly);
                }
            }
        }
    }

    class SelectShipsFromColoniesToAidDefenseOfPlanet : MoveShipsAction
    {
        public SelectShipsFromColoniesToAidDefenseOfPlanet(IPlayer player, IPlanet targetPlanet)
        {
            this.ActingPlayer = player;
            this.Min = 1;
            this.Max = 4;
            this.Source = state => state.GetShipsOnPlanets(player);
            this.Sink = (state, ship) => targetPlanet.AlliedDefenders.AddShip(ship);
        }
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
        private readonly List<IPlayer> offensiveAllies;
        private readonly List<IPlayer> defensiveAllies;

        public GameState()
        {
            this.warp = new Warp();
            this.gate = new HyperspaceGate();
            this.planets = new List<IPlanet>();
            this.offensiveAllies = new List<IPlayer>();
            this.defensiveAllies = new List<IPlayer>();
        }

        public IPlayer ActivePlayer { get { return this.players[this.activePlayerIndex]; } }

        public IWarp Warp { get { return this.warp; } }

        public IHyperspaceGate HyperspaceGate { get { return this.gate; } }

        public IPlayer DefensePlayer { get { return this.players[this.defensePlayerIndex]; } }

        public IPlayer[] Players { get { return this.players; } }
        public IPlayer[] NonOffensePlayers { get { return this.players.Where(player => player != this.ActivePlayer).ToArray(); } }

        public IEnumerable<IPlayer> OffensiveAllies { get { return this.offensiveAllies; } }
        public IEnumerable<IPlayer> DefensiveAllies { get { return this.defensiveAllies; } }

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

        public void RemovePlanet(IPlanet planet)
        {
            this.planets.Remove(planet);
        }

        public void AddShipToHyperspaceGate(IShip ship)
        {
            var colony = this.GetPlanetsWithColony(ship.Owner).Where(planet => planet.GetShips().Contains(ship)).Single();
            colony.RemoveShip(ship);
            this.gate.AddShip(ship);
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

        public IEnumerable<IShip> GetShipsOnPlanets(IPlayer player)
        {
            var planets = this.GetPlanets(player);
            return planets.SelectMany(planet => planet.GetShips(player));
        }

        public IEnumerable<IPlayer> GetPossibleAllies()
        {
            return this.players.Where(player => player != this.ActivePlayer && player != this.DefensePlayer);
        }

        public void AddOffensiveAlly(IPlayer ally)
        {
            this.offensiveAllies.Add(ally);
        }

        public void AddDefensiveAlly(IPlayer ally)
        {
            this.defensiveAllies.Add(ally);
        }
    }

    internal class Hand : List<ICard>, IHand
    {
        void IHand.Remove(ICard card)
        {
            this.Remove(card);
        }
    }

    public abstract class ShipContainer : IShipContainer
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

        public void RemoveShip(IShip ship)
        {
            this.ships.Remove(ship);
        }
    }

    internal class Warp : ShipContainer, IWarp
    {
    }

    internal class HyperspaceGate : ShipContainer, IHyperspaceGate
    {
        public IPlanet TargetPlanet { get; set; }
    }

    public interface IEncounterCardSelection : ISelection<IEncounterCard> { }

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

    class MoveShipsAction
    {
        public IPlayer ActingPlayer { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public Func<GameState, IEnumerable<IShip>> Source { get; set; }
        public Action<GameState, IShip> Sink { get; set; }

        public void Execute(GameState state)
        {
            for (int i = 0; i < this.Max; i++)
            {
                var ships = this.Source(state);
                if (!ships.Any())
                {
                    break;
                }
                var ship = this.ActingPlayer.ChooseShip(ships);
                if (ship == null)
                {
                    if (i < this.Min)
                    {
                        i--;
                        continue;
                    }
                    break;
                }
                this.Sink(state, ship);
            }
        }
    }

    class SelectShipsFromColoniesForHyperspaceGateAction : MoveShipsAction
    {
        public SelectShipsFromColoniesForHyperspaceGateAction(IPlayer player)
        {
            this.ActingPlayer = player;
            this.Max = 4;
            this.Min = 1;
            this.Source = state => state.GetShipsOnPlanets(player);
            this.Sink = (state, ship) => state.AddShipToHyperspaceGate(ship);
        }
    }

    public class DefaultLaunchPhase
    {
        public void Do(GameState state)
        {
            var offense = state.ActivePlayer;
            // The offense takes the hyperspace gate and points it at one planet in the system indicated by the drawn destiny card. 
            var planets = state.GetPlanets(state.DefensePlayer);
            var planet = offense.ChooseTargetPlanet(planets);
            state.HyperspaceGate.TargetPlanet = planet;
            /*
            The offense then takes one to four ships from any of his or her colonies, stacks them, and places them on the wide end of the
            hyperspace gate. The offense may take ships from his or her home colonies or foreign colonies. Ships may all be taken from
            the same colony or from different colonies. A player should be careful not to remove all of the ships from a colony, however,
            as he or she will lose the colony by doing so (see “Stripping a Planet of Ships” on page 13)
            */
            if (planet.Owner == offense)
            {
                var opponents = planet.GetShips().Select(ship => ship.Owner).Distinct().Where(player => player != offense);
                if (opponents.Any())
                {
                    var defense = offense.ChoosePlayerToAttack(opponents);
                    state.SetDefensivePlayer(defense);
                }
            }
            var action = new SelectShipsFromColoniesForHyperspaceGateAction(offense);
            action.Execute(state);
            //for (int i = 0; i < 4; i++)
            //{
            //    var ships = state.GetShipsOnPlanets(offense);
            //    if (!ships.Any())
            //    {
            //        break;
            //    }
            //    var ship = offense.ChooseShip(ships);
            //    if (ship == null)
            //    {
            //        if (i == 0)
            //        {
            //            i--;
            //            continue;
            //        }
            //        break;
            //    }
            //}
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
                var ships = state.Warp.GetShips(activePlayer);
                var ship = activePlayer.ChooseShip(ships);
                var colonies = state.GetPlanetsWithColony(activePlayer);
                IShipContainer target = state.HyperspaceGate;
                if (colonies.Any())
                {
                    target = activePlayer.SelectPlanetToPlaceShip(ship, colonies);
                }
                state.Warp.RemoveShip(ship);
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

    public class Planet : ShipContainer, IPlanet
    {
        public IPlayer Owner { get; set; }

        public IShipContainer AlliedDefenders { get; private set; }

        public Planet()
        {
            this.AlliedDefenders = new DefenderShips();
        }

        public Planet(IPlayer owner) : this()
        {
            this.Owner = owner;
        }
    }

    public class DefenderShips : ShipContainer
    {

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
        IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> planets);
        IShip ChooseShip(IEnumerable<IShip> ships);
        IPlayer ChoosePlayerToAttack(IEnumerable<IPlayer> players);
        Alliance ChooseAllianceSide(IEnumerable<Alliance> choices);
        IEnumerable<IPlayer> InviteOffensiveAllies(IEnumerable<IPlayer> invited);
        IEnumerable<IPlayer> InviteDefensiveAllies(IEnumerable<IPlayer> invited);
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
