using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.Extensions
{
    public static class FlowMetadataExtemtions
    {
        public static int ToColumnSize(  this FlowMetadata metadata)
        {
            var colSize = 12;

            switch (metadata.Size)
            {
                case 25:
                    colSize = 3;
                    break;
                case 33:
                    colSize = 4;
                    break;
                case 50:
                    colSize = 6;
                    break;
                case 66:
                    colSize = 8;
                    break;
                case 75:
                    colSize = 9;
                    break;
                default:
                    colSize = 12;
                    break;

            }
            return colSize;
        }
    }
}
