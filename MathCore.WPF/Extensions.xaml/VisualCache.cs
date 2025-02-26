﻿using System;
using System.Windows.Markup;
using System.Windows.Media;
using MathCore.Annotations;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedType.Global

namespace MathCore.WPF
{
    /// <summary>Кеширование изображения</summary>
    [MarkupExtensionReturnType(typeof(BitmapCache))]
    public class VisualCache : MarkupExtension
    {
        /// <summary>Масштаб рендринга изображения</summary>
        public double Scale { get; set; } = 1;

        public bool ClearType { get; set; }

        /// <summary>Привязка к пикселям</summary>
        public bool RealPixels { get; set; }

        /// <summary>Кеширование изображения</summary>
        public VisualCache() { }

        /// <summary>Кеширование изображения</summary>
        /// <param name="scale"></param>
        public VisualCache(double scale) => Scale = scale;

        #region Overrides of MarkupExtension

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider sp) =>
            new BitmapCache
            {
                EnableClearType = ClearType,
                RenderAtScale = Scale,
                SnapsToDevicePixels = RealPixels
            };

        #endregion
    }
}