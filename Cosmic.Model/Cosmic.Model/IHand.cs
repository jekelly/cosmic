using System.Collections.Generic;

namespace Cosmic.Model
{
    public interface IHand : IEnumerable<ICard>
    {
        int Count { get; }
        void Add(ICard card);
        bool Remove(ICard card);
    }
}
