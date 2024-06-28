/*++
Copyright (c) 2012 Microsoft Corporation

Module Name:

    Z3Object.cs

Abstract:

    Z3 Managed API: Internal Z3 Objects

Author:

    Christoph Wintersteiger (cwinter) 2012-03-21

Notes:
    
--*/

using System.Diagnostics;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Z3
{
    /// <summary>
    /// Internal base class for interfacing with native Z3 objects.
    /// Should not be used externally.
    /// </summary>
    public abstract class Z3Object : IDisposable
    {
        private readonly CancellationTokenRegistration _disposeMethodRegistration;

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Z3Object()
        {
            Dispose();            
        }
        
        public bool Disposed { get; private set; }

        /// <summary>
        /// Disposes of the underlying native Z3 object.
        /// </summary>
        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }
            
            if (m_n_obj != IntPtr.Zero)
            {
                DecRef(m_n_obj);
                m_n_obj = IntPtr.Zero;                
            }

            _disposeMethodRegistration.Dispose();
            Disposed = true;

            GC.SuppressFinalize(this);
        }

        #region Object Invariant
        
        private void ObjectInvariant()
        {
            Debug.Assert(this.m_ctx != null);
        }

        #endregion

        #region Internal
        private readonly Context m_ctx;
        private IntPtr m_n_obj = IntPtr.Zero;

        internal Z3Object(Context ctx)
        {
            Debug.Assert(ctx != null);
            m_ctx = ctx;
            _disposeMethodRegistration = ctx.RegisterForDispose(this);
        }

        internal Z3Object(Context ctx, IntPtr obj)
        {
            Debug.Assert(ctx != null);
            m_ctx = ctx;
            m_n_obj = obj;
            IncRef(obj);
            _disposeMethodRegistration = ctx.RegisterForDispose(this);
        }

        internal abstract void IncRef(IntPtr o);
        internal abstract void DecRef(IntPtr o);

        internal virtual void CheckNativeObject(IntPtr obj) { }

        internal virtual IntPtr NativeObject
        {
            get { return m_n_obj; }
            set
            {
                if (value != IntPtr.Zero) { CheckNativeObject(value); IncRef(value); }
                if (m_n_obj != IntPtr.Zero) { DecRef(m_n_obj); }
                m_n_obj = value;
            }
        }

        internal static IntPtr GetNativeObject(Z3Object s)
        {
            if (s == null) return IntPtr.Zero;
            return s.NativeObject;
        }

        /// <summary>
        /// Access Context object 
        /// </summary>
	    public Context Context
        {
            get 
            {
                return m_ctx; 
            }            
        }

        internal static IntPtr[] ArrayToNative(Z3Object[] a)
        {

            if (a == null) return null;
            IntPtr[] an = new IntPtr[a.Length];
            for (uint i = 0; i < a.Length; i++)
                if (a[i] != null) an[i] = a[i].NativeObject;
            return an;
        }

        internal static uint ArrayLength(Z3Object[] a)
        {
            return (a == null)?0:(uint)a.Length;
        }
#endregion
    }
}
