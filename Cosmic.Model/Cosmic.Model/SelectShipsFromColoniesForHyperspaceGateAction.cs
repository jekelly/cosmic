using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    class SelectShipsFromColoniesForHyperspaceGateAction : MoveShipsAction
    {
        public SelectShipsFromColoniesForHyperspaceGateAction(IPlayer player)
        {
            this.ActingPlayer = player;
            this.Max = 4;
            this.Min = 1;
            this.Source = state => state.GetShipsOnPlanets(player);
            this.Sink = (state, ship) => state.AddShipToHyperspaceGate(ship);
        }
    }
}
