using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LoremipsumSharp.Common
{
    public static class ObjectExtensions
    {
         public static IDictionary<string, T> AnonymousObjectToDictionary<T>(
       this object obj, Func<object, T> valueSelect)
        {
            return TypeDescriptor.GetProperties(obj)
                .OfType<PropertyDescriptor>()
                .ToDictionary<PropertyDescriptor, string, T>(
                    prop => prop.Name,
                    prop => valueSelect(prop.GetValue(obj))
                );
        }

        
    }
}