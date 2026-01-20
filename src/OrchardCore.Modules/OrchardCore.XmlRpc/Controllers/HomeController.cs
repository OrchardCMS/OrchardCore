using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.XmlRpc.Models;
using OrchardCore.XmlRpc.Services;

namespace OrchardCore.XmlRpc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IXmlRpcWriter _writer;
        private readonly IEnumerable<IXmlRpcHandler> _xmlRpcHandlers;
        private readonly ILogger _logger;

        public HomeController(
            IXmlRpcWriter writer,
            IEnumerable<IXmlRpcHandler> xmlRpcHandlers,
            ILogger<HomeController> logger)
        {
            _writer = writer;
            _xmlRpcHandlers = xmlRpcHandlers;
            _logger = logger;
        }

        [HttpPost, ActionName("Index")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ServiceEndpoint([ModelBinder(BinderType = typeof(MethodCallModelBinder))] XRpcMethodCall methodCall)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("XmlRpc method '{XmlRpcMethodName}' invoked", methodCall.MethodName);
            }

            var methodResponse = await DispatchAsync(methodCall);

            if (methodResponse == null)
            {
                return StatusCode(500, "TODO: xmlrpc fault");
            }

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false,
                Indent = true
            };

            // Save to an intermediate MemoryStream to preserve the encoding declaration.
            using var stream = new MemoryStream();
            using (var w = XmlWriter.Create(stream, settings))
            {
                var result = _writer.MapMethodResponse(methodResponse);
                result.Save(w);
            }

            var content = Encoding.UTF8.GetString(stream.ToArray());
            return Content(content, "text/xml");
        }

        private async Task<XRpcMethodResponse> DispatchAsync(XRpcMethodCall request)
        {
            var context = new XmlRpcContext
            {
                Url = Url,
                ControllerContext = ControllerContext,
                HttpContext = HttpContext,
                RpcMethodCall = request
            };

            try
            {
                foreach (var handler in _xmlRpcHandlers)
                {
                    await handler.ProcessAsync(context);
                }
            }
            catch (Exception e)
            {
                // If a core exception is raised, report the error message, otherwise signal a 500.
                context.RpcMethodResponse ??= new XRpcMethodResponse();
                context.RpcMethodResponse.Fault = new XRpcFault(0, e.Message);
            }

            return context.RpcMethodResponse;
        }
    }
}
