using System;
using System.Linq;
using Cosmic.Model;
using FluentAssertions;
using Xunit;

namespace Tests
{
    public class DefaultAlliancePhaseTests
    {
        /// <summary>
        /// Six players, with one planet each, 5 ships on each planet.
        /// One player is offense, one player is defense. Both invite all but one player.
        /// One allies with offense, one allies with defense,
        /// one declines to ally and one would ally with either but is not invited.
        /// Max ships are contributed in all cases.
        /// </summary>
        class DefaultAlliancePhaseTestFixture
        {
            const int PlayerCount = 6;
            public GameState game = new GameState();
            public ChooserPlayer[] players = new ChooserPlayer[PlayerCount];
            public Planet[] planets = new Planet[PlayerCount];
            public IShip[][] ships = new IShip[PlayerCount][];

            public ChooserPlayer offense { get { return this.players[0]; } }
            public ChooserPlayer defense { get { return this.players[1]; } }
            public ChooserPlayer offensiveAlly { get { return this.players[2]; } }
            public ChooserPlayer defensiveAlly { get { return this.players[3]; } }
            public ChooserPlayer neutralAlly { get { return this.players[4]; } }
            public ChooserPlayer shunnedAlly { get { return this.players[5]; } }

            public DefaultAlliancePhaseTestFixture()
            {
                for (int i = 0; i < PlayerCount; i++)
                {
                    this.players[i] = new ChooserPlayer();
                    this.planets[i] = new Planet(this.players[i]);
                    this.ships[i] = new IShip[5];
                    for (int s = 0; s < 5; s++)
                    {
                        this.ships[i][s] = new TestShip() { Owner = this.players[i] };
                        this.planets[i].AddShip(this.ships[i][s]);
                    }
                    int c = 0;
                    int p = i;
                    this.players[i].ShipChooser = () => this.ships[p][c++];
                    this.game.AddPlanet(this.planets[i]);
                }
                this.offense.OffensiveAllyInviteChooser = (i) => i.Where(p => p != this.shunnedAlly);
                this.defense.DefensiveAllyInviteChooser = (i) => i.Where(p => p != this.shunnedAlly);
                this.offensiveAlly.AllianceChooser = () => Alliance.Offense;
                this.defensiveAlly.AllianceChooser = () => Alliance.Defense;
                this.shunnedAlly.AllianceChooser = () => Alliance.Offense | Alliance.Defense;

                game.SetPlayers(this.players);
                game.SetDefensivePlayer(this.defense);
                game.HyperspaceGate.TargetPlanet = this.planets[1];
            }
        }

        [Fact]
        public void CorrectPlayers_IdentifiedAsPotentialAllies()
        {
            var fixture = new DefaultAlliancePhaseTestFixture();
            var allies = fixture.game.GetPossibleAllies();
            allies.Should().Contain(fixture.offensiveAlly);
            allies.Should().Contain(fixture.defensiveAlly);
            allies.Should().Contain(fixture.neutralAlly);
            allies.Should().NotContain(fixture.offense);
            allies.Should().NotContain(fixture.defense);
        }

        [Fact]
        public void Offense_InvitesFirst()
        {
            bool offenseAsked = false;
            bool defenseAsked = false;
            var fixture = new DefaultAlliancePhaseTestFixture();
            fixture.offense.OffensiveAllyInviteChooser = (i) =>
            {
                offenseAsked = true;
                if (defenseAsked)
                {
                    throw new Exception();
                }
                return i.Where(p => p != fixture.shunnedAlly);
            };
            fixture.defense.DefensiveAllyInviteChooser = (i) => { defenseAsked = true; return i.Where(p => p != fixture.shunnedAlly); };
            DefaultAlliancePhase phase = new DefaultAlliancePhase();
            new Action(() => phase.Do(fixture.game)).ShouldNotThrow<Exception>();
            offenseAsked.Should().BeTrue();
        }

        [Fact]
        public void ShunnedAllies_DoNotContributeShips()
        {
            var fixture = new DefaultAlliancePhaseTestFixture();
            DefaultAlliancePhase phase = new DefaultAlliancePhase();
            phase.Do(fixture.game);
            fixture.game.HyperspaceGate.GetShips(fixture.shunnedAlly).Count().Should().Be(0);
            fixture.game.HyperspaceGate.TargetPlanet.AlliedDefenders.GetShips(fixture.shunnedAlly).Count().Should().Be(0);
        }

        [Fact]
        public void ShunnedAllies_AreNotAllied()
        {
            var fixture = new DefaultAlliancePhaseTestFixture();
            DefaultAlliancePhase phase = new DefaultAlliancePhase();
            phase.Do(fixture.game);
            fixture.game.OffensiveAllies.Should().NotContain(fixture.shunnedAlly);
            fixture.game.DefensiveAllies.Should().NotContain(fixture.shunnedAlly);
        }

        [Fact]
        public void NeutralPlayers_DoNotContributeShips()
        {
            var fixture = new DefaultAlliancePhaseTestFixture();
            DefaultAlliancePhase phase = new DefaultAlliancePhase();
            phase.Do(fixture.game);
            fixture.game.HyperspaceGate.GetShips(fixture.neutralAlly).Count().Should().Be(0);
            fixture.game.HyperspaceGate.TargetPlanet.AlliedDefenders.GetShips(fixture.neutralAlly).Count().Should().Be(0);
        }

        [Fact]
        public void NeutralPlayers_AreNotAllied()
        {
            var fixture = new DefaultAlliancePhaseTestFixture();
            DefaultAlliancePhase phase = new DefaultAlliancePhase();
            phase.Do(fixture.game);
            fixture.game.OffensiveAllies.Should().NotContain(fixture.neutralAlly);
            fixture.game.DefensiveAllies.Should().NotContain(fixture.neutralAlly);
        }

        [Fact]
        public void OffensiveAllies_ContributeShipsToGate()
        {
            var fixture = new DefaultAlliancePhaseTestFixture();
            DefaultAlliancePhase phase = new DefaultAlliancePhase();
            phase.Do(fixture.game);
            fixture.game.OffensiveAllies.Should().Contain(fixture.offensiveAlly);
            fixture.game.HyperspaceGate.GetShips(fixture.offensiveAlly).Count().Should().Be(4);
        }

        [Fact]
        public void DefensiveAllies_ContributeShipsToPlanetDefenses()
        {
            var fixture = new DefaultAlliancePhaseTestFixture();
            DefaultAlliancePhase phase = new DefaultAlliancePhase();
            fixture.game.HyperspaceGate.TargetPlanet.GetShips(fixture.defensiveAlly).Count().Should().Be(0);
            phase.Do(fixture.game);
            fixture.game.DefensiveAllies.Should().Contain(fixture.defensiveAlly);
            fixture.game.HyperspaceGate.TargetPlanet.AlliedDefenders.GetShips(fixture.defensiveAlly).Count().Should().Be(4);
        }
    }
}
