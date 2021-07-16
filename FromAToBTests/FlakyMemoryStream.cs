using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FromAToBTests
{
    /// <summary>
    /// Represents a stream that should not be trusted to be consistent.
    /// </summary>
    public class FlakyMemoryStream : MemoryStream
    {
        private readonly Queue<bool> _shouldThrowQueue = new Queue<bool>();
        public FlakyMemoryStream()
        {
        }

        public FlakyMemoryStream(params bool[] throwArray)
        {
            PopulateThrowQueue(throwArray);
        }

        /// <summary>
        /// By passing in bool[] { false, true, false, true } after bytes, calling ReadAsync
        /// will return on the first call and third call, and throw on the second and last.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="throwArray"></param>
        public FlakyMemoryStream(byte[] bytes, params bool[] throwArray) : base(bytes)
        {
            PopulateThrowQueue(throwArray);
        }

        private void PopulateThrowQueue(bool[] throwArray)
        {
            foreach (var value in throwArray)
            {
                _shouldThrowQueue.Enqueue(value);
            }
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _shouldThrowQueue.Count > 0 && _shouldThrowQueue.Dequeue()
                ? throw new AccessViolationException("???")
                : base.ReadAsync(buffer, offset, count, cancellationToken);
        }
    }
}