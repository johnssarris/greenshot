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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Dapplo.Windows.Common.Structs;

namespace Greenshot.Base.Controls
{
    /// <summary>
    /// Button that displays a color swatch and opens a standard color picker on click.
    /// </summary>
    public class ColorButton : Button, IGreenshotLanguageBindable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _selectedColor = Color.White;

        [Category("Greenshot"), DefaultValue(null)]
        public string LanguageKey { get; set; }

        public ColorButton()
        {
            Click += ColorButtonClick;
        }

        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;

                Brush brush = value != Color.Transparent
                    ? (Brush)new SolidBrush(value)
                    : new HatchBrush(HatchStyle.Percent50, Color.White, Color.Gray);

                if (Image != null)
                {
                    using Graphics g = Graphics.FromImage(Image);
                    g.FillRectangle(brush, new NativeRect(4, 17, 16, 3));
                }

                brush.Dispose();
                Invalidate();
            }
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            using var colorDialog = new ColorDialog { Color = SelectedColor };
            if (colorDialog.ShowDialog(this) != DialogResult.OK) return;
            if (colorDialog.Color.Equals(SelectedColor)) return;
            SelectedColor = colorDialog.Color;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedColor"));
        }
    }
}
