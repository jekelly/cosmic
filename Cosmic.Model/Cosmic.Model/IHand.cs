using System.Collections.Generic;

namespace Cosmic.Model
{
    public interface IHand : IEnumerable<ICard>
    {
        void Add(ICard card);
        void Remove(ICard card);
    }
}
