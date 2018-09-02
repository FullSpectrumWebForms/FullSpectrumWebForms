using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Core
{
    public class PropertyWrapperAttribute: Attribute
    {
        public PropertyWrapperAttribute(string property)
        {
            Property = property;
        }
        public string Property;
    }
}
