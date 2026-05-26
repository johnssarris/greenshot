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
using System.Drawing.Drawing2D;
using Greenshot.Base.Effects;
using Greenshot.Base.Interfaces;

namespace Greenshot.Base.Core
{
    /// <summary>
    /// Minimal ISurface implementation that holds a bitmap and capture metadata.
    /// Used when there is no annotation editor.
    /// </summary>
    public class SimpleSurface : ISurface
    {
        private Image _image;

        public event SurfaceMessageEventHandler SurfaceMessage;

        public Image Image
        {
            get => _image;
            set
            {
                _image?.Dispose();
                _image = value != null ? (Image)value.Clone() : null;
            }
        }

        public Image GetImageForExport() => _image != null ? (Image)_image.Clone() : null;

        public ICaptureDetails CaptureDetails { get; set; }
        public string LastSaveFullPath { get; set; }
        public string UploadUrl { get; set; }
        public bool Modified { get; set; }

        public void SendMessageEvent(object source, SurfaceMessageTyp messageType, string message)
        {
            SurfaceMessage?.Invoke(source, new SurfaceMessageEventArgs
            {
                Surface = this,
                MessageType = messageType,
                Message = message
            });
        }

        public void ApplyBitmapEffect(IEffect effect)
        {
            if (_image == null || effect == null) return;
            using var matrix = new Matrix();
            var result = effect.Apply(_image, matrix);
            if (result != null && !ReferenceEquals(result, _image))
            {
                _image.Dispose();
                _image = result;
            }
        }

        public void Dispose()
        {
            _image?.Dispose();
            _image = null;
        }
    }
}
