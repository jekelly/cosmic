using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    public class DefaultPlanningPhase
    {
        public void Do(GameState state)
        {
            var offense = state.ActivePlayer;
            var defense = state.DefensePlayer;

            var offenseHand = state.GetHand(offense).Where(card => card is IEncounterCard).Cast<IEncounterCard>();
            var defenseHand = state.GetHand(defense).Where(card => card is IEncounterCard).Cast<IEncounterCard>();

            if (!offenseHand.Any())
            {
                // TODO
            }
            if (!defenseHand.Any())
            {
                state.DrawNewHand(defense);
                defenseHand = state.GetHand(defense).Where(card => card is IEncounterCard).Cast<IEncounterCard>();
            }

            Task.WaitAll(
                this.ChooseCardAsync(state, offense, offenseHand), 
                this.ChooseCardAsync(state, defense, defenseHand));
        }

        private Task ChooseCardAsync(GameState state, IPlayer player, IEnumerable<IEncounterCard> hand)
        {
            return
                Task.Run(() => player.ChooseEncounterCard(hand))
                .ContinueWith(t =>
                {
                    state.SetEncounterCard(player, t.Result);
                });
        }
    }
}
