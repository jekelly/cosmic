using System.Linq;
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
}
