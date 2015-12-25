using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Model
{
    public interface IRandom
    {
        int Next(int max);
        int Next(int min, int max);
        T PickOne<T>(T[] items);
    }

    class Rand : IRandom
    {
        private readonly Random rand;

        public Rand(int seed)
        {
            this.rand = new Random(seed);
        }

        public int Next(int max)
        {
            return this.rand.Next(max);
        }

        public int Next(int min, int max)
        {
            return this.rand.Next(min, max);
        }

        public T PickOne<T>(T[] items)
        {
            int i = this.Next(items.Length);
            return items[i];
        }
    }
}
