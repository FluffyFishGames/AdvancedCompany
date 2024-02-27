using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Lib.SyncHandler
{
    public interface IItemSynchronizationHandler
    {
        void ReadObject(global::GrabbableObject item);
        void ApplyObject(global::GrabbableObject item);
        void Read(FastBufferReader reader);
        void Write(FastBufferWriter writer);
    }
    public abstract class IItemSynchronisationHandler<T> : IItemSynchronizationHandler where T : GrabbableObject
    {
        public abstract void Read(T item);
        public abstract void Apply(T item);

        public void ReadObject(global::GrabbableObject item)
        {
            Read((T)item);
        }

        public void ApplyObject(global::GrabbableObject item)
        {
            Apply((T)item);
        }

        public abstract void Read(FastBufferReader reader);
        public abstract void Write(FastBufferWriter writer);
    }
}
