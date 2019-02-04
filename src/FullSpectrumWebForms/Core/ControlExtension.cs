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

        public void Add(ControlExtension controlExtension)
        {
            if (controlExtension is null)
                throw new ArgumentNullException("Cannot add an empty control extension to a control");
            if (controlExtension.Control != null)
                throw new Exception($"Control Extension '{controlExtension.Id}' already assigned to a control");

            ControlExtensions.Add(controlExtension.Id, controlExtension);
            controlExtension.Bind(Control);


            Control.CallCustomClientEvent("registerControlExtension", new
            {
                controlExtension.ClientId
            });

            controlExtension.Initialize();
        }

        public T Add<T>() where T: ControlExtension
        {
            var controlExtension = Activator.CreateInstance<T>();
            Add(controlExtension);
            return controlExtension;
        }

        public T Get<T>() where T : ControlExtension
        {
            if (TryGet<T>(out var controlExtension))
                return controlExtension;
            throw new Exception($"Cannot find or parse control extension for type {typeof(T).FullName}");
        }
        public bool TryGet<T>(out T controlExtension) where T : ControlExtension
        {
            if( TryGet(typeof(T).FullName, out var controlExtension_))
            {
                controlExtension = controlExtension_ as T;
                return controlExtension != null;
            }
            controlExtension = null;
            return false;
        }

        public bool TryGet(string id, out ControlExtension controlExtension)
        {
            return ControlExtensions.TryGetValue(id, out controlExtension);
        }

        public void Clear()
        {
            foreach (var controlExtension in ControlExtensions.Values.ToList())
                Remove(controlExtension);
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

        public bool Remove(ControlExtension controlExtension)
        {
            var isRemoved = ControlExtensions.Remove(controlExtension.Id);
            if (isRemoved)
            {
                controlExtension.Uninitialize();
                Control.CallCustomClientEvent("unregisterControlExtension", new
                {
                    controlExtension.ClientId
                });
            }
            return isRemoved;
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

        internal protected virtual void Bind(ControlBase control)
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

        protected internal virtual void Uninitialize()
        {
        }
    }
}
