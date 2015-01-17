using System;
using System.Collections.Generic;

namespace Barak.Benchmark.Presentation.ViewModels
{
    public class EqualityPredicate<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> m_Equals;
        private Func<T, int> m_GetHash;

        #region IEqualityComparer<T> Members

        public EqualityPredicate(Func<T, T, bool> equals, Func<T, int> getHash)
        {
            m_GetHash = getHash;
            m_Equals = equals;
        }

        public bool Equals(T x, T y)
        {
            return m_Equals.Invoke(x, y);
        }

        public int GetHashCode(T obj)
        {
            return m_GetHash.Invoke(obj);
        }

        #endregion
    }
}