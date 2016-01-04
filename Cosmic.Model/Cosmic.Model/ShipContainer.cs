using System.Collections.Generic;
using System.Linq;

namespace Cosmic.Model
{
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

        public bool RemoveShip(IShip ship)
        {
            return this.ships.Remove(ship);
        }
    }

    internal class Warp : ShipContainer, IWarp
    {
    }

    internal class HyperspaceGate : ShipContainer, IHyperspaceGate
    {
        public IPlanet TargetPlanet { get; set; }
    }
}
