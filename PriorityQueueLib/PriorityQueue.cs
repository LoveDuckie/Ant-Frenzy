using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriorityQueueLib
{
    /// <summary>
    /// Used for storing potential path nodes in an ordered manner
    /// 
    /// Where T makes sure that the class adheres to the idea of having a IComparable method interface
    /// </summary>
    /// <typeparam name="T">The type of the item that we are going to be storing</typeparam>
    public class PriorityQueue<T> where T: IComparable<T>
    {
        #region Members
        private List<T> _data = new List<T>();
        #endregion

        #region Properties
        public List<T> Data
        {
            get { return _data; }
        }
        #endregion

        #region Constructors
        public PriorityQueue()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Simple method for returning whether or not the queue is empty.
        /// </summary>
        /// <returns>Returns true or false based on whether it is empty of not</returns>
        public bool IsEmpty()
        {
            return (_data.Count == 0);
        }

        /// <summary>
        /// Return the last item in the list without removing it unlike Poll
        /// </summary>
        /// <returns>Returns the item at the top of the list.</returns>
        public T Peek()
        {
            // Return the item at the head of the list
            if (_data != null)
            {
                if (_data.Count > 0)
                {
                    return _data[0];
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                return default(T);
            }
        }

        public void Add(T pValue)
        {
            Enqueue(pValue);
        }

        public void Enqueue(T pValue)
        {
            _data.Add(pValue);

            int ci = _data.Count - 1;

            // Considering that the list is never going to be 0
            // this will keep going infinitely.
            while (ci > 0)
            {
                int _pivotItem = (ci - 1) / 2;

                if (_data[_pivotItem].CompareTo(_data[ci]) > 0)
                {
                    T tmp = _data[ci];
                    _data[ci] = _data[_pivotItem];
                    _data[_pivotItem] = tmp;
                    ci = _pivotItem;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Remove an item from the queue and then re-sort it
        /// </summary>
        /// <param name="pValue">The item that we want to remove</param>
        /// <returns>Returns the value that we just removed.</returns>
        public T Dequeue(T pValue)
        {
            
            int _lastItem = _data.Count - 1;
            T frontItem = _data[0];
            _data[0] = _data[_lastItem];
            _data.RemoveAt(_lastItem);

            --_lastItem;
            int _pivotItem = 0;

            while (true)
            {
                int _currentItem = _pivotItem * 2 + 1;

                if (_currentItem > _lastItem)
                    break;

                int rc = _currentItem + 1;

                if (rc <= _lastItem && _data[rc].CompareTo(_data[_currentItem]) < 0)
                    _currentItem = rc;

                if (_data[_pivotItem].CompareTo(_data[_currentItem]) <= 0)
                    break;

                T tmp = _data[_pivotItem]; _data[_pivotItem] = _data[_currentItem];

                _data[_currentItem] = tmp;
                _pivotItem = _currentItem;
            }
            return frontItem;
        }

        public T Get(int pIndex)
        {
            if (pIndex < _data.Count && pIndex > 0)
            {
                return _data[pIndex];
            }
            else
            {
                return default(T);
            }
        }
        #endregion

        /// <summary>
        /// Remove the last item in the list and return it as a value
        /// </summary>
        /// <returns>The value that is to be returned</returns>
        public T Poll()
        {
            T _lastitem = _data[0];

            // Remove the last item in the list.
            Dequeue(_lastitem);

            return _lastitem;
        }

        /// <summary>
        /// Simply put clears out the list in question
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }


        /// <summary>
        /// Removes the item at the head and returns it as a value type as well
        /// </summary>
        /// <returns>Returns the item</returns>
        public T Pop()
        {
            T _return = _data[0];

            Dequeue(_return);

            return _return;
        }

        /// <summary>
        /// Not really sure how to implement this
        /// </summary>
        /// <param name="other">The other object that we are going to be dealing with</param>
        /// <returns>Returns which one is considered larger.</returns>
        public int CompareTo(T other)
        {
            throw new NotImplementedException();
        }
    }
}
