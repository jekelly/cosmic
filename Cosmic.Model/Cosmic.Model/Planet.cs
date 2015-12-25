using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
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
}
