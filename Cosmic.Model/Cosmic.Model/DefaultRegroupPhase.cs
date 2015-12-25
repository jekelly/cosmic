using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    public class DefaultRegroupPhase
    {
        public void Do(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var shipsInWarp = state.Warp.GetShips(activePlayer);
            if (shipsInWarp.Any())
            {
                var ships = state.Warp.GetShips(activePlayer);
                var ship = activePlayer.ChooseShip(ships);
                var colonies = state.GetPlanetsWithColony(activePlayer);
                IShipContainer target = state.HyperspaceGate;
                if (colonies.Any())
                {
                    target = activePlayer.SelectPlanetToPlaceShip(ship, colonies);
                }
                state.Warp.RemoveShip(ship);
                target.AddShip(ship);
            }
        }
    }
}
