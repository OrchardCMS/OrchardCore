using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Settings.ViewModels
{
    public class TimeZoneViewModel
    {
        public string Id;
        public string DisplayName;

        public string Comment;

        public TimeZoneViewModel(string id, string displayName, string comment)
        {
            this.Id = id;
            this.DisplayName = displayName;
            this.Comment = comment;
        }
    }
}