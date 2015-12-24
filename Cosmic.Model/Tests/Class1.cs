﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cosmic.Model;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Tests
{
    public class CardsTests
    {
        [Fact]
        public void ShouldBe39AttackCards()
        {
            Cards.AttackCards.Count().Should().Be(39);
        }
    }

    public class FlareDeckTests
    {
        private readonly IRandom r = new TestRandom();

        [Fact]
        public void Test()
        {
            FlareDeck fd = new FlareDeck();
            fd.Shuffle(r);
            fd.Draw().Should().NotBeNull();
        }
    }

    public class DefaultRegroupPhaseTests
    {
        private readonly RegroupTestsFixture fixture = new RegroupTestsFixture();

        [Fact]
        public void NoShipsInWarp_NoOp()
        {
            var phase = new DefaultRegroupPhase();
            phase.Do(fixture.game);
            fixture.game.Warp.GetShips().Count().Should().Be(0);
        }

        [Fact]
        public void OneShipInWarp_RemovesOne()
        {
            IShip ship = fixture.CreateShipInWarp();
            fixture.AddPlanetWithShips(shipCount: 1);
            var phase = new DefaultRegroupPhase();
            phase.Do(fixture.game);
            fixture.game.Warp.GetShips().Should().NotContain(ship);
        }

        [Fact]
        public void OwnsColony_PlacesOnPreferredColony()
        {
            IShip ship = fixture.CreateShipInWarp();
            fixture.AddPlanetWithShips(shipCount: 1);
            var planet = fixture.AddPreferedPlanetWithShips(shipCount: 1);
            var phase = new DefaultRegroupPhase();
            phase.Do(fixture.game);
            planet.GetShips(fixture.testPlayer).Should().Contain(ship);
        }

        [Fact]
        public void NoColony_PlacesInHyperspaceGate()
        {
            fixture.AddPlanet();
            var ship = fixture.CreateShipInWarp();
            var phase = new DefaultRegroupPhase();
            phase.Do(fixture.game);
            fixture.game.HyperspaceGate.GetShips(fixture.testPlayer).Should().Contain(ship);
        }

        class RegroupTestsFixture
        {
            public readonly TestPlayer testPlayer = new TestPlayer();
            public readonly GameState game = new GameState();

            public RegroupTestsFixture()
            {
                this.game.SetPlayers(this.testPlayer);
            }

            public TestPlanet AddPlanet()
            {
                TestPlanet planet = new TestPlanet() { Owner = this.testPlayer };
                this.game.AddPlanet(planet);
                return planet;
            }

            public TestPlanet AddPreferedPlanet()
            {
                var planet = this.AddPlanet();
                this.testPlayer.PreferredPlanet = planet;
                return planet;
            }

            public TestShip CreateTestShip()
            {
                return new TestShip() { Owner = this.testPlayer };
            }

            public IShip CreateShipInWarp()
            {
                var ship = this.CreateTestShip();
                this.game.Warp.AddShip(ship);
                return ship;
            }

            public TestPlanet AddPreferedPlanetWithShips(int shipCount)
            {
                var planet = this.AddPreferedPlanet();
                for (int i = 0; i < shipCount; i++)
                {
                    planet.AddShip(this.CreateTestShip());
                }
                return planet;
            }

            public TestPlanet AddPlanetWithShips(int shipCount)
            {
                var planet = this.AddPlanet();
                for (int i = 0; i < shipCount; i++)
                {
                    planet.AddShip(this.CreateTestShip());
                }
                return planet;
            }
        }

    }

    class TestShip : IShip
    {
        public IPlayer Owner { get; set; }
    }

    class TestPlanet : IPlanet
    {
        private readonly List<IShip> ships = new List<IShip>();

        public IPlayer Owner { get; set; }

        public void AddShip(IShip ship)
        {
            this.ships.Add(ship);
        }

        public IEnumerable<IShip> GetShips()
        {
            return this.ships;
        }

        public IEnumerable<IShip> GetShips(IPlayer player)
        {
            return this.ships;
        }

        public void RemoveShip(IShip ship)
        {
            this.ships.Remove(ship);
        }
    }

    public class DefaultLaunchPhaseTests
    {
        /// <summary>
        /// Two players, each with one planet with 5 ships on it.
        /// Offense will launch at opponent with 1 ship.
        /// </summary>
        class SingleShipToOpponentPlanetLaunchPhaseFixture
        {
            public readonly GameState game;
            public readonly IPlanet playerPlanet;
            public readonly IPlanet opponentPlanet;
            public readonly ChooserPlayer player;
            public readonly ChooserPlayer opponent;
            public readonly IShip[] playerShips;
            public readonly IShip[] opponentShips;

            public SingleShipToOpponentPlanetLaunchPhaseFixture()
            {
                var player = new ChooserPlayer();
                var opponent = new ChooserPlayer();
                this.playerPlanet = new TestPlanet() { Owner = player };
                this.opponentPlanet = new TestPlanet() { Owner = opponent };
                this.playerShips = new IShip[5];
                this.opponentShips = new IShip[5];
                for (int i = 0; i < 5; i++)
                {
                    this.playerShips[i] = new TestShip() { Owner = player };
                    this.opponentShips[i] = new TestShip() { Owner = opponent };
                    this.playerPlanet.AddShip(this.playerShips[i]);
                    this.opponentPlanet.AddShip(this.opponentShips[i]);
                }
                this.player = player;
                this.opponent = opponent;
                this.game = new GameState();
                this.game.SetPlayers(player, opponent);
                this.game.AddPlanet(playerPlanet);
                this.game.AddPlanet(opponentPlanet);
                int count = 0;
                this.player.ShipChooser = () =>
                {
                    return count++ == 0 ? this.playerShips[0] : null;
                };
                this.player.PlanetChooser = () => this.opponentPlanet;
            }
        }

        [Fact]
        public void PlayerWithNoColonies_DoesNotGetPromptedForShips()
        {
            var fixture = new NoColoniesThrowsOnShipChoiceFixture();
            DefaultLaunchPhase phase = new DefaultLaunchPhase();
            new Action(() => phase.Do(fixture.game)).ShouldNotThrow();
        }

        class NoColoniesThrowsOnShipChoiceFixture : SingleShipToOpponentPlanetLaunchPhaseFixture
        {
            public NoColoniesThrowsOnShipChoiceFixture()
            {
                this.game.RemovePlanet(this.playerPlanet);
                this.player.ShipChooser = () => { throw new Exception(); };
            }
        }

        [Fact]
        public void ShipChoice_PlacedInHyperspaceGate()
        {
            var fixture = new SingleShipToOpponentPlanetLaunchPhaseFixture();
            DefaultLaunchPhase phase = new DefaultLaunchPhase();
            phase.Do(fixture.game);
            fixture.game.HyperspaceGate.GetShips().Should().Contain(fixture.playerShips[0]);
        }

        [Fact]
        public void ShipChoice_RemovedFromSourcePlanet()
        {
            var fixture = new SingleShipToOpponentPlanetLaunchPhaseFixture();
            DefaultLaunchPhase phase = new DefaultLaunchPhase();
            phase.Do(fixture.game);
            fixture.playerPlanet.GetShips().Should().NotContain(fixture.playerShips[0]);
        }

        [Fact]
        public void EncounterInHomeSystem_MayTargetEmptyPlanet()
        {
            var fixture = new HomeSystemEmptyPlanetFixture();
            DefaultLaunchPhase phase = new DefaultLaunchPhase();
            phase.Do(fixture.game);
            fixture.game.HyperspaceGate.TargetPlanet.Should().Be(fixture.playerPlanet);
        }

        [Fact]
        public void EncounterInHomeSystem_ChoosesOpponent()
        {
            var fixture = new HomeSystemOpponentColonyFixture();
            DefaultLaunchPhase phase = new DefaultLaunchPhase();
            phase.Do(fixture.game);
            fixture.game.DefensePlayer.Should().Be(fixture.opponent);
        }

        [Fact]
        public void HyperGateChoice_IsRespected()
        {
            var fixture = new SingleShipToOpponentPlanetLaunchPhaseFixture();
            DefaultLaunchPhase phase = new DefaultLaunchPhase();
            phase.Do(fixture.game);
            fixture.game.HyperspaceGate.TargetPlanet.Should().Be(fixture.opponentPlanet);
        }

        [Fact]
        public void MinimumOfOneShip_Enforced()
        {
            var fixture = new IndecisivePlayerFixture();
            DefaultLaunchPhase phase = new DefaultLaunchPhase();
            phase.Do(fixture.game);
            fixture.count.Should().Be(3);
            fixture.game.HyperspaceGate.GetShips().Should().Contain(fixture.playerShips[0]);
            fixture.playerPlanet.GetShips().Should().NotContain(fixture.playerShips[0]);
        }

        [Fact]
        public void MaximumOfFourShips_Enforced()
        {
            var fixture = new OverzealousPlayerFixture();
            DefaultLaunchPhase phase = new DefaultLaunchPhase();
            phase.Do(fixture.game);
            fixture.count.Should().Be(4);
            foreach (var s in fixture.playerShips.Take(4))
            {
                fixture.game.HyperspaceGate.GetShips().Should().Contain(s);
                fixture.playerPlanet.GetShips().Should().NotContain(s);
            }
            fixture.game.HyperspaceGate.GetShips().Should().NotContain(fixture.playerShips[4]);
            fixture.playerPlanet.GetShips().Should().Contain(fixture.playerShips[4]);
        }

        private class ChooserPlayer : NullPlayer
        {
            public Func<IPlanet> PlanetChooser { get; set; }
            public Func<ChooserPlayer> PlayerChooser { get; set; }
            public Func<IShip> ShipChooser { get; set; }

            public override IPlayer ChoosePlayerToAttack(IEnumerable<IPlayer> players)
            {
                return this?.PlayerChooser();
            }

            public override IShip ChooseShip(IEnumerable<IShip> ships)
            {
                return this?.ShipChooser();
            }

            public override IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> planets)
            {
                return this?.PlanetChooser();
            }

            public override IPlanet SelectPlanetToPlaceShip(IShip ship, IEnumerable<IPlanet> planets)
            {
                return this?.PlanetChooser();
            }
        }

        class HomeSystemOpponentColonyFixture : SingleShipToOpponentPlanetLaunchPhaseFixture
        {
            public HomeSystemOpponentColonyFixture()
            {
                this.playerPlanet.AddShip(new TestShip() { Owner = this.opponent });
                this.game.SetDefensivePlayer(this.player);
                this.player.PlanetChooser = () => this.playerPlanet;
                this.player.PlayerChooser = () => this.opponent;
            }
        }

        class HomeSystemEmptyPlanetFixture : SingleShipToOpponentPlanetLaunchPhaseFixture
        {
            public HomeSystemEmptyPlanetFixture()
            {
                this.game.SetDefensivePlayer(this.player);
                this.player.PlanetChooser = () => this.playerPlanet;
            }
        }

        class IndecisivePlayerFixture : SingleShipToOpponentPlanetLaunchPhaseFixture
        {
            public int count;

            public IndecisivePlayerFixture()
            {
                this.player.ShipChooser = () =>
                {
                    try
                    {
                        if (count == 0) return null;
                        if (count == 1) return this.playerShips[0];
                        return null;
                    }
                    finally
                    {
                        count++;
                    }
                };
            }
        }

        class OverzealousPlayerFixture : SingleShipToOpponentPlanetLaunchPhaseFixture
        {
            public int count;

            public OverzealousPlayerFixture()
            {
                this.player.ShipChooser = () =>
                {
                    return this.playerShips[this.count++];
                };
            }
        }
    }

    class TargetablePlayer : NullPlayer
    {
        public IShip TargetShip { get; set; }
        public IPlanet TargetPlanet { get; set; }
        public IPlayer TargetPlayer { get; set; }

        public override IShip ChooseShip(IEnumerable<IShip> ships)
        {
            return this.TargetShip;
        }

        public override IPlayer ChoosePlayerToAttack(IEnumerable<IPlayer> players)
        {
            return this.TargetPlayer;
        }

        public override IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> planets)
        {
            return this.TargetPlanet;
        }
    }

    class NullPlayer : IPlayer
    {
        public virtual bool AcceptEncounterInHomeSystem()
        {
            return false;
        }

        public virtual IPlayer ChoosePlayerToAttack(IEnumerable<IPlayer> players)
        {
            return null;
        }

        public virtual IShip ChooseShip(IEnumerable<IShip> ships)
        {
            return null;
        }

        public virtual IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> planets)
        {
            return null;
        }

        public virtual Alien SelectAlien(params Alien[] aliens)
        {
            return null;
        }

        public virtual IEncounterCard SelectEncounterCard()
        {
            return null;
        }

        public virtual IPlanet SelectPlanetToPlaceShip(IShip ship, IEnumerable<IPlanet> planets)
        {
            return null;
        }
    }

    public class DefaultDestinyPhaseTests
    {
        private readonly DefaultDestinyPhaseFixture fixture = new DefaultDestinyPhaseFixture();

        [Fact]
        public void DestinyInOpponentSystem_EncountersOpponent()
        {
            fixture.PushDestinyCardForOpponent();
            DefaultDestinyPhase phase = new DefaultDestinyPhase();
            phase.Do(fixture.game);
            fixture.game.DefensePlayer.Should().Be(fixture.opponent);
        }

        [Fact]
        public void DestinyInOwnSystem_AllowsSkip()
        {
            fixture.PushDestinyCardForOpponent();
            fixture.PushDestinyCardForPlayer();
            DefaultDestinyPhase phase = new DefaultDestinyPhase();
            phase.Do(fixture.game);
            fixture.game.DefensePlayer.Should().Be(fixture.opponent);
        }

        [Fact]
        public void CannotSelectHomeSystem_IfNoOpposingColonies()
        {
            fixture.PushDestinyCardForOpponent();
            fixture.PushDestinyCardForPlayer();
            fixture.player.AcceptHomeSystemEncounters = true;
            DefaultDestinyPhase phase = new DefaultDestinyPhase();
            phase.Do(fixture.game);
            fixture.game.DefensePlayer.Should().Be(fixture.opponent);
        }

        [Fact]
        public void CanSelectHomeSystem_IfOpponentsHaveColonyThere()
        {
            fixture.AddOpponentShipToPlayerPlanet();
            fixture.PushDestinyCardForOpponent();
            fixture.PushDestinyCardForPlayer();
            fixture.player.AcceptHomeSystemEncounters = true;
            DefaultDestinyPhase phase = new DefaultDestinyPhase();
            phase.Do(fixture.game);
            fixture.game.DefensePlayer.Should().Be(fixture.player);
        }

        class DefaultDestinyPhaseFixture
        {
            public readonly TestPlayer player = new TestPlayer();
            public readonly TestPlayer opponent = new TestPlayer();
            public readonly GameState game = new GameState();
            public readonly DestinyDeck deck = new DestinyDeck();
            public readonly TestPlanet playerPlanet = new TestPlanet();

            public DefaultDestinyPhaseFixture()
            {
                this.playerPlanet.Owner = this.player;
                this.game.SetPlayers(this.player, this.opponent);
                this.game.SetDestinyDeck(this.deck);
                this.game.AddPlanet(playerPlanet);
            }

            public void AddOpponentShipToPlayerPlanet()
            {
                this.playerPlanet.AddShip(new TestShip() { Owner = this.opponent });
            }

            public void PushDestinyCardForPlayer()
            {
                this.deck.PutCardOnTop(new TestDestinyCard(this.player));
            }

            public void PushDestinyCardForOpponent()
            {
                this.deck.PutCardOnTop(new TestDestinyCard(this.opponent));
            }

            class TestDestinyCard : IDestinyCard
            {
                private readonly IPlayer player;

                public TestDestinyCard(IPlayer player)
                {
                    this.player = player;
                }

                public IPlayer SelectPlayer(GameState game)
                {
                    return this.player;
                }
            }
        }
    }

    public class GameSetupTests
    {
        private readonly IPlayer testPlayer = new TestPlayer();
        private readonly GameSetup setup = new GameSetup();

        public GameSetupTests()
        {
        }

        [Fact]
        public void PlanetsCreated()
        {
            var game = setup.Setup(testPlayer);
            game.GetPlanets(testPlayer).Count().Should().Be(5);
        }

        [Fact]
        public void ShipsCreated()
        {
            var game = setup.Setup(testPlayer);
            game.GetPlanets(testPlayer).Sum(p => p.GetShips(testPlayer).Count()).Should().Be(20);
        }

        [Fact]
        public void HandDrawn()
        {
            var game = setup.Setup(testPlayer);
            game.GetHand(testPlayer).Count().Should().Be(8);
        }

        [Fact]
        public void DestinyDeck()
        {
            var game = setup.Setup(testPlayer);
            var card = game.DrawDestinyCard();
            card.SelectPlayer(game).Should().Be(testPlayer);
        }

        [Fact]
        public void AliensPicked()
        {
            var game = setup.Setup(testPlayer);
            var alien = game.GetAlien(testPlayer);
            alien.Should().Be(AlienDefinitions.Aliens[36]);
        }

        [Fact]
        public void StartingPlayer()
        {
            var game = setup.Setup(testPlayer);
            game.ActivePlayer.Should().Be(testPlayer);
        }
    }

    class TestPlayer : IPlayer
    {
        public Alien SelectAlien(params Alien[] aliens)
        {
            return aliens[0];
        }

        public IPlanet PreferredPlanet { get; set; }
        public IShip PreferredShip { get; set; }

        public bool AcceptHomeSystemEncounters { get; set; }

        public IPlanet SelectPlanetToPlaceShip(IShip ship, IEnumerable<IPlanet> planets)
        {
            return this.PreferredPlanet ?? planets.First();
        }

        public IEncounterCard SelectEncounterCard()
        {
            throw new NotImplementedException();
        }

        public bool AcceptEncounterInHomeSystem()
        {
            return this.AcceptHomeSystemEncounters;
        }

        public IPlanet ChooseTargetPlanet(IEnumerable<IPlanet> planets)
        {
            return this.PreferredPlanet ?? planets.First();
        }

        public IShip ChooseShip(IEnumerable<IShip> ships)
        {
            return this.PreferredShip ?? ships.First();
        }

        public IPlayer ChoosePlayerToAttack(IEnumerable<IPlayer> players)
        {
            throw new NotImplementedException();
        }
    }

    //public class DefaultPlanningPhaseTests
    //{
    //    [Fact]
    //    public void Test()
    //    {
    //        DefaultPlanningPhase phase = new DefaultPlanningPhase();
    //    }
    //}

    public class DeckTests
    {
        private readonly IRandom r = new TestRandom();
        private readonly TestCard card1 = new TestCard();
        private readonly TestCard card2 = new TestCard();

        [Fact]
        public void ReturnsTopCard()
        {
            Deck<ICard> deck = new Deck<ICard>(card1, card2);
            deck.Draw().Should().Be(card1);
            deck.Draw().Should().Be(card2);
        }

        [Fact]
        public void ThrowsIfEmpty()
        {
            Deck<ICard> d = new Deck<ICard>();
            new Action(() => d.Draw()).ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void ShuffleReorders()
        {
            Deck<ICard> deck = new Deck<ICard>(card1, card2);
            deck.Shuffle(r);
            deck.Draw().Should().Be(card2);
            deck.Draw().Should().Be(card1);
        }

        class TestCard : ICard { }
    }
    class TestRandom : IRandom
    {
        public int Next(int max)
        {
            return 0;
        }

        public int Next(int min, int max)
        {
            return max - 1;
        }

        public T PickOne<T>(T[] items)
        {
            return items[0];
        }
    }
}
