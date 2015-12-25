using System.Linq;
using Cosmic.Model;
using FluentAssertions;
using Xunit;

namespace Tests
{
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
}
