using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    // TODO: unnecessary
    public interface IShip
    {
        IPlayer Owner { get; }
    }

    public class Ship : IShip
    {
        private IPlayer owner;

        public Ship(IPlayer owner)
        {
            this.owner = owner;
        }

        public IPlayer Owner { get { return this.owner; } }
    }
}
