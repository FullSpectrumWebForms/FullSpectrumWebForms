using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Core
{
    public sealed class ControlExtensionsCollection : ICollection<ControlExtension>
    {
        internal ControlExtensionsCollection(ControlBase control)
        {
            Control = control;
        }

        private ControlBase Control { get; set; }

        private readonly Dictionary<string, ControlExtension> ControlExtensions = new Dictionary<string, ControlExtension>();
        public int Count => ControlExtensions.Count;

        public bool IsReadOnly => false;

        public void Add(ControlExtension item)
        {
            item.Bind(Control);


            Control.CallCustomClientEvent("registerControlExtension", new
            {
                item.ClientId
            });

            item.Initialize();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ControlExtension item)
        {
            return ControlExtensions.ContainsValue(item);
        }

        public void CopyTo(ControlExtension[] array, int arrayIndex)
        {
            ControlExtensions.Values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ControlExtension> GetEnumerator()
        {
            return ControlExtensions.Values.GetEnumerator();
        }

        public bool Remove(ControlExtension item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)ControlExtensions).GetEnumerator();
        }
    }
    public abstract class ControlExtension
    {
        public virtual string Id => GetType().FullName;
        public virtual string ClientId => Id;

        public ControlBase Control { get; private set; }

        internal void Bind(ControlBase control)
        {
            Control = control;
        }

        internal protected virtual void Initialize()
        {

        }


        protected void CallClientMethod(string name, object parameters = null)
        {
            Control.CallCustomClientEvent("callControlExtensionMethod", new
            {
                ClientId,
                MethodName = name,
                Parameters = parameters
            });
        }
        protected Task<T> CallClientMethod<T>(string name, object parameters = null)
        {
            return Control.CallCustomClientEvent<T>("callControlExtensionMethod", new
            {
                ClientId,
                MethodName = name,
                Parameters = parameters
            });
        }

    }
}
