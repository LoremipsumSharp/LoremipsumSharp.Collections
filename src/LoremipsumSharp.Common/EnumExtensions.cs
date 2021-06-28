using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace LoremipsumSharp.Common
{
  public static class EnumExtension
    {
        /// <summary>
        /// 返回枚举的名称
        /// </summary>
        /// <typeparam name="T">枚举值类型</typeparam>
        /// <param name="key">枚举值</param>
        /// <param name="enumType">枚举类型</param>
        /// <returns></returns>
        public static string GetEnumName(this object val, Type enumType)
        {
            return Enum.GetName(enumType, val);
        }


        public static IEnumerable<T> GetAllItems<T>(this Enum value)
        {
            foreach (object item in Enum.GetValues(typeof(T)))
            {
                yield return (T)item;
            }
        }

        public static T GetValueByName<T>(this string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }

        
        public static string ToDescription<TEnum>(this TEnum source) where TEnum : struct
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }
    }
}