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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using Greenshot.Base;
using Greenshot.Base.Core;
using Greenshot.Base.Interfaces;
using Greenshot.Configuration;
using Greenshot.Helpers;

namespace Greenshot.Destinations
{
    /// <summary>
    /// Description of PrinterDestination.
    /// </summary>
    public class PrinterDestination : AbstractDestination
    {
        private readonly string _printerName;

        public PrinterDestination()
        {
        }

        public PrinterDestination(string printerName)
        {
            _printerName = printerName;
        }

        public override string Designation => nameof(WellKnownDestinations.Printer);

        public override string Description
        {
            get
            {
                if (_printerName != null)
                {
                    return Language.GetString(LangKey.settings_destination_printer) + " - " + _printerName;
                }

                return Language.GetString(LangKey.settings_destination_printer);
            }
        }

        public override int Priority => 2;

        public override Keys EditorShortcutKeys => Keys.Control | Keys.P;

        public override Image DisplayIcon => GreenshotResources.GetImage("Printer.Image");

        public override bool IsDynamic => true;

        /// <summary>
        /// Create destinations for all the installed printers
        /// </summary>
        /// <returns>IEnumerable of IDestination</returns>
        public override IEnumerable<IDestination> DynamicDestinations()
        {
            string defaultPrinter = new PrinterSettings().PrinterName;
            var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            printers.Sort((p1, p2) =>
                defaultPrinter.Equals(p1) ? -1 :
                defaultPrinter.Equals(p2) ? 1 :
                string.Compare(p1, p2, StringComparison.Ordinal));
            foreach (string printer in printers)
            {
                yield return new PrinterDestination(printer);
            }
        }

        // TODO: Implement IAcceptsPreRenderedImage to avoid a redundant surface render pass
        // when a shared rendered bitmap is already available from the capture pipeline.
        // PrintHelper would need an overload accepting a pre-rendered Image instead of ISurface.

        /// <summary>
        /// Export the capture to the printer
        /// </summary>
        /// <param name="manuallyInitiated"></param>
        /// <param name="surface"></param>
        /// <param name="captureDetails"></param>
        /// <returns>ExportInformation</returns>
        public override ExportInformation ExportCapture(bool manuallyInitiated, ISurface surface, ICaptureDetails captureDetails)
        {
            ExportInformation exportInformation = new ExportInformation(Designation, Description);
            using var printHelper = new PrintHelper(surface, captureDetails);
            PrinterSettings printerSettings = !string.IsNullOrEmpty(_printerName)
                ? printHelper.PrintTo(_printerName)
                : !manuallyInitiated
                    ? printHelper.PrintTo(new PrinterSettings().PrinterName)
                    : printHelper.PrintWithDialog();

            exportInformation.ExportMade = printerSettings != null;

            ProcessExport(exportInformation, surface);
            return exportInformation;
        }
    }
}