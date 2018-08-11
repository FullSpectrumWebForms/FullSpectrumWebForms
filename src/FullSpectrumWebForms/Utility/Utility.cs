using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Utility
{
    public static class Utility
    {
        public static void AddRange<T>( this System.Collections.ObjectModel.ObservableCollection<T> obj, IEnumerable<T> values )
        {
            foreach (var value in values)
                obj.Add(value);
        }
    }
}