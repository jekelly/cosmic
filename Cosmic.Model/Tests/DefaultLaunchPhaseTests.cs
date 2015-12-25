using System;
using System.Linq;
using Cosmic.Model;
using FluentAssertions;
using Xunit;

namespace Tests
{
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
                this.playerPlanet = new Planet(player);
                this.opponentPlanet = new Planet(opponent);
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
}
