﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MathCore.WPF.Converters
{
    [ValueConversion(typeof(bool?), typeof(Visibility))]
    [MarkupExtensionReturnType(typeof(Bool2Visibility))]
    public class Bool2Visibility : ValueConverter
    {
        public bool Inverted { get; set; }

        public bool Collapsed { get; set; }

        protected override object? Convert(object? v, Type? t, object? p, CultureInfo? c) =>
            v switch
            {
                null => null,
                Visibility => v,
                true => !Inverted ? Visibility.Visible : Collapsed ? Visibility.Collapsed : Visibility.Hidden,
                false => Inverted ? Visibility.Visible : Collapsed ? Visibility.Collapsed : Visibility.Hidden,
                _ => throw new NotSupportedException()
            };

        protected override object? ConvertBack(object? v, Type? t, object? p, CultureInfo? c) =>
            v switch
            {
                null => null,
                bool => v,
                Visibility.Visible => !Inverted,
                Visibility.Hidden => Inverted,
                Visibility.Collapsed => Inverted,
                _ => throw new NotSupportedException()
            };
    }
}