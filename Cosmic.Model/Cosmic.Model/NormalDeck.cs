using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    public class NormalDeck : Deck<ICard>
    {
        // 39 Attacks: 00, 01, 04 x4, 05, 06 x7, 07, 08 x7, 09, 10 x4, 11, 12 x2, 13, 14 x2, 15, 20 x2, 23, 30, 40
        private static readonly ICard[] DefaultCards = new ICard[] {
            new AttackEncounterCard(00),
            new AttackEncounterCard(01),
            new AttackEncounterCard(04), new AttackEncounterCard(04),
            new AttackEncounterCard(04), new AttackEncounterCard(04),
            new AttackEncounterCard(05),
            new AttackEncounterCard(06), new AttackEncounterCard(06),
            new AttackEncounterCard(06), new AttackEncounterCard(06),
            new AttackEncounterCard(06), new AttackEncounterCard(06), new AttackEncounterCard(06),
            new AttackEncounterCard(07),
            new AttackEncounterCard(08), new AttackEncounterCard(08),
            new AttackEncounterCard(08), new AttackEncounterCard(08),
            new AttackEncounterCard(08), new AttackEncounterCard(08), new AttackEncounterCard(08),
            new AttackEncounterCard(09),
            new AttackEncounterCard(10), new AttackEncounterCard(10),
            new AttackEncounterCard(10), new AttackEncounterCard(10),
            new AttackEncounterCard(11),
            new AttackEncounterCard(12), new AttackEncounterCard(12),
            new AttackEncounterCard(13),
            new AttackEncounterCard(14), new AttackEncounterCard(14),
            new AttackEncounterCard(15),
            new AttackEncounterCard(20), new AttackEncounterCard(20),
            new AttackEncounterCard(23),
            new AttackEncounterCard(30),
            new AttackEncounterCard(40),
            // 15 Negotiate cards
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            new NegotiateEncounterCard(), new NegotiateEncounterCard(), new NegotiateEncounterCard(),
            // 1 Morph card
            new MorphEncounterCard(),
            // 6 Reinforcements: +2 x2, +3 x3, +5
            new ReinforcementCard(2), new ReinforcementCard(2),
            new ReinforcementCard(3), new ReinforcementCard(3), new ReinforcementCard(3),
            new ReinforcementCard(5),
            // 11 Artifacts: Card Zap x2, Cosmic Zap x2, Emotion Control, Force Field, Ionic Gas, Mobius Tubes x2, Plague, Quash
            new CardZapCard(), new CardZapCard(),
            new CosmicZapCard(), new CosmicZapCard(),
            new EmotionControlCard(),
            new ForceFieldCard(),
            new IonicGasCard(),
            new MobiusTubesCard(), new MobiusTubesCard(),
            new PlagueCard(),
            new QuashCard(),
        };


        public NormalDeck(params ICard[] flares)
            : base(DefaultCards.Concat(flares).ToArray())
        {
        }
    }
}
