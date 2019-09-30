using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers
{
    [Admin]
    public class NodeController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<SitemapNode> _displayManager;
        private readonly IEnumerable<ISitemapNodeProviderFactory> _factories;
        private readonly ISitemapService _sitemapService;
        private readonly INotifier _notifier;

        public NodeController(
            IAuthorizationService authorizationService,
            IDisplayManager<SitemapNode> displayManager,
            IEnumerable<ISitemapNodeProviderFactory> factories,
            ISitemapService sitemapService,
            IHtmlLocalizer<NodeController> htmlLocalizer,
            INotifier notifier)
        {
            _displayManager = displayManager;
            _factories = factories;
            _sitemapService = sitemapService;
            _authorizationService = authorizationService;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public IHtmlLocalizer H { get; }

        public async Task<IActionResult> List(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(id);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            return View(await BuildDisplayViewModel(sitemapSet));
        }

        private async Task<SitemapNodeListViewModel> BuildDisplayViewModel(SitemapSet sitemapSet)
        {
            var thumbnails = new Dictionary<string, dynamic>();
            foreach (var factory in _factories)
            {
                var treeNode = factory.Create(sitemapSet);
                dynamic thumbnail = await _displayManager.BuildDisplayAsync(treeNode, this, "TreeThumbnail");
                thumbnail.TreeNode = treeNode;
                thumbnails.Add(factory.Name, thumbnail);
            }

            var model = new SitemapNodeListViewModel
            {
                SitemapSet = sitemapSet,
                Thumbnails = thumbnails,
            };

            return model;
        }

        public async Task<IActionResult> Create(string id, string type)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(id);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            var sitemapNode = _factories.FirstOrDefault(x => x.Name == type)?.Create(sitemapSet);

            if (sitemapNode == null)
            {
                return NotFound();
            }

            var model = new SitemapNodeEditViewModel
            {
                SitemapSetId = id,
                SitemapNode = sitemapNode,
                SitemapNodeId = sitemapNode.Id,
                SitemapNodeType = type,
                Editor = await _displayManager.BuildEditorAsync(sitemapNode, updater: this, isNew: true)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SitemapNodeEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(model.SitemapSetId);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            var sitemapNode = _factories.FirstOrDefault(x => x.Name == model.SitemapNodeType)?.Create(sitemapSet);

            if (sitemapNode == null)
            {
                return NotFound();
            }

            dynamic editor = await _displayManager.UpdateEditorAsync(sitemapNode, updater: this, isNew: true);
            editor.TreeNode = sitemapNode;

            if (ModelState.IsValid)
            {
                sitemapNode.Id = model.SitemapNodeId;
                sitemapSet.SitemapNodes.Add(sitemapNode);
                _sitemapService.SaveSitemapDocument(document);

                _notifier.Success(H["Sitemap node added successfully"]);
                return RedirectToAction("List", new { id = model.SitemapSetId });
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id, string sitemapNodeId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(id);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            var sitemapNode = sitemapSet.GetSitemapNodeById(sitemapNodeId);

            if (sitemapNode == null)
            {
                return NotFound();
            }

            var model = new SitemapNodeEditViewModel
            {
                SitemapSetId = id,
                SitemapNode = sitemapNode,
                SitemapNodeId = sitemapNode.Id,
                SitemapNodeType = sitemapNode.GetType().Name,
                Editor = await _displayManager.BuildEditorAsync(sitemapNode, updater: this, isNew: false)
            };

            model.Editor.TreeNode = sitemapNode;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SitemapNodeEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(model.SitemapSetId);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            var sitemapNode = sitemapSet.GetSitemapNodeById(model.SitemapNodeId);

            if (sitemapNode == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(sitemapNode, updater: this, isNew: false);

            if (ModelState.IsValid)
            {
                _sitemapService.SaveSitemapDocument(document);
                _notifier.Success(H["Sitemap node updated successfully"]);
                return RedirectToAction(nameof(List), new { id = model.SitemapSetId });
            }

            _notifier.Error(H["The sitemap node has validation errors"]);
            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id, string sitemapNodeId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(id);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            var sitemapNode = sitemapSet.GetSitemapNodeById(sitemapNodeId);

            if (sitemapNode == null)
            {
                return NotFound();
            }

            if (sitemapSet.RemoveSitemapNode(sitemapNode) == false)
            {
                return new StatusCodeResult(500);
            }

            _sitemapService.SaveSitemapDocument(document);

            _notifier.Success(H["Sitemap node deleted successfully"]);

            return RedirectToAction(nameof(List), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(string id, string sitemapNodeId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(id);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            var sitemapNode = sitemapSet.GetSitemapNodeById(sitemapNodeId);

            if (sitemapNode == null)
            {
                return NotFound();
            }

            sitemapNode.Enabled = !sitemapNode.Enabled;

            _sitemapService.SaveSitemapDocument(document);

            _notifier.Success(H["Sitemap node toggled successfully"]);

            return RedirectToAction(nameof(List), new { id });
        }


        [HttpPost]
        public async Task<IActionResult> MoveNode(string sitemapSetId, string nodeToMoveId,
            string destinationNodeId, int position)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(sitemapSetId);

            if ((sitemapSet == null) || (sitemapSet.SitemapNodes == null))
            {
                return NotFound();
            }

            var nodeToMove = sitemapSet.GetSitemapNodeById(nodeToMoveId);
            if (nodeToMove == null)
            {
                return NotFound();
            }

            var destinationNode = sitemapSet.GetSitemapNodeById(destinationNodeId); // don't check for null. When null the item will be moved to the root.

            if (destinationNode != null && !destinationNode.CanSupportChildNodes)
            {
                return BadRequest();
            }

            if (destinationNode != null && !nodeToMove.CanBeChildNode)
            {
                return BadRequest();
            }

            if (sitemapSet.RemoveSitemapNode(nodeToMove) == false)
            {
                return StatusCode(500);
            }

            if (sitemapSet.InsertSitemapNodeAt(nodeToMove, destinationNode, position) == false)
            {
                return StatusCode(500);
            }

            _sitemapService.SaveSitemapDocument(document);

            return Ok();
        }
    }
}

