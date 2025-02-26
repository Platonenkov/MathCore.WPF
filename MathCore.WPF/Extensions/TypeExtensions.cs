﻿using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MathCore.Annotations;
// ReSharper disable UnusedMember.Global

// ReSharper disable MemberCanBePrivate.Global

namespace MathCore.WPF.Extensions
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<(Type SourceType, Type TargetType), Func<object, object>> __CastersDictionary = new();

        private static readonly ParameterExpression __ConvParameter = Expression.Parameter(typeof(object), "value");

        private static Func<object, object> GetCasterFrom(Type TargetType, object? Source) => TargetType.GetCasterFrom(Source?.GetType() ?? typeof(object));

        public static Func<object, object> GetCasterTo(this Type SourceType, Type TargetType) => TargetType.GetCasterFrom(SourceType);
        public static Func<object, object> GetCasterFrom(this Type TargetType, Type SourceType) =>
            __CastersDictionary.GetOrAdd((SourceType, TargetType), ((Type Source, Type Target) vv) => Expression.Lambda<Func<object, object>>(
                    Expression.Convert(
                        Expression.Convert(
                            Expression.Convert(__ConvParameter, vv.Source), 
                            vv.Target), 
                        typeof(object)), 
                    __ConvParameter)
               .Compile());

        public static object Cast(this Type type, object obj) => GetCasterFrom(type, obj)(obj);

        private static readonly ConcurrentDictionary<(Type SourceType, Type TargetType), Func<object, object>> __ConvertersDictionary = new();

        public static Func<object, object> GetConverterTo(this Type SourceType, Type TargetType)
            => TargetType.GetConverterFrom(SourceType);

        public static Func<object, object> GetConverterFrom(this Type TargetType, Type SourceType) => 
            __ConvertersDictionary.GetOrAdd(
                (SourceType, TargetType), 
                ((Type Source, Type Target) vv) => vv.Source.GetConvertExpression_Object(vv.Target).Compile());


        public static Expression GetCastExpression(this Type FromType, Type ToType, ref ParameterExpression? parameter)
        {
            parameter ??= Expression.Parameter(typeof(object), "value");
            return Expression.Convert(Expression.Convert(Expression.Convert(__ConvParameter, FromType), ToType), typeof(object));
        }

        public static LambdaExpression GetConvertExpression(this Type FromType, Type ToType)
        {
            var c = FromType.GetTypeConverter();
            TypeConverter? c_to = null;
            if (!c.CanConvertTo(ToType) && !(c_to = ToType.GetTypeConverter()).CanConvertFrom(FromType))
                throw new NotSupportedException($"Преобразование из {FromType} в {ToType} не поддерживается");
            var expr_from = Expression.Parameter(FromType, "pFrom");
            var expr_from2tObject = Expression.Convert(expr_from, typeof(object));
            var expr_converter = Expression.Constant(c_to ?? c);
            var method = (c_to is null
                            ? (Delegate)(Func<object, Type, object>)c.ConvertTo
                            : (Func<object, object>)c_to.ConvertFrom)
                            .Method;
            var exprs_converter = c_to is null
                ? new Expression[] { expr_from2tObject, Expression.Constant(ToType) }
                : new Expression[] { expr_from2tObject };
            var expr_conversation = Expression.Call(expr_converter, method, exprs_converter);

            return Expression.Lambda(Expression.Convert(expr_conversation, ToType), expr_from);
        }

        public static Expression<Func<object, object>> GetConvertExpression_Object(this Type FromType, Type ToType)
        {
            var c = FromType.GetTypeConverter();
            TypeConverter? c_to = null;
            if (!c.CanConvertTo(ToType) && !(c_to = ToType.GetTypeConverter()).CanConvertFrom(FromType))
                throw new NotSupportedException($"Преобразование из {FromType} в {ToType} не поддерживается");
            var expr_from = Expression.Parameter(typeof(object), "pFrom");
            var expr_converter = Expression.Constant(c_to ?? c);
            var method = (c_to is null
                            ? (Delegate)(Func<object, Type, object>)c.ConvertTo
                            : (Func<object, object>)c_to.ConvertFrom)
                            .Method;
            var exprs_converter = c_to is null
                ? new Expression[] { expr_from, Expression.Constant(ToType) }
                : new Expression[] { expr_from };
            var expr_conversation = Expression.Call(expr_converter, method, exprs_converter);

            return Expression.Lambda<Func<object, object>>(Expression.Convert(expr_conversation, typeof(object)), expr_from);
        }

        /// <summary>Получить конвертер значений для указанного типа данных</summary>
        /// <param name="type">Тип, для которого требуется получить конвертер</param>
        /// <returns>Конвертер указанного типа данных</returns>
        public static TypeConverter GetTypeConverter(this Type type) => TypeDescriptor.GetConverter(type);

        /// <summary>Получить тип по его имени из всех загруженных сборок</summary>
        /// <param name="TypeName">Имя типа</param>
        /// <returns>Тип</returns>
        [DebuggerStepThrough]
        public static Type? GetType(string TypeName)
        {
            var type_array = AppDomain.CurrentDomain.GetAssemblies().
                SelectMany((a, i) => a.GetTypes()).Where(t => t.Name == TypeName).ToArray();
            return type_array.Length != 0 ? type_array[0] : null;
        }

        /// <summary>Получить все атрибуты типа указанного типа</summary>
        /// <typeparam name="TAttribute">Тип требуемых атрибутов</typeparam>
        /// <param name="T">Тип, атрибуты которого требуется получить</param>
        /// <returns>Массив атрибутов типа указанного типа</returns>
        [DebuggerStepThrough]
        public static TAttribute[] GetCustomAttributes<TAttribute>(this Type T)
            where TAttribute : Attribute => GetCustomAttributes<TAttribute>(T, false);

        [DebuggerStepThrough]
        public static TAttribute[] GetCustomAttributes<TAttribute>(this Type T, bool Inherited)
             where TAttribute : Attribute => T.GetCustomAttributes(typeof(TAttribute), Inherited).OfType<TAttribute>().ToArray();

        [DebuggerStepThrough]
        public static object? CreateObject(this Type type) =>
            //var constructor = type.GetConstructor(new Type[] { });
            //if (constructor is null)
            //    throw new InvalidOperationException("Не найден конструктор типа " +
            //        type + " без параметров. Для данного типа доступны следующие конструкторы " +
            //        type.GetConstructors().ConvertObjectTo(CInfo =>
            //        {
            //            if (CInfo.Length == 0) return "{}";
            //            var Result = "{" + CInfo[0].ToString();
            //            for (var i = 1; i < CInfo.Length; i++)
            //                Result += "; " + CInfo[i].ToString();
            //            return Result + "}";
            //        }));
            //return constructor.Invoke(new object[] { });
            //Contract.Requires(type != null);
            Activator.CreateInstance(type);

        [DebuggerStepThrough]
        public static T Create<T>(this Type type) => (T)type.CreateObject()!;

        [DebuggerStepThrough]
        public static T? Create<T>(this Type type, params object[] Params) => (T?)type.CreateObject(Params);

        [DebuggerStepThrough]
        public static object? CreateObject(this Type type, params object[] Params) => Activator.CreateInstance(type, Params);

        [DebuggerStepThrough]
        public static object? CreateObject(this Type type, BindingFlags Flags, Binder binder, params object[] Params) => Activator.CreateInstance(type, Flags, binder, Params);

        [DebuggerStepThrough]
        public static T? Create<T>(params object[] Params) => (T?)CreateObject(typeof(T), Params);

        [DebuggerStepThrough]
        public static T? Create<T>(BindingFlags Flags, Binder binder, params object[] Params) => (T?)CreateObject(typeof(T), Flags, binder, Params);

        public static void AddConverter(this Type type, Type ConverterType) => TypeDescriptor.AddAttributes(type, new TypeConverterAttribute(ConverterType));

        public static void AddConverter(this Type type, params Type[] ConverterTypes) =>
            TypeDescriptor.AddAttributes(type, ConverterTypes.Select(t => new TypeConverterAttribute(t)).Cast<Attribute>().ToArray());

        public static TypeDescriptionProvider GetProvider(this Type type) => TypeDescriptor.GetProvider(type);

        public static void AddProvider(this Type type, TypeDescriptionProvider provider) => TypeDescriptor.AddProvider(provider, type);
    }
}