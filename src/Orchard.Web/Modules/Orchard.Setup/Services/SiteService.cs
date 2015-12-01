//using Orchard.ContentManagement;
//using Orchard.Core.Settings;
//using Orchard.Core.Settings.Services;
//using System;
//using System.Threading.Tasks;

//namespace Orchard.Setup.Services
//{
//    /// <summary>
//    /// <see cref="ISiteService"/> implementation used for Setup.
//    /// Returns a predefined object with default values for Setup.
//    /// </summary>
//    class SafeModeSiteService : ISiteService
//    {
//        public Task<ISite> GetSiteSettingsAsync()
//        {
//            return Task.FromResult<ISite>(new SafeModeSite());
//        }
//    }

//    class SafeModeSite : ContentPart, ISite
//    {
//        public string PageTitleSeparator
//        {
//            get { return " - "; }
//        }

//        public string SiteName
//        {
//            get { return "Orchard Setup"; }
//        }

//        public string SiteSalt
//        {
//            get { return "42"; }
//        }

//        public string SiteUrl
//        {
//            get { return "/"; }
//        }

//        public string SuperUser
//        {
//            get { return ""; }
//        }

//        public string HomePage
//        {
//            get { return ""; }
//            set { throw new NotImplementedException(); }
//        }

//        public string Culture
//        {
//            get { return ""; }
//            set { throw new NotImplementedException(); }
//        }

//        public string Calendar
//        {
//            get { return ""; }
//            set { throw new NotImplementedException(); }
//        }

//        //public ResourceDebugMode ResourceDebugMode
//        //{
//        //    get { return ResourceDebugMode.FromAppSetting; }
//        //    set { throw new NotImplementedException(); }
//        //}

//        public bool UseCdn
//        {
//            get { return false; }
//            set { throw new NotImplementedException(); }
//        }

//        public int PageSize
//        {
//            get { throw new NotImplementedException(); }
//            set { throw new NotImplementedException(); }
//        }

//        public int MaxPageSize
//        {
//            get { throw new NotImplementedException(); }
//            set { throw new NotImplementedException(); }
//        }

//        public int MaxPagedCount
//        {
//            get { return 0; }
//            set { throw new NotImplementedException(); }
//        }

//        public string BaseUrl
//        {
//            get { return ""; }
//        }

//        public string TimeZone
//        {
//            get { return TimeZoneInfo.Local.Id; }
//        }
//    }
//}
