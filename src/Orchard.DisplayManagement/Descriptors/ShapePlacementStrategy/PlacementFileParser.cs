using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.FileSystem;
using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    /// <summary>
    /// Parses and caches the Placement.info file contents for a given IWebSiteFolder vdir
    /// </summary>
    public interface IPlacementFileParser : IDependency
    {
        PlacementFile Parse(string virtualPath);
        PlacementFile ParseText(string placementText);
    }


    public class PlacementFileParser : IPlacementFileParser
    {
        private readonly IClientFolder _webSiteFolder;

        public PlacementFileParser(IClientFolder webSiteFolder)
        {
            _webSiteFolder = webSiteFolder;
        }

        public bool DisableMonitoring { get; set; }

        public PlacementFile Parse(string virtualPath)
        {
            var placementText = _webSiteFolder.ReadFile(virtualPath);
            return ParseText(placementText);
        }

        public PlacementFile ParseText(string placementText)
        {
            if (placementText == null)
                return null;


            var element = XElement.Parse(placementText);
            return new PlacementFile
            {
                Nodes = Accept(element).ToList()
            };
        }

        private IEnumerable<PlacementNode> Accept(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "Placement":
                    return AcceptMatch(element);
                case "Match":
                    return AcceptMatch(element);
                case "Place":
                    return AcceptPlace(element);
            }
            return Enumerable.Empty<PlacementNode>();
        }


        private IEnumerable<PlacementNode> AcceptMatch(XElement element)
        {
            if (element.HasAttributes == false)
            {
                // Match with no attributes will collapse child results upward
                // rather than return an unconditional node
                return element.Elements().SelectMany(Accept);
            }

            // return match node that carries back key/value dictionary of condition,
            // and has child rules nested as Nodes
            return new[]{new PlacementMatch{
                Terms = element.Attributes().ToDictionary(attr=>attr.Name.LocalName, attr=>attr.Value),
                Nodes=element.Elements().SelectMany(Accept).ToArray(),
            }};
        }

        private IEnumerable<PlacementShapeLocation> AcceptPlace(XElement element)
        {
            // return attributes as part locations
            return element.Attributes().Select(attr => new PlacementShapeLocation
            {
                ShapeType = attr.Name.LocalName,
                Location = attr.Value
            });
        }
    }
}