using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    public class DefaultAlliancePhase
    {
        public void Do(GameState state)
        {
            // First, the offense announces which players he or she wishes to
            // have as allies.The offense may not invite the defense as an ally.
            // These players should not respond to the offense’s invitation yet.
            var offense = state.ActivePlayer;
            var defense = state.DefensePlayer;
            var potentialAllies = state.GetPossibleAllies();
            var offenseInvites = new HashSet<IPlayer>(offense.InviteOffensiveAllies(potentialAllies));
            // Next, the defense invites allies. He or she may invite any players
            // (except the offense) to be allies, even those already invited
            // by the offense.
            var defenseInvites = new HashSet<IPlayer>(defense.InviteDefensiveAllies(potentialAllies));
            // Once allies are invited, players other than the offense and
            // defense choose sides.Starting with the player to the left of
            // the offense and continuing clockwise, each player accepts or
            // declines invitations to ally. A player may only ally with either
            // the offense or the defense – not both. A player may choose to
            // ally with neither side.
            foreach (var potentialAlly in potentialAllies)
            {
                List<Alliance> choices = new List<Alliance>();
                choices.Add(Alliance.Neither);
                if (offenseInvites.Contains(potentialAlly))
                {
                    choices.Add(Alliance.Offense);
                }
                if (defenseInvites.Contains(potentialAlly))
                {
                    choices.Add(Alliance.Defense);
                }
                if (choices.Count == 1)
                {
                    continue;
                }
                var allianceChoice = potentialAlly.ChooseAllianceSide(choices);
                if (allianceChoice.HasFlag(Alliance.Offense))
                {
                    // If a player allies with the offense, the allying player places one
                    // to four of his or her ships (taken from any colonies) on the
                    // hyperspace gate. A player allied with the offense is referred to
                    // as an offensive ally.
                    var action = new SelectShipsFromColoniesForHyperspaceGateAction(potentialAlly);
                    action.Execute(state);
                    state.AddOffensiveAlly(potentialAlly);
                }
                if (allianceChoice.HasFlag(Alliance.Defense))
                {
                    // If a player allies with the defense, the allying player places one
                    // to four of his or her ships (taken from any colonies) next to,
                    // but not on, the targeted planet. A player allied with the defense
                    // is referred to as a defensive ally.
                    var action = new SelectShipsFromColoniesToAidDefenseOfPlanet(potentialAlly, state.HyperspaceGate.TargetPlanet);
                    action.Execute(state);
                    state.AddDefensiveAlly(potentialAlly);
                }
            }
        }
    }
}
