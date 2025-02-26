﻿using System;
using System.Globalization;
using MathCore.Annotations;
// ReSharper disable MemberCanBePrivate.Global

namespace MathCore.WPF.Converters
{
    public class Lambda<TValue, TResult> : ValueConverter
    {
        public delegate TResult Converter(TValue Value, Type? TargetValueType, object? Parameter, CultureInfo? Culture);

        public delegate TValue ConverterBack(TResult Value, Type? SourceValueType, object? Parameter, CultureInfo? Culture);

        private readonly Converter _Converter;

        private readonly ConverterBack _BackConverter;

        public Lambda(
            Func<TValue, TResult> Converter, 
            Func<TResult, TValue>? BackConverter = null)
            : this((v, _, _, _) => Converter(v), BackConverter is null ? null : ((v, _, _, _) => BackConverter(v)))
        { }

        public Lambda(Converter Converter, ConverterBack? BackConverter = null)
        {
            _Converter = Converter;
            _BackConverter = BackConverter ?? ((_, _, _, _) => throw new NotSupportedException());
        }

        /// <inheritdoc />
        protected override object? Convert(object? v, Type? t, object? p, CultureInfo? c) => 
            v is null 
                ? null 
                : _Converter((TValue)v, t, p, c);

        /// <inheritdoc />
        protected override object? ConvertBack(object? v, Type? t, object? p, CultureInfo? c) =>
            v is null
                ? null
                : _BackConverter((TResult) v, t, p, c);
    } 
}