using System.Collections.Generic;

namespace Cosmic.Model
{
    public class Hand : List<ICard>, IHand
    {
        void IHand.Remove(ICard card)
        {
            this.Remove(card);
        }
    }
}
