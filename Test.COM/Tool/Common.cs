﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Test.COM.Entity;

namespace Test.COM.Tool
{
    public static class Common
    {
        #region Enum扩展
        /// <summary>
        /// 获取Enum的Description
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
        /// <summary>
        /// Enum转List
        /// </summary>
        public static List<BaseItem> Enum2List<TEnum>(string defaultValue) where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
            {
                return new List<BaseItem>();
            }

            return (
                from int s in Enum.GetValues(typeof(TEnum))
                select new BaseItem()
                {
                    Key = s.ToString(),
                    Value = Enum.GetName(typeof(TEnum), s),
                    Text = ((Enum)Enum.Parse(typeof(TEnum), s.ToString(), true)).GetDescription(),
                    IsSelected = s.ToString() == defaultValue
                }).ToList();
        }
        #endregion

        #region String扩展
        public static int ToInt(this string value)
        {
            return int.Parse(value);
        }

        public static int ToInt(this string value, int defaultValue)
        {
            int result;
            return int.TryParse(value, out result) ? result : defaultValue;
        }

        public static int? ToNullableInt(this string value)
        {
            int result;

            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out result))
            {
                return null;
            }

            return result;
        }

        public static decimal ToDecimal(this string value)
        {
            return decimal.Parse(value);
        }

        public static decimal ToDecimal(this string value, decimal defaultValue)
        {
            decimal result;
            return decimal.TryParse(value, out result) ? result : defaultValue;
        }

        public static decimal ToRoundDecimal(this string value, decimal defaultValue, int decimals)
        {
            decimal result;
            result = Math.Round(decimal.TryParse(value, out result) ? result : defaultValue, decimals);
            return result;
        }
        public static decimal? ToNullableDecimal(this string value)
        {
            decimal result;
            if (string.IsNullOrEmpty(value) || !decimal.TryParse(value, out result))
            {
                return null;
            }
            return result;
        }
        #endregion
    }
    /// <summary>
    /// 利用泛型进行对象复制
    /// </summary>
    public static class CopyObject<TIn, TOut>
    {
        private static readonly Func<TIn, TOut> Cache = GetFunc();
        private static Func<TIn, TOut> GetFunc()
        {
            var parameterExpression = Expression.Parameter(typeof(TIn), "p");
            var memberBindingList = new List<MemberBinding>();
            foreach (var item in typeof(TOut).GetProperties())
            {
                if (!item.CanWrite)
                    continue;
                var property = Expression.Property(parameterExpression, typeof(TIn).GetProperty(item.Name));
                var memberBinding = Expression.Bind(item, property);
                memberBindingList.Add(memberBinding);
            }

            var memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
            var lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, new[] { parameterExpression });
            return lambda.Compile();
        }

        public static TOut Trans(TIn tIn)
        {
            return Cache(tIn);
        }
    }
}
