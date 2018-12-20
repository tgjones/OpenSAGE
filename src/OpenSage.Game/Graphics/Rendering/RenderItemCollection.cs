﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering
{
    public sealed class RenderItemCollection
    {
        // TODO: Should this just be 0? Or somewhere in the hundreds?
        // A map usually has thousands of objects.
        private const int PreAllocatedItems = 128;
        private const double GrowthFactor = 1.5;

        // The backing storage for render items. 
        private RenderItem[] _items;

        // TODO: Bounds check?
        public ref RenderItem this[int i] => ref _items[i];

        // An array of flags indicating if a render item should be included in the culling set.
        // Must have the same capacity and length as _items.
        private bool[] _culled;

        // Number of render items in the collection.
        public int Length { get; private set; }

        // Sorted indices of objects that were chosen to be rendered.
        // Indices refer to _items.
        private readonly List<int> _culledItemIndices;
        public IReadOnlyList<int> CulledItemIndices => _culledItemIndices;

        public RenderItemCollection()
        {
            _culledItemIndices = new List<int>();
            _items = new RenderItem[PreAllocatedItems];
            _culled = new bool[PreAllocatedItems];
            Length = 0;
        }

        public void Add(RenderItem item)
        {
            if (Length == _items.Length)
            {
                var newCapacity = (int) (Length * GrowthFactor);
                Array.Resize(ref _items, newCapacity);
                Array.Resize(ref _culled, newCapacity);
            }

            _items[Length++] = item;
        }

        // Performs frustum culling for a single render item in _items.
        // Increments the provided integer reference if the object was within the frustum.
        private void Cull(int i, in BoundingFrustum cameraFrustum)
        {
            if (_items[i].CullFlags.HasFlag(CullFlags.AlwaysVisible) || cameraFrustum.Intersects(_items[i].BoundingBox))
            {
                _culled[i] = true;
            }
            else
            {
                _culled[i] = false;
            }
        }

        public void CullAndSort(in BoundingFrustum cameraFrustum, int batchSize)
        {
            if (Length == 0)
            {
                return;
            }

            // Step 1: Compute visibility for each item in _items and store the result _culled.

            // TODO: If Length <= batchSize, don't call Parallel.ForEach
            if (batchSize == -1)
            {
                // Perform culling in the main thread.
                for (var i = 0; i < Length; i++)
                {
                    Cull(i, cameraFrustum);
                }
            }
            else
            {
                // We need a copy of cameraFrustum, as we can't send in parameters to closures. 
                var frustum = cameraFrustum;

                // Perform culling using the thread pool, in batches of batchSize.
                Parallel.ForEach(Partitioner.Create(0, Length, batchSize), range =>
                {
                    for (var i = range.Item1; i < range.Item2; i++)
                    {
                        Cull(i, frustum);
                    }
                });
            }

            // Step 2: Go through _culled, and store the indices of culled values in _resultIndices.
            // Also count the number of culled render items.
            
            for (var i = 0; i < Length; i++)
            {
                if (_culled[i])
                {
                    _culledItemIndices.Add(i);
                }
            }

            // Step 3: Sort the indices by comparing render item keys.
            _culledItemIndices.Sort((a, b) => _items[a].Key.CompareTo(_items[b].Key));
            // Array.Sort(_resultIndices, 0, _resultIndicesLength, new IndiceComparer(_items));
        }

        public void Clear()
        {
            Length = 0;
            _culledItemIndices.Clear();

            // TODO: Should we provide a different method for actually clearing the item buffer?
            // Otherwise there might be memory leaks when switching between scenes.
        }

        internal readonly struct IndiceComparer : IComparer<int>
        {
            private readonly RenderItem[] _items;

            public IndiceComparer(RenderItem[] items)
            {
                _items = items;
            }

            public int Compare(int x, int y) => _items[x].Key.CompareTo(_items[y].Key);
        }
    }
}
