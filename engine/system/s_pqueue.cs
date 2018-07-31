﻿using System;
using System.Collections.Generic;

namespace engine.system
{
    public class Pqueue<T>
    {
        /*
     
        there's an open issue for adding a binary heap to the standard c# library.
        until then, this open-sourced class.

        sourced from: www.redblobgames.com/pathfinding/a-star/implementation.html

     */

        private readonly List<Tuple<T, double>> _elements = new List<Tuple<T, double>>();

        public int Count => _elements.Count;

        public void Enqueue(T item, double priority)
        {
            _elements.Add(Tuple.Create(item, priority));
        }

        public T Dequeue()
        {
            var bestIndex = 0;

            for (var i = 0; i < _elements.Count; i++)
                if (_elements[i].Item2 < _elements[bestIndex].Item2)
                    bestIndex = i;

            var bestItem = _elements[bestIndex].Item1;
            _elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}