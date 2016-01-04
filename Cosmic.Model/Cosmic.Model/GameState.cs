using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmic.Model
{
    public class PlayedEncounterCard
    {
        public IEncounterCard Card;
        public bool Visible;
    }

    public enum GamePhase
    {
        Regroup,
        Destiny,
        Launch,
        Alliance,
        Planning,
        Reveal,
        Resolution
    }

    public class GameState
    {
        private int activePlayerIndex;
        private int defensePlayerIndex;
        private readonly IHyperspaceGate gate;
        private GamePhase currentPhase;

        public EncounterOutcome GetEncounterOutcome(IPlayer player)
        {
            int playerIndex = this.GetPlayerIndex(player);
            return this.encounterOutcomes[playerIndex];
        }

        private readonly IWarp warp;
        private DestinyDeck destinyDeck;
        private readonly List<ICard> discardPile;

        public void MoveShipToWarp(IShipContainer source, IShip ship)
        {
            if (!source.RemoveShip(ship))
            {
                throw new InvalidOperationException("Cannot remove a ship from a source that does not contain it.");
            }
            this.warp.AddShip(ship);
        }

        private IPlayer[] players;

        private Alien[] aliens;
        private IHand[] hands;
        private List<IPlanet> planets;

        private NormalDeck encounterDeck;

        public void RevealEncounterCards()
        {
            foreach (var card in this.playedEncounterCards)
            {
                if (card != null)
                {
                    card.Visible = true;
                }
            }
        }

        public void TakeCompensation(IPlayer player, IPlayer opponent, int count)
        {
            var playerHand = this.GetHand(player);
            var opponentHand = this.GetHand(opponent);
            int cards = Math.Min(opponentHand.Count, count);
            for (int i = 0; i < cards; i++)
            {
                // TODO: randomize
                var card = opponentHand.ElementAt(0);
                opponentHand.Remove(card);
                playerHand.Add(card);
            }
        }

        public void DiscardHand(IPlayer player)
        {
            var hand = this.GetHand(player);
            foreach (var card in hand.ToArray())
            {
                hand.Remove(card);
                this.DiscardCard(card);
            }
        }

        public void DrawNewHand(IPlayer player)
        {
            this.DiscardHand(player);
            for (int i = 0; i < 8; i++)
            {
                this.DrawCardToHand(player);
            }
        }

        private readonly List<IPlayer> offensiveAllies;
        private readonly List<IPlayer> defensiveAllies;

        private PlayedEncounterCard[] playedEncounterCards;

        public GameState()
        {
            this.warp = new Warp();
            this.gate = new HyperspaceGate();
            this.planets = new List<IPlanet>();
            this.offensiveAllies = new List<IPlayer>();
            this.defensiveAllies = new List<IPlayer>();
            this.discardPile = new List<ICard>();
        }

        public PlayedEncounterCard GetPlayedEncounterCard(IPlayer player)
        {
            int index = this.GetPlayerIndex(player);
            return this.playedEncounterCards[index];
        }

        public EncounterResult GetEncounterResult()
        {
            return this.encounterResult;
        }

        public void SetNextPlayer()
        {
            int index = this.activePlayerIndex;
            int next = (index + 1) % this.players.Length;
            this.activePlayerIndex = next;
        }

        public void StartNewEncounter()
        {
            this.currentPhase = GamePhase.Regroup;
        }

        public void StartNewTurn()
        {
            this.StartNewEncounter();
        }

        public void RunPhase()
        {
            switch (this.currentPhase)
            {
                case GamePhase.Regroup:
                    break;
                case GamePhase.Destiny:
                    break;
                case GamePhase.Launch:
                    break;
                case GamePhase.Alliance:
                    break;
                case GamePhase.Planning:
                    break;
                case GamePhase.Reveal:
                    break;
                case GamePhase.Resolution:
                    break;
            }
            this.currentPhase = (this.currentPhase + 1);
        }


        private EncounterOutcome[] encounterOutcomes;

        public void SetEncounterOutcome(IPlayer player, EncounterOutcome outcome)
        {
            int playerIndex = this.GetPlayerIndex(player);
            this.encounterOutcomes[playerIndex] = outcome;
        }

        public void SetEncounterCard(IPlayer player, IEncounterCard result)
        {
            int playerIndex = this.GetPlayerIndex(player);
            this.playedEncounterCards[playerIndex] = new PlayedEncounterCard()
            {
                Card = result,
                Visible = false,
            };
            var hand = this.GetHand(player);
            if (!hand.Remove(result))
            {
                throw new InvalidOperationException("Cannot select a card that isn't in hand.");
            }
        }

        private EncounterResult encounterResult;

        public void SetEncounterResult(EncounterResult encounterResult)
        {
            this.encounterResult = encounterResult;
        }

        public IPlayer ActivePlayer { get { return this.players[this.activePlayerIndex]; } }

        public IWarp Warp { get { return this.warp; } }

        public IHyperspaceGate HyperspaceGate { get { return this.gate; } }

        public IPlayer DefensePlayer { get { return this.players[this.defensePlayerIndex]; } }

        public IPlayer[] Players { get { return this.players; } }
        public IPlayer[] NonOffensePlayers { get { return this.players.Where(player => player != this.ActivePlayer).ToArray(); } }

        public IEnumerable<IPlayer> OffensiveAllies { get { return this.offensiveAllies; } }
        public IEnumerable<IPlayer> DefensiveAllies { get { return this.defensiveAllies; } }

        public IEnumerable<IPlayer> PlayersInResolutionOrder
        {
            get
            {
                yield return this.ActivePlayer;
                yield return this.DefensePlayer;
                for (int i = this.activePlayerIndex; 
                    i != this.activePlayerIndex - 1;
                    i = (i + 1) % this.players.Length)
                {
                    if (i == this.defensePlayerIndex) continue;
                    yield return this.players[i];
                }
            }
        }

        public int EncounterNumber { get; private set; }

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
            this.playedEncounterCards = new PlayedEncounterCard[players.Length];
            this.encounterOutcomes = new EncounterOutcome[players.Length];
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
}
