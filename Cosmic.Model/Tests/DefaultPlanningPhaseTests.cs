using System.Linq;
using Cosmic.Model;
using FluentAssertions;
using Xunit;

namespace Tests
{
    public class DefaultPlanningPhaseTests
    {
        /*
        The offense and the defense now each select an encounter card
        from their hand (an attack, negotiate, or morph) and place it
        facedown in front of themselves. If the defense has no encounter
        cards in hand, he or she may reveal any remaining cards
        in hand, discard them, and then draw a new eight-card hand
        before selecting a card during this phase. If the offense has no
        encounter cards in hand, his or her turn ends immediately, as
        described under “Drawing New Cards” on page 13.
        */
        [Fact]
        public void EncounterCards_AreSelected()
        {
            var fixture = new BothAttackFixture();
            fixture.Execute();
            fixture.game.GetPlayedEncounterCard(fixture.offense).Card.Should().Be(fixture.attack);
            fixture.game.GetPlayedEncounterCard(fixture.defense).Card.Should().Be(fixture.attack);

        }

        [Fact]
        public void EncounterCards_AreSelectedInSecret()
        {
            var fixture = new BothAttackFixture();
            fixture.Execute();
            fixture.game.GetPlayedEncounterCard(fixture.offense).Visible.Should().BeFalse();
            fixture.game.GetPlayedEncounterCard(fixture.defense).Visible.Should().BeFalse();
        }

        [Fact]
        public void DefenseDrawsNewHand_IfNoEncounterCardsInHand()
        {
            var fixture = new NoEncounterCardsDefense();
            var originalHand = fixture.game.GetHand(fixture.game.DefensePlayer).ToArray();
            fixture.Execute();
            var newHand = fixture.game.GetHand(fixture.game.DefensePlayer);
            foreach (var oldCard in originalHand)
            {
                newHand.Should().NotContain(oldCard);
            }
            newHand.Count.Should().Be(7);
        }

        [Fact(Skip = "Need a way to switch phases/active players on the game model")]
        public void OffenseTurnEnds_IfNoCardsInHand()
        {
            // TODO
        }

        class NoEncounterCardsDefense : BaseFixture
        {
            public NoEncounterCardsDefense()
            {
                this.game.SetEncounterDeck(new NormalDeck());
                this.game.AddCardToHand(this.offense, this.attack);
                this.game.AddCardToHand(this.defense, this.artifactCard);
                this.game.AddCardToHand(this.defense, this.flareCard);
                this.game.AddCardToHand(this.defense, this.reinforcementsCard);
            }
        }

        class BothAttackFixture : BaseFixture
        {
            public BothAttackFixture()
            {
                this.game.AddCardToHand(this.offense, this.attack);
                this.game.AddCardToHand(this.defense, this.attack);
                this.offense.EncounterCardChooser = (h) => this.attack;
                this.defense.EncounterCardChooser = (h) => this.attack;
            }
        }

        abstract class BaseFixture
        {
            public readonly GameState game = new GameState();
            public readonly ChooserPlayer offense = new ChooserPlayer();
            public readonly ChooserPlayer defense = new ChooserPlayer();

            public readonly AttackEncounterCard attack = new AttackEncounterCard(1234);
            public readonly NegotiateEncounterCard negotiate = new NegotiateEncounterCard();
            public readonly MorphEncounterCard morph = new MorphEncounterCard();

            public readonly IArtifactCard artifactCard = new TestArtifactCard();
            public readonly ICard flareCard = new TestFlareCard();
            public readonly IReinforcementCard reinforcementsCard = new TestReinforcementsCard();

            public readonly DefaultPlanningPhase sut = new DefaultPlanningPhase();

            public void Execute()
            {
                sut.Do(this.game);
            }

            public BaseFixture()
            {
                this.offense.EncounterCardChooser = (h) => h.First();
                this.defense.EncounterCardChooser = (h) => h.First();
                game.SetPlayers(this.offense, this.defense);
                game.SetDefensivePlayer(this.defense);
            }

            public class TestArtifactCard : IArtifactCard
            {
            }

            public class TestFlareCard : ICard
            {
            }

            public class TestReinforcementsCard : IReinforcementCard
            {
            }
        }
    }
}
