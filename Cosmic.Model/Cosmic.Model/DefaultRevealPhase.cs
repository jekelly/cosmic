using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cosmic.Model
{
    public enum EncounterResult
    {
        Success,
        Failure
    }

    [Flags]
    public enum EncounterOutcome
    {
        NoEffect,
        AllShipsToWarp,
        EstablishColony,
        CollectCompensation,
        DefenderRewards,
    }

    public class DefaultRevealPhase
    {
        private static readonly EncounterResults WinsAgainstAttack = new EncounterResults()
        {
            EncounterResult = EncounterResult.Success,
            OffenseOutcome = EncounterOutcome.EstablishColony,
            OffensiveAllyOutcome = EncounterOutcome.EstablishColony,
            DefenseOutcome = EncounterOutcome.AllShipsToWarp,
            DefensiveAllyOutcome = EncounterOutcome.AllShipsToWarp
        };
        private static readonly EncounterResults LosesAgainstAttack = new EncounterResults()
        {
            EncounterResult = EncounterResult.Failure,
            OffenseOutcome = EncounterOutcome.AllShipsToWarp,
            OffensiveAllyOutcome = EncounterOutcome.AllShipsToWarp,
            DefenseOutcome = EncounterOutcome.NoEffect,
            DefensiveAllyOutcome = EncounterOutcome.DefenderRewards,
        };
        private static readonly EncounterResults AttacksAgainstNegotiate = new EncounterResults()
        {
            EncounterResult = EncounterResult.Success,
            OffenseOutcome = EncounterOutcome.EstablishColony,
            OffensiveAllyOutcome = EncounterOutcome.EstablishColony,
            DefenseOutcome = EncounterOutcome.AllShipsToWarp | EncounterOutcome.CollectCompensation,
            DefensiveAllyOutcome = EncounterOutcome.AllShipsToWarp,
        };
        private static readonly EncounterResults NegotiatesAgainstAttack = new EncounterResults()
        {
            EncounterResult = EncounterResult.Failure,
            OffenseOutcome = EncounterOutcome.AllShipsToWarp | EncounterOutcome.CollectCompensation,
            OffensiveAllyOutcome = EncounterOutcome.AllShipsToWarp,
            DefenseOutcome = EncounterOutcome.NoEffect,
            DefensiveAllyOutcome = EncounterOutcome.DefenderRewards,
        };
        private static readonly EncounterResults DualMorph = new EncounterResults()
        {
            EncounterResult = EncounterResult.Failure,
            OffenseOutcome = EncounterOutcome.AllShipsToWarp,
            DefenseOutcome = EncounterOutcome.AllShipsToWarp,
            OffensiveAllyOutcome = EncounterOutcome.AllShipsToWarp,
            DefensiveAllyOutcome = EncounterOutcome.AllShipsToWarp
        };


        public void Do(GameState state)
        {
            var offense = state.ActivePlayer;
            var defense = state.DefensePlayer;
            // The offense and defense turn their cards faceup simultaneously and a winner is determined.
            state.RevealEncounterCards();
            // Determine outcome
            var offenseCard = state.GetPlayedEncounterCard(offense);
            var defenseCard = state.GetPlayedEncounterCard(defense);
            var offenseType = offenseCard.Card.Type;
            var defenseType = defenseCard.Card.Type;

            if (offenseType == EncounterCardType.Morph)
            {
                if (defenseType == EncounterCardType.Morph)
                {
                    DualMorph.Execute(state);
                    return;
                }
                offenseCard = defenseCard;
                offenseType = defenseType;
            }
            if (defenseType == EncounterCardType.Morph)
            {
                defenseCard = offenseCard;
                defenseType = offenseType;
            }
            if (offenseType == defenseType)
            {
                if (offenseType == EncounterCardType.Attack)
                {
                    // resolve attack
                    var hyperspaceGate = state.HyperspaceGate;
                    var offenseShips = hyperspaceGate.GetShips().Count();
                    var targetPlanet = hyperspaceGate.TargetPlanet;
                    var defenseShips = targetPlanet.GetShips(defense).Count() +
                        targetPlanet.AlliedDefenders.GetShips().Count();

                    var offenseTotal = offenseShips + ((AttackEncounterCard)offenseCard).Value;
                    var defenseTotal = defenseShips + ((AttackEncounterCard)defenseCard).Value;

                    if (offenseTotal > defenseTotal)
                    {
                        // offense win
                        WinsAgainstAttack.Execute(state);
                    }
                    else
                    {
                        // defense win
                        LosesAgainstAttack.Execute(state);
                    }
                }
                else
                {
                    Contract.Assert(offenseType == EncounterCardType.Negotiate);
                    // resolve negotiate
                    this.ResolveNegotiate(state);
                }
            }
            else if (offenseType == EncounterCardType.Attack)
            {
                Contract.Assert(defenseType == EncounterCardType.Negotiate);
                AttacksAgainstNegotiate.Execute(state);
            }
            else
            {
                Contract.Assert(offenseType == EncounterCardType.Negotiate);
                Contract.Assert(defenseType == EncounterCardType.Attack);
                NegotiatesAgainstAttack.Execute(state);
            }
        }

        private void ResolveNegotiate(GameState state)
        {
            throw new NotImplementedException();
        }

        class EncounterResults
        {
            public EncounterOutcome OffenseOutcome { get; set; }
            public EncounterOutcome OffensiveAllyOutcome { get; set; }
            public EncounterOutcome DefenseOutcome { get; set; }
            public EncounterOutcome DefensiveAllyOutcome { get; set; }
            public EncounterResult EncounterResult { get; set; }

            public void Execute(GameState state)
            {
                var offense = state.ActivePlayer;
                var defense = state.DefensePlayer;

                state.SetEncounterOutcome(offense, this.OffenseOutcome);
                foreach (var ally in state.OffensiveAllies)
                {
                    state.SetEncounterOutcome(ally, this.OffensiveAllyOutcome);
                }
                state.SetEncounterOutcome(defense, this.DefenseOutcome);
                foreach (var ally in state.DefensiveAllies)
                {
                    state.SetEncounterOutcome(ally, this.DefensiveAllyOutcome);
                }
                state.SetEncounterResult(this.EncounterResult);
            }
        }
    }
}
