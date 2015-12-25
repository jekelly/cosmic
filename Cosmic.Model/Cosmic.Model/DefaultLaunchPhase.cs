using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    public class DefaultLaunchPhase
    {
        public void Do(GameState state)
        {
            var offense = state.ActivePlayer;
            // The offense takes the hyperspace gate and points it at one planet in the system indicated by the drawn destiny card. 
            var planets = state.GetPlanets(state.DefensePlayer);
            var planet = offense.ChooseTargetPlanet(planets);
            state.HyperspaceGate.TargetPlanet = planet;
            /*
            The offense then takes one to four ships from any of his or her colonies, stacks them, and places them on the wide end of the
            hyperspace gate. The offense may take ships from his or her home colonies or foreign colonies. Ships may all be taken from
            the same colony or from different colonies. A player should be careful not to remove all of the ships from a colony, however,
            as he or she will lose the colony by doing so (see “Stripping a Planet of Ships” on page 13)
            */
            if (planet.Owner == offense)
            {
                var opponents = planet.GetShips().Select(ship => ship.Owner).Distinct().Where(player => player != offense);
                if (opponents.Any())
                {
                    var defense = offense.ChoosePlayerToAttack(opponents);
                    state.SetDefensivePlayer(defense);
                }
            }
            var action = new SelectShipsFromColoniesForHyperspaceGateAction(offense);
            action.Execute(state);
            //for (int i = 0; i < 4; i++)
            //{
            //    var ships = state.GetShipsOnPlanets(offense);
            //    if (!ships.Any())
            //    {
            //        break;
            //    }
            //    var ship = offense.ChooseShip(ships);
            //    if (ship == null)
            //    {
            //        if (i == 0)
            //        {
            //            i--;
            //            continue;
            //        }
            //        break;
            //    }
            //}
        }
    }
}
