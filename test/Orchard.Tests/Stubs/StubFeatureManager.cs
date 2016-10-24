//using Orchard.Environment.Extensions.Features;
//using System;
//using System.Collections.Generic;
//using Orchard.Environment.Extensions.Models;
//using System.Threading.Tasks;

//namespace Orchard.Tests.Stubs
//{
//    public class StubFeatureManager : IFeatureManager
//    {
//        public FeatureDependencyNotificationHandler FeatureDependencyNotification
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public Task<IEnumerable<string>> DisableFeaturesAsync(IEnumerable<string> featureIds)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<IEnumerable<string>> DisableFeaturesAsync(IEnumerable<string> featureIds, bool force)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<IEnumerable<string>> EnableFeaturesAsync(IEnumerable<string> featureIds)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<IEnumerable<string>> EnableFeaturesAsync(IEnumerable<string> featureIds, bool force)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<IEnumerable<FeatureDescriptor>> GetAvailableFeaturesAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<IEnumerable<string>> GetDependentFeaturesAsync(string featureId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<IEnumerable<FeatureDescriptor>> GetDisabledFeaturesAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<IEnumerable<FeatureDescriptor>> GetEnabledFeaturesAsync()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}