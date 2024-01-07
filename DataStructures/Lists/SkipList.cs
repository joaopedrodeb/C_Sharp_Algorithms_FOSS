/***
 * The Skip-List data structure implementation. 
 * 
 * Uses the custom class SkipListNode<T>.
 * 
 * Note: A more memory efficient implementation would use structs, but this is the simplest implementation possible.
 */
using System;
using System.Collections.Generic;
using DataStructures.Common;

namespace DataStructures.Lists
{
    public class SkipList<T> : IEnumerable<T> where T : IComparable<T>
    {
        private int Count { get; set; }
        private int CurrentMaxLevel { get; set; }
        private Random Randomizer { get; set; }

        private readonly int MaxLevel = 32;
        private readonly double Probability = 0.5;

        private SkipListNode<T> FirstNode { get; set; }

        public SkipList()
        {
            InitializeSkipList();
        }

        private void InitializeSkipList()
        {
            Count = 0;
            CurrentMaxLevel = 1;
            Randomizer = new Random();
            FirstNode = new SkipListNode<T>(default(T), MaxLevel);

            for (int i = 0; i < MaxLevel; ++i)
                FirstNode.Forwards[i] = FirstNode;
        }

        private int GetNextLevel()
        {
            int level = 0;

            while (Randomizer.NextDouble() < Probability && level <= CurrentMaxLevel && level < MaxLevel)
                ++level;

            return level;
        }

        public void Add(T item)
        {
            var current = FirstNode;
            var toBeUpdated = new SkipListNode<T>[MaxLevel];

            for (int i = CurrentMaxLevel - 1; i >= 0; --i)
            {
                while (current.Forwards[i] != FirstNode && current.Forwards[i].Value.IsLessThan(item))
                    current = current.Forwards[i];

                toBeUpdated[i] = current;
            }

            current = current.Forwards[0];

            int level = GetNextLevel();
            UpdateListLevel(toBeUpdated, level);

            var newNode = new SkipListNode<T>(item, level);

            for (int i = 0; i < level; ++i)
            {
                newNode.Forwards[i] = toBeUpdated[i].Forwards[i];
                toBeUpdated[i].Forwards[i] = newNode;
            }

            ++Count;
        }

        private void UpdateListLevel(SkipListNode<T>[] toBeUpdated, int newLevel)
        {
            if (newLevel > CurrentMaxLevel)
            {
                for (int i = CurrentMaxLevel; i < newLevel; ++i)
                    toBeUpdated[i] = FirstNode;

                CurrentMaxLevel = newLevel;
            }
        }

        public bool Remove(T item)
        {
            T deleted;
            return Remove(item, out deleted);
        }

        public bool Remove(T item, out T deleted)
        {
            var current = FirstNode;
            var toBeUpdated = new SkipListNode<T>[MaxLevel];

            for (int i = CurrentMaxLevel - 1; i >= 0; --i)
            {
                while (current.Forwards[i] != FirstNode && current.Forwards[i].Value.IsLessThan(item))
                    current = current.Forwards[i];

                toBeUpdated[i] = current;
            }

            current = current.Forwards[0];

            if (!current.Value.IsEqualTo(item))
            {
                deleted = default(T);
                return false;
            }

            for (int i = 0; i < CurrentMaxLevel; ++i)
                if (toBeUpdated[i].Forwards[i] == current)
                    toBeUpdated[i].Forwards[i] = current.Forwards[i];

            --Count;

            while (CurrentMaxLevel > 1 && FirstNode.Forwards[CurrentMaxLevel - 1] == FirstNode)
                --CurrentMaxLevel;

            deleted = current.Value;
            return true;
        }

        public bool Contains(T item)
        {
            T itemOut;
            return Find(item, out itemOut);
        }

        public bool Find(T item, out T result)
        {
            var current = FirstNode;

            for (int i = CurrentMaxLevel - 1; i >= 0; --i)
                while (current.Forwards[i] != FirstNode && current.Forwards[i].Value.IsLessThan(item))
                    current = current.Forwards[i];

            current = current.Forwards[0];

            if (current.Value.IsEqualTo(item))
            {
                result = current.Value;
                return true;
            }

            result = default(T);
            return false;
        }

        public T DeleteMin()
        {
            T min;

            if (!TryDeleteMin(out min))
            {
                throw new InvalidOperationException("SkipList is empty.");
            }

            return min;
        }

        public bool TryDeleteMin(out T result)
        {
            if (IsEmpty)
            {
                result = default(T);
                return false;
            }

            return Remove(FirstNode.Forwards[0].Value, out result);
        }

        public T Peek()
        {
            T peek;

            if (!TryPeek(out peek))
            {
                throw new InvalidOperationException("SkipList is empty.");
            }

            return peek;
        }

        public bool TryPeek(out T result)
        {
            if (IsEmpty)
            {
                result = default(T);
                return false;
            }

            result = FirstNode.Forwards[0].Value;
            return true;
        }

        #region IEnumerable<T> Implementation
        public IEnumerator<T> GetEnumerator()
        {
            var node = FirstNode;
            while (node.Forwards[0] != null && node.Forwards[0] != FirstNode)
            {
                node = node.Forwards[0];
                yield return node.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion IEnumerable<T> Implementation

        #region ICollection<T> Implementation
        public bool IsReadOnly
        {
            get { return false; }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException();
            if (array.Length == 0 || arrayIndex >= array.Length || arrayIndex < 0)
                throw new IndexOutOfRangeException();

            var enumerator = GetEnumerator();

            for (int i = arrayIndex; i < array.Length; ++i)
            {
                if (enumerator.MoveNext())
                    array[i] = enumerator.Current;
                else
                    break;
            }
        }
        /// <summary>
        /// Checks if list is empty or not
        /// </summary>
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public void Clear()
        {
            InitializeSkipList();
        }
        #endregion
    }
}
