using System;
using System.Linq;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.FileSystem.AppData;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.IO;
using System.Text;

namespace Orchard.Environment.Shell.Descriptor
{
    public class ShellDescriptorCache : IShellDescriptorCache
    {
        private readonly IAppDataFolder _appDataFolder;
        private readonly ILogger _logger;

        private const string DescriptorCacheFileName = "cache.dat";
        private static readonly object _synLock = new object();
        public ShellDescriptorCache(IAppDataFolder appDataFolder,
            ILogger<ShellDescriptorCache> logger,
            IStringLocalizer<ShellDescriptorCache> localizer)
        {
            _appDataFolder = appDataFolder;
            _logger = logger;
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public ShellDescriptor Fetch(string name)
        {
            lock (_synLock)
            {
                VerifyCacheFile();
                var text = _appDataFolder.ReadFileAsync(DescriptorCacheFileName).Result;
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(text);
                XmlNode rootNode = xmlDocument.DocumentElement;
                if (rootNode != null)
                {
                    foreach (XmlNode tenantNode in rootNode.ChildNodes)
                    {
                        if (String.Equals(tenantNode.Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            return GetShellDecriptorForCacheText(tenantNode.InnerText);
                        }
                    }
                }

                return null;

            }

        }

        public void Store(string name, ShellDescriptor descriptor)
        {
            lock (_synLock)
            {
                VerifyCacheFile();
                var text = _appDataFolder.ReadFileAsync(DescriptorCacheFileName).Result;
                bool tenantCacheUpdated = false;
                var saveWriter = new StringWriter();
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(text);
                XmlNode rootNode = xmlDocument.DocumentElement;
                if (rootNode != null)
                {
                    foreach (XmlNode tenantNode in rootNode.ChildNodes)
                    {
                        if (String.Equals(tenantNode.Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            tenantNode.InnerText = GetCacheTextForShellDescriptor(descriptor);
                            tenantCacheUpdated = true;
                            break;
                        }
                    }
                    if (!tenantCacheUpdated)
                    {
                        XmlElement newTenant = xmlDocument.CreateElement(name);
                        newTenant.InnerText = GetCacheTextForShellDescriptor(descriptor);
                        rootNode.AppendChild(newTenant);
                    }
                }

                xmlDocument.Save(saveWriter);
                _appDataFolder.CreateFileAsync(DescriptorCacheFileName, saveWriter.ToString()).Wait();
            }
        }

        private static string GetCacheTextForShellDescriptor(ShellDescriptor descriptor)
        {
            var sb = new StringBuilder();
            sb.Append(descriptor.SerialNumber + "|");
            foreach (var feature in descriptor.Features)
            {
                sb.Append(feature.Name + ";");
            }
            sb.Append("|");
            foreach (var parameter in descriptor.Parameters)
            {
                sb.Append(parameter.Component + "," + parameter.Name + "," + parameter.Value);
                sb.Append(";");
            }

            return sb.ToString();
        }

        private static ShellDescriptor GetShellDecriptorForCacheText(string p)
        {
            string[] fields = p.Trim().Split(new[] { "|" }, StringSplitOptions.None);
            var shellDescriptor = new ShellDescriptor { SerialNumber = Convert.ToInt32(fields[0]) };
            string[] features = fields[1].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            shellDescriptor.Features = features.Select(feature => new ShellFeature { Name = feature }).ToList();
            string[] parameters = fields[2].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            shellDescriptor.Parameters = parameters.Select(parameter => parameter.Split(new[] { "," }, StringSplitOptions.None)).Select(parameterFields => new ShellParameter { Component = parameterFields[0], Name = parameterFields[1], Value = parameterFields[2] }).ToList();

            return shellDescriptor;
        }

        /// <summary>
        /// Creates an empty cache file if it doesn't exist already
        /// </summary>
        private void VerifyCacheFile()
        {
            if (!_appDataFolder.GetFileInfo(DescriptorCacheFileName).Exists)
            {
                var writer = new StringWriter();
                using (var xmlWriter = XmlWriter.Create(writer))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("Tenants");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                }
                _appDataFolder.CreateFileAsync(DescriptorCacheFileName, writer.ToString()).Wait();
            }
        }

    }
}
