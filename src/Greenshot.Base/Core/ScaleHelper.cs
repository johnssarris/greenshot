/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2004-2026 Thomas Braun, Jens Klingen, Robin Krom
 *
 * For more information see: https://getgreenshot.org/
 * The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using Dapplo.Windows.Common.Structs;

namespace Greenshot.Base.Core
{
    /// <summary>
    /// Helpers for scaling and aligning rectangles.
    /// </summary>
    public static class ScaleHelper
    {
        /// <summary>
        /// Calculates the size an element must be resized to in order to fit another element, keeping aspect ratio.
        /// </summary>
        /// <param name="currentSize">Size of the element to be resized</param>
        /// <param name="targetSize">Target size</param>
        /// <param name="crop">If true, scale so the element fills targetSize (may overflow); if false, scale to fit inside</param>
        public static NativeSizeFloat GetScaledSize(NativeSizeFloat currentSize, NativeSizeFloat targetSize, bool crop)
        {
            float wFactor = targetSize.Width / currentSize.Width;
            float hFactor = targetSize.Height / currentSize.Height;
            float factor = crop ? Math.Max(wFactor, hFactor) : Math.Min(wFactor, hFactor);
            return new NativeSizeFloat(currentSize.Width * factor, currentSize.Height * factor);
        }

        /// <summary>
        /// Calculates the position of an element aligned within a rectangle.
        /// </summary>
        public static NativeRectFloat GetAlignedRectangle(NativeRectFloat currentRect, NativeRectFloat targetRect, ContentAlignment alignment)
        {
            var newRect = new NativeRectFloat(targetRect.Location, currentRect.Size);
            return alignment switch
            {
                ContentAlignment.TopCenter => newRect.ChangeX((targetRect.Width - currentRect.Width) / 2),
                ContentAlignment.TopRight => newRect.ChangeX(targetRect.Width - currentRect.Width),
                ContentAlignment.MiddleLeft => newRect.ChangeY((targetRect.Height - currentRect.Height) / 2),
                ContentAlignment.MiddleCenter => newRect.ChangeY((targetRect.Height - currentRect.Height) / 2).ChangeX((targetRect.Width - currentRect.Width) / 2),
                ContentAlignment.MiddleRight => newRect.ChangeY((targetRect.Height - currentRect.Height) / 2).ChangeX(targetRect.Width - currentRect.Width),
                ContentAlignment.BottomLeft => newRect.ChangeY(targetRect.Height - currentRect.Height),
                ContentAlignment.BottomCenter => newRect.ChangeY(targetRect.Height - currentRect.Height).ChangeX((targetRect.Width - currentRect.Width) / 2),
                ContentAlignment.BottomRight => newRect.ChangeY(targetRect.Height - currentRect.Height).ChangeX(targetRect.Width - currentRect.Width),
                _ => newRect
            };
        }
    }
}
