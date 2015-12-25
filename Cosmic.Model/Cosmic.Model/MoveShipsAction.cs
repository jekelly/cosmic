using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    class MoveShipsAction
    {
        public IPlayer ActingPlayer { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public Func<GameState, IEnumerable<IShip>> Source { get; set; }
        public Action<GameState, IShip> Sink { get; set; }

        public void Execute(GameState state)
        {
            for (int i = 0; i < this.Max; i++)
            {
                var ships = this.Source(state);
                if (!ships.Any())
                {
                    break;
                }
                var ship = this.ActingPlayer.ChooseShip(ships);
                if (ship == null)
                {
                    if (i < this.Min)
                    {
                        i--;
                        continue;
                    }
                    break;
                }
                this.Sink(state, ship);
            }
        }
    }
}
