using System;
using System.Collections.Generic;

namespace Cosmic.Model
{
    public class Deck<T> where T : ICard
    {
        private readonly List<T> cards;
        private int index;

        public Deck(params T[] cards)
        {
            this.cards = new List<T>(cards);
            this.index = 0;
        }

        public ICard Draw()
        {
            return this.cards[this.index++];
        }

        public void Shuffle(IRandom r)
        {
            for (int i = this.index; i < this.cards.Count; i++)
            {
                int newI = r.Next(i, this.cards.Count);
                T temp = this.cards[i];
                this.cards[i] = this.cards[newI];
                this.cards[newI] = temp;
            }
        }

        public void PutCardOnTop(T card)
        {
            this.cards.Insert(this.index, card);
        }
    }
}
