using System.Net;
using Amazon.Runtime;

namespace OrchardCore.FileStorage.AmazonS3;

public static class AmazonWebServiceResponseExtensions
{
    public static bool IsSuccessful(this AmazonWebServiceResponse response) =>
        response.HttpStatusCode == HttpStatusCode.OK;

    public static bool IsDeleteSuccessful(this AmazonWebServiceResponse response) =>
        response.HttpStatusCode == HttpStatusCode.NoContent;
}
