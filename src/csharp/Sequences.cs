// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.ML.OnnxRuntimeGenAI
{
    /// <summary>
    /// Represents a collection of encoded prompts/responses.
    /// </summary>
    public class Sequences : IDisposable
    {
        private IntPtr _sequencesHandle;
        private bool _disposed = false;
        private ulong _numSequences;

        internal Sequences(IntPtr sequencesHandle)
        {
            _sequencesHandle = sequencesHandle;
            _numSequences = NativeMethods.OgaSequencesCount(_sequencesHandle).ToUInt64();
        }

        internal IntPtr Handle { get { return _sequencesHandle; } }

        /// <summary>
        /// Gets the number of sequences in the collection. This is equivalent to the batch size.
        /// </summary>
        public ulong NumSequences { get { return _numSequences; } }

        /// <summary>
        /// The indexed accessor of individual sequence.
        /// </summary>
        public ReadOnlySpan<int> this[ulong sequenceIndex]
        {
            get
            {
                if (sequenceIndex >= _numSequences)
                {
                    throw new ArgumentOutOfRangeException(nameof(sequenceIndex));
                }
                ulong sequenceLength = NativeMethods.OgaSequencesGetSequenceCount(_sequencesHandle, (UIntPtr)sequenceIndex).ToUInt64();
                IntPtr sequencePtr = NativeMethods.OgaSequencesGetSequenceData(_sequencesHandle, (UIntPtr)sequenceIndex);
                unsafe
                {
                    return new ReadOnlySpan<int>(sequencePtr.ToPointer(), (int)sequenceLength);
                }
            }
        }

        ~Sequences()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            NativeMethods.OgaDestroySequences(_sequencesHandle);
            _sequencesHandle = IntPtr.Zero;
            _disposed = true;
        }
    }
}
