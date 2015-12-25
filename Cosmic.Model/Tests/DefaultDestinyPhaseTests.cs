using Cosmic.Model;
using FluentAssertions;
using Xunit;

namespace Tests
{
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
            public readonly Planet playerPlanet = new Planet();

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
}
