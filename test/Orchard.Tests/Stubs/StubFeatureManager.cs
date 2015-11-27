using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Tests.Stubs
{
    public class StubFeatureManager : IFeatureManager
    {
        public FeatureDependencyNotificationHandler FeatureDependencyNotification
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<string> DisableFeatures(IEnumerable<string> featureIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DisableFeatures(IEnumerable<string> featureIds, bool force)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnableFeatures(IEnumerable<string> featureIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnableFeatures(IEnumerable<string> featureIds, bool force)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FeatureDescriptor> GetAvailableFeatures()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDependentFeatures(string featureId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FeatureDescriptor> GetDisabledFeatures()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FeatureDescriptor> GetEnabledFeatures()
        {
            throw new NotImplementedException();
        }
    }
}