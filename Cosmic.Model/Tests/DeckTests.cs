using System;
using Cosmic.Model;
using FluentAssertions;
using Xunit;

namespace Tests
{
    public class DeckTests
    {
        private readonly IRandom r = new TestRandom();
        private readonly TestCard card1 = new TestCard();
        private readonly TestCard card2 = new TestCard();

        [Fact]
        public void ReturnsTopCard()
        {
            Deck<ICard> deck = new Deck<ICard>(card1, card2);
            deck.Draw().Should().Be(card1);
            deck.Draw().Should().Be(card2);
        }

        [Fact]
        public void ThrowsIfEmpty()
        {
            Deck<ICard> d = new Deck<ICard>();
            new Action(() => d.Draw()).ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void ShuffleReorders()
        {
            Deck<ICard> deck = new Deck<ICard>(card1, card2);
            deck.Shuffle(r);
            deck.Draw().Should().Be(card2);
            deck.Draw().Should().Be(card1);
        }

        class TestCard : ICard { }
    }
}
