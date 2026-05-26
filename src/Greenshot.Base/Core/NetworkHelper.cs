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
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Greenshot.Base.Core.FileFormatHandlers;
using Dapplo.Ini;
using Greenshot.Base.Interfaces;
using log4net;

namespace Greenshot.Base.Core
{
    /// <summary>
    /// Description of NetworkHelper.
    /// </summary>
    public static class NetworkHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkHelper));
        private static readonly ICoreConfiguration Config = IniConfigRegistry.GetSection<ICoreConfiguration>();

        static NetworkHelper()
        {
            try
            {
                // Disable certificate checking
                ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            }
            catch (Exception ex)
            {
                Log.Warn("An error has occurred while allowing self-signed certificates:", ex);
            }
        }

        /// <summary>
        /// Download the uri into a memory stream, without catching exceptions
        /// </summary>
        /// <param name="url">Of an image</param>
        /// <returns>MemoryStream which is already seek-ed to 0</returns>
        public static MemoryStream GetAsMemoryStream(string url)
        {
            var request = CreateWebRequest(url);
            using var response = (HttpWebResponse) request.GetResponse();
            var memoryStream = RecyclableMemoryStreamFactory.GetStream("NetworkHelper.GetAsMemoryStream");
            using (var responseStream = response.GetResponseStream())
            {
                responseStream?.CopyTo(memoryStream);
                // Make sure it can be used directly
                memoryStream.Seek(0, SeekOrigin.Begin);
            }

            return memoryStream;
        }


        /// <summary>
        /// Download the uri to create a Bitmap
        /// </summary>
        /// <param name="url">Of an image</param>
        /// <returns>Bitmap</returns>
        public static Bitmap DownloadImage(string url)
        {
            var fileFormatHandlers = SimpleServiceProvider.Current.GetAllInstances<IFileFormatHandler>();

            var extensions = string.Join("|", fileFormatHandlers.ExtensionsFor(FileFormatHandlerActions.LoadFromStream));

            var imageUrlRegex = new Regex($@"(http|https)://.*(?<extension>{extensions})");
            var match = imageUrlRegex.Match(url);
            try
            {
                using var memoryStream = GetAsMemoryStream(url);
                try
                {
                    if (fileFormatHandlers.TryLoadFromStream(memoryStream, match.Success ? match.Groups["extension"]?.Value : null, out var bitmap))
                    {
                        return bitmap;
                    }
                }
                catch (Exception)
                {
                    // If we arrive here, the image loading didn't work, try to see if the response has a http(s) URL to an image and just take this instead.
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    string content;
                    using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8, true))
                    {
                        content = streamReader.ReadLine();
                    }

                    if (string.IsNullOrEmpty(content))
                    {
                        throw;
                    }

                    match = imageUrlRegex.Match(content);
                    if (!match.Success)
                    {
                        throw;
                    }

                    using var memoryStream2 = GetAsMemoryStream(match.Value);
                    if (fileFormatHandlers.TryLoadFromStream(memoryStream2, match.Success ? match.Groups["extension"]?.Value : null, out var bitmap))
                    {
                        return bitmap;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Problem downloading the image from: " + url, e);
            }

            return null;
        }

        /// <summary>
        /// Helper method to create a web request with a lot of default settings
        /// </summary>
        /// <param name="uri">string with uri to connect to</param>
        /// <returns>WebRequest</returns>
        public static HttpWebRequest CreateWebRequest(string uri)
        {
            return CreateWebRequest(new Uri(uri));
        }

        /// <summary>
        /// Helper method to create a web request, eventually with proxy
        /// </summary>
        /// <param name="uri">Uri with uri to connect to</param>
        /// <returns>WebRequest</returns>
        public static HttpWebRequest CreateWebRequest(Uri uri)
        {
            var webRequest = (HttpWebRequest) WebRequest.Create(uri);
            webRequest.Proxy = Config.UseProxy ? CreateProxy(uri) : null;
            // Make sure the default credentials are available
            webRequest.Credentials = CredentialCache.DefaultCredentials;

            // Allow redirect, this is usually needed so that we don't get a problem when a service moves
            webRequest.AllowAutoRedirect = true;
            // Set default timeouts
            webRequest.Timeout = Config.WebRequestTimeout * 1000;
            webRequest.ReadWriteTimeout = Config.WebRequestReadWriteTimeout * 1000;
            return webRequest;
        }

        /// <summary>
        /// Create a IWebProxy Object which can be used to access the Internet
        /// This method will check the configuration if the proxy is allowed to be used.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>IWebProxy filled with all the proxy details or null if none is set/wanted</returns>
        public static IWebProxy CreateProxy(Uri uri)
        {
            IWebProxy proxyToUse = null;
            if (!Config.UseProxy)
            {
                return proxyToUse;
            }

            proxyToUse = WebRequest.DefaultWebProxy;
            if (proxyToUse != null)
            {
                proxyToUse.Credentials = CredentialCache.DefaultCredentials;
                if (!Log.IsDebugEnabled)
                {
                    return proxyToUse;
                }

                // check the proxy for the Uri
                if (!proxyToUse.IsBypassed(uri))
                {
                    var proxyUri = proxyToUse.GetProxy(uri);
                    if (proxyUri != null)
                    {
                        Log.Debug("Using proxy: " + proxyUri + " for " + uri);
                    }
                    else
                    {
                        Log.Debug("No proxy found!");
                    }
                }
                else
                {
                    Log.Debug("Proxy bypass for: " + uri);
                }
            }
            else
            {
                Log.Debug("No proxy found!");
            }

            return proxyToUse;
        }
    }
}
