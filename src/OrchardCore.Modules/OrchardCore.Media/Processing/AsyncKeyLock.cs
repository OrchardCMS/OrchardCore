// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// The async key lock prevents multiple asynchronous threads acting upon the same object with the given key at the same time.
    /// It is designed so that it does not block unique requests allowing a high throughput.
    /// </summary>
    internal sealed class AsyncKeyLock
    {
        /// <summary>
        /// A collection of doorman counters used for tracking references to the same key.
        /// </summary>
        private static readonly Dictionary<string, Doorman> Keys = new Dictionary<string, Doorman>();

        /// <summary>
        /// A pool of unused doorman counters that can be re-used to avoid allocations.
        /// </summary>
        private static readonly Stack<Doorman> Pool = new Stack<Doorman>(MaxPoolSize);

        /// <summary>
        /// Maximum size of the doorman pool. If the pool is already full when releasing
        /// a doorman, it is simply left for garbage collection.
        /// </summary>
        private const int MaxPoolSize = 20;

        /// <summary>
        /// SpinLock used to protect access to the Keys and Pool collections.
        /// </summary>
        private static SpinLock spinLock = new SpinLock(false);

        /// <summary>
        /// Locks the current thread in read mode asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> ReaderLockAsync(string key)
        {
            Doorman doorman = GetDoorman(key);

            return await doorman.ReaderLockAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Locks the current thread in write mode asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> WriterLockAsync(string key)
        {
            Doorman doorman = GetDoorman(key);

            return await doorman.WriterLockAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the doorman for the specified key. If no such doorman exists, an unused doorman
        /// is obtained from the pool (or a new one is allocated if the pool is empty), and it's
        /// assigned to the requested key.
        /// </summary>
        /// <param name="key">The key for the desired doorman.</param>
        /// <returns>The <see cref="Doorman"/>.</returns>
        private static Doorman GetDoorman(string key)
        {
            Doorman doorman;
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);

                if (!Keys.TryGetValue(key, out doorman))
                {
                    doorman = (Pool.Count > 0) ? Pool.Pop() : new Doorman(ReleaseDoorman);
                    doorman.Key = key;
                    Keys.Add(key, doorman);
                }

                doorman.RefCount++;
            }
            finally
            {
                if (lockTaken)
                {
                    spinLock.Exit();
                }
            }

            return doorman;
        }

        /// <summary>
        /// Releases a reference to a doorman. If the ref-count hits zero, then the doorman is
        /// returned to the pool (or is simply left for the garbage collector to cleanup if the
        /// pool is already full).
        /// </summary>
        /// <param name="doorman">The <see cref="Doorman"/>.</param>
        private static void ReleaseDoorman(Doorman doorman)
        {
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);

                if (--doorman.RefCount == 0)
                {
                    Keys.Remove(doorman.Key);
                    if (Pool.Count < MaxPoolSize)
                    {
                        doorman.Key = null;
                        Pool.Push(doorman);
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    spinLock.Exit();
                }
            }
        }
    }
}