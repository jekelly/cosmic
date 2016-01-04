using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    public interface ICard
    {
    }

    public interface IEncounterCard : ICard
    {
        EncounterCardType Type { get; }
    }

    public interface IArtifactCard : ICard
    {
    }

    public interface IReinforcementCard : ICard
    {
    }

    public class AttackEncounterCard : IEncounterCard
    {
        private readonly int value;

        public AttackEncounterCard(int value)
        {
            this.value = value;
        }

        public EncounterCardType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Value { get { return this.value; } }

        public static explicit operator AttackEncounterCard(PlayedEncounterCard v)
        {
            throw new NotImplementedException();
        }
    }

    public class NegotiateEncounterCard : IEncounterCard
    {
        public EncounterCardType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class MorphEncounterCard : IEncounterCard
    {
        public EncounterCardType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    class ReinforcementCard : ICard
    {
        private readonly int modifier;

        public ReinforcementCard(int modifier)
        {
            this.modifier = modifier;
        }

        public int Modifier { get { return this.modifier; } }
    }

    class CardZapCard : ICard
    {
    }

    class CosmicZapCard : ICard
    {
    }

    class EmotionControlCard : ICard
    {
    }

    class ForceFieldCard : ICard
    {
    }

    class IonicGasCard : ICard
    {
    }

    class MobiusTubesCard : ICard
    {
    }

    class PlagueCard : ICard
    {
    }

    class QuashCard : ICard
    {
    }
}
