using System;
using System.Collections.Generic;

namespace OrchardCore.Environment.Extensions.Features
{
    public class FeatureEntry
    {
        public FeatureEntry(IFeatureInfo featureInfo, IEnumerable<Type> exportedTypes)
        {
            FeatureInfo = featureInfo;
            ExportedTypes = exportedTypes;
        }

        public IFeatureInfo FeatureInfo { get; protected set; }
        public IEnumerable<Type> ExportedTypes { get; protected set; }
    }
}
