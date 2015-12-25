using System.Linq;
using Cosmic.Model;
using FluentAssertions;
using Xunit;

namespace Tests
{
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

            public Planet AddPlanet()
            {
                Planet planet = new Planet(this.testPlayer);
                this.game.AddPlanet(planet);
                return planet;
            }

            public Planet AddPreferedPlanet()
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

            public Planet AddPreferedPlanetWithShips(int shipCount)
            {
                var planet = this.AddPreferedPlanet();
                for (int i = 0; i < shipCount; i++)
                {
                    planet.AddShip(this.CreateTestShip());
                }
                return planet;
            }

            public Planet AddPlanetWithShips(int shipCount)
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
}
