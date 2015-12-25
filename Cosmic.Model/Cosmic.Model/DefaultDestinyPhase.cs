using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    public class DefaultDestinyPhase
    {
        public void Do(GameState state)
        {
            while (true)
            {
                var destinyCard = state.DrawDestinyCard();
                var player = destinyCard.SelectPlayer(state);
                if (player == state.ActivePlayer)
                {
                    var externalColonies = state.GetPlanetsWithColony(state.NonOffensePlayers);
                    var playerPlanets = state.GetPlanets(state.ActivePlayer);
                    if (!playerPlanets.Any(planet => externalColonies.Contains(planet)) || !player.AcceptEncounterInHomeSystem())
                    {
                        continue;
                    }
                }
                state.SetDefensivePlayer(player);
                break;
            }
        }
    }
}
