using Amazon.Runtime;
using System.Net;

namespace OrchardCore.FileStorage.AmazonS3;

internal static class AmazonWebServiceResponseExtensions
{
    public static bool IsSuccessful(this AmazonWebServiceResponse response) =>
        response.HttpStatusCode == HttpStatusCode.OK;

    public static bool DeletedSuccessfully(this AmazonWebServiceResponse response) =>
        response.HttpStatusCode == HttpStatusCode.NoContent;
}
