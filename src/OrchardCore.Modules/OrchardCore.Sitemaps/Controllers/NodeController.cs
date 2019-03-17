using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Controllers
{
    [Admin]
    public class NodeController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<SitemapNode> _displayManager;
        private readonly IEnumerable<ISitemapNodeProviderFactory> _factories;
        private readonly ISitemapSetService _sitemapSetService;
        private readonly INotifier _notifier;

        public NodeController(
            IAuthorizationService authorizationService,
            IDisplayManager<SitemapNode> displayManager,
            IEnumerable<ISitemapNodeProviderFactory> factories,
            ISitemapSetService sitemapSetService,
            IShapeFactory shapeFactory,
            IStringLocalizer<NodeController> stringLocalizer,
            IHtmlLocalizer<NodeController> htmlLocalizer,
            INotifier notifier)
        {
            _displayManager = displayManager;
            _factories = factories;
            _sitemapSetService = sitemapSetService;
            _authorizationService = authorizationService;

            New = shapeFactory;
            _notifier = notifier;
            T = stringLocalizer;
            H = htmlLocalizer;
        }

        public dynamic New { get; set; }
        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }


        public async Task<IActionResult> List(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            return View(await BuildDisplayViewModel(tree));
        }

        private async Task<SitemapNodeListViewModel> BuildDisplayViewModel(Models.SitemapSet tree)
        {
            var thumbnails = new Dictionary<string, dynamic>();
            foreach (var factory in _factories)
            {
                var treeNode = factory.Create();
                dynamic thumbnail = await _displayManager.BuildDisplayAsync(treeNode, this, "TreeThumbnail");
                thumbnail.TreeNode = treeNode;
                thumbnails.Add(factory.Name, thumbnail);
            }

            var model = new SitemapNodeListViewModel
            {
                SitemapSet = tree,
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

            var tree = await _sitemapSetService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            var treeNode = _factories.FirstOrDefault(x => x.Name == type)?.Create();

            if (treeNode == null)
            {
                return NotFound();
            }

            var model = new SitemapNodeEditViewModel
            {
                SitemapSetId = id,
                SitemapNode = treeNode,
                SitemapNodeId = treeNode.Id,
                SitemapNodeType = type,
                Editor = await _displayManager.BuildEditorAsync(treeNode, updater: this, isNew: true)
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

            var tree = await _sitemapSetService.GetByIdAsync(model.SitemapSetId);

            if (tree == null)
            {
                return NotFound();
            }

            var treeNode = _factories.FirstOrDefault(x => x.Name == model.SitemapNodeType)?.Create();

            if (treeNode == null)
            {
                return NotFound();
            }

            dynamic editor = await _displayManager.UpdateEditorAsync(treeNode, updater: this, isNew: true);
            editor.TreeNode = treeNode;

            if (ModelState.IsValid)
            {
                treeNode.Id = model.SitemapNodeId;
                tree.SitemapNodes.Add(treeNode);
                await _sitemapSetService.SaveAsync(tree);

                _notifier.Success(H["Sitemap node added successfully"]);
                return RedirectToAction("List", new { id = model.SitemapSetId });
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id, string treeNodeId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            var treeNode = tree.GetSitemapNodeById(treeNodeId);

            if (treeNode == null)
            {
                return NotFound();
            }

            var model = new SitemapNodeEditViewModel
            {
                SitemapSetId = id,
                SitemapNode = treeNode,
                SitemapNodeId = treeNode.Id,
                SitemapNodeType = treeNode.GetType().Name,
                Editor = await _displayManager.BuildEditorAsync(treeNode, updater: this, isNew: false)
            };

            model.Editor.TreeNode = treeNode;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SitemapNodeEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(model.SitemapSetId);

            if (tree == null)
            {
                return NotFound();
            }

            var treeNode = tree.GetSitemapNodeById(model.SitemapNodeId);

            if (treeNode == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(treeNode, updater: this, isNew: false);

            if (ModelState.IsValid)
            {
                await _sitemapSetService.SaveAsync(tree);

                _notifier.Success(H["Sitemap node updated successfully"]);
                return RedirectToAction(nameof(List), new { id = model.SitemapSetId });
            }

            _notifier.Error(H["The sitemap node has validation errors"]);
            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id, string treeNodeId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            var treeNode = tree.GetSitemapNodeById(treeNodeId);

            if (treeNode == null)
            {
                return NotFound();
            }

            if (tree.RemoveSitemapNode(treeNode) == false)
            {
                return new StatusCodeResult(500);
            }

            await _sitemapSetService.SaveAsync(tree);

            _notifier.Success(H["Sitemap node deleted successfully"]);

            return RedirectToAction(nameof(List), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(string id, string treeNodeId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            var treeNode = tree.GetSitemapNodeById(treeNodeId);

            if (treeNode == null)
            {
                return NotFound();
            }

            treeNode.Enabled = !treeNode.Enabled;

            await _sitemapSetService.SaveAsync(tree);

            _notifier.Success(H["Sitemap node toggled successfully"]);

            return RedirectToAction(nameof(List), new { id });
        }


        [HttpPost]
        public async Task<IActionResult> MoveNode(string treeId, string nodeToMoveId,
            string destinationNodeId, int position)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(treeId);

            if ((tree == null) || (tree.SitemapNodes == null))
            {
                return NotFound();
            }


            var nodeToMove = tree.GetSitemapNodeById(nodeToMoveId);
            if (nodeToMove == null)
            {
                return NotFound();
            }

            var destinationNode = tree.GetSitemapNodeById(destinationNodeId); // don't check for null. When null the item will be moved to the root.

            if (tree.RemoveSitemapNode(nodeToMove) == false)
            {
                return StatusCode(500);
            }

            if (tree.InsertSitemapNodeAt(nodeToMove, destinationNode, position) == false)
            {
                return StatusCode(500);
            }

            await _sitemapSetService.SaveAsync(tree);

            return Ok();
        }
    }
}

