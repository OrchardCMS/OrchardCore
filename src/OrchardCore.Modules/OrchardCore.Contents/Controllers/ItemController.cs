using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Contents.Controllers
{
    public class ItemController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayHelper _displayHelper;

        public ItemController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IAuthorizationService authorizationService,
            IDisplayHelper displayHelper)
        {
            _contentManager = contentManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _authorizationService = authorizationService;
            _displayHelper = displayHelper;
        }

        public async Task<IActionResult> Display(string contentItemId, string jsonPath)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return View("Display", model);
        }

        private static ConcurrentDictionary<string, ContentItem> _contentItems = new();

        public async Task<IActionResult> Display2(string contentItemId, string jsonPath)
        {
            // no database read
            if (!_contentItems.TryGetValue(contentItemId, out var contentItem))
            {
                contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);
                _contentItems.TryAdd(contentItemId, contentItem);
            }            

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return View("Display", model);
        }

        public async Task<IActionResult> Display3(string contentItemId, string jsonPath)
        {
            // no authorization
            var contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);

            if (contentItem == null)
            {
                return NotFound();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return View("Display", model);
        }

        private static ConcurrentDictionary<string, IShape> _shapes = new();

        public async Task<IActionResult> Display4(string contentItemId, string jsonPath)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            if (!_shapes.TryGetValue(contentItemId, out var model))
            {
                model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);
                _shapes[contentItemId] = model;
            }

            return View("Display", model);
        }

        public async Task<IActionResult> Display5(string contentItemId, string jsonPath)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            return Content(contentItemId);
        }

        public async Task<IActionResult> Display6(string contentItemId, string jsonPath)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);

            if (contentItem == null)
            {
                return NotFound();
            }

            return Content(contentItemId);
        }

        public async Task<IActionResult> Display7(string contentItemId, string jsonPath)
        {
            if (!_contentItems.TryGetValue(contentItemId, out var contentItem))
            {
                contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);
                _contentItems.TryAdd(contentItemId, contentItem);
            }

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            return Content(contentItemId);
        }

        public async Task<IActionResult> Display8(string contentItemId, string jsonPath)
        {
            // no rendering
            if (!_contentItems.TryGetValue(contentItemId, out var contentItem))
            {
                contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);
                _contentItems.TryAdd(contentItemId, contentItem);
            }

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return Content(model.Metadata.Type);
        }

        public async Task<IActionResult> Display9(string contentItemId, string jsonPath)
        {
            // nmo rendering, no auth
            if (!_contentItems.TryGetValue(contentItemId, out var contentItem))
            {
                contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);
                _contentItems.TryAdd(contentItemId, contentItem);
            }

            if (contentItem == null)
            {
                return NotFound();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return Content(model.Metadata.Type);
        }

        public async Task<IActionResult> Display10(string contentItemId, string jsonPath)
        {
            // nmo rendering, no auth
            if (!_contentItems.TryGetValue(contentItemId, out var contentItem))
            {
                contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);
                _contentItems.TryAdd(contentItemId, contentItem);
            }

            if (contentItem == null)
            {
                return NotFound();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return View("Display", model);
        }

        public async Task<IActionResult> Display11(string contentItemId, string jsonPath)
        {
            // nmo rendering, no auth
            if (!_contentItems.TryGetValue(contentItemId, out var contentItem))
            {
                contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);
                _contentItems.TryAdd(contentItemId, contentItem);
            }

            if (contentItem == null)
            {
                return NotFound();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return Content(content, "text/html");
        }

        public async Task<IActionResult> Display12(string contentItemId, string jsonPath)
        {
            // nmo rendering, no auth
            if (!_contentItems.TryGetValue(contentItemId, out var contentItem))
            {
                contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);
                _contentItems.TryAdd(contentItemId, contentItem);
            }

            if (contentItem == null)
            {
                return NotFound();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return Content(content + Random.Shared.Next().ToString(), "text/html");
        }

        string content = """           

            <!DOCTYPE html>
            <html lang="en-US" dir="ltr" data-bs-theme="auto" data-tenant="Default">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>demo - About</title>
                <link type="image/x-icon" rel="shortcut icon" href="/TheTheme/images/favicon.ico">

                <!-- This script can't wait till the footer -->








                <link href="/OrchardCore.Resources/Styles/bootstrap.min.css" rel="stylesheet" type="text/css" />
            <link href="/TheTheme/styles/theme.css?v=P0jaZqQA3nMqURawa9uE_5rNCAih868iJ6eNaSXIzpY" rel="stylesheet" type="text/css" />
            <link href="/OrchardCore.Resources/Vendor/fontawesome-free/css/all.css?v=MOkHlb3t2Fz0UAEbcMD5UY3AgqwffzU5wgO9vcopomQ" rel="stylesheet" type="text/css" /><script src="/OrchardCore.Resources/Scripts/js.cookie.js?v=luDs8XBpc8wFQ-5uhfPN347fKaW3G934Si4gl2B6h2U"></script>
            <script src="/TheAdmin/js/TheAdmin-header.js?v=0jYnDjV-HX6mPD9n_oWnVORVJ99WXUDIfQbDnIxwQcw"></script>

            </head>
            <body>
                <nav class="navbar navbar-expand-md fixed-top">
                    <div class="container">
                        <a class="navbar-brand" href="/#">demo</a>
                        <button type="button" class="navbar-toggler" data-bs-toggle="collapse" data-bs-target="#navbar" aria-expanded="false" aria-controls="navbar" aria-label="Toggle navigation">
                            <span class="navbar-toggler-icon"></span>
                        </button>
                        <div class="collapse navbar-collapse" id="navbar">
                            <div class="d-flex w-100 align-items-end justify-content-end justify-content-md-between flex-column flex-md-row">

            <ul class="menu menu-main-menu navbar-nav">
            <li class="nav-item"><a class="nav-link text-end" href="/">Home</a>
            </li>

            <li class="nav-item"><a class="nav-link text-end" href="/about">About</a>
            </li>
            </ul>



            <ul class="navbar-nav user-top-navbar">
                <li class="nav-item dropdown text-end">
                <a role="button" class="nav-link dropdown-toggle" id="bd-theme" aria-expanded="false" data-bs-toggle="dropdown" data-bs-display="static" aria-label="Toggle theme">
                    <span class="theme-icon-active"><i class="fa-solid fa-circle-half-stroke"></i></span>
                    <span class="d-none" id="bd-theme-text">Toggle theme</span>
                </a>
                <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="bd-theme-text">
                    <li>
                        <button type="button" class="dropdown-item" data-bs-theme-value="auto" aria-pressed="false">
                            <span class="theme-icon">
                                <i class="fa-solid fa-circle-half-stroke"></i>
                            </span>
                            <span class="ps-2">Auto</span>
                        </button>
                    </li>
                    <li>
                        <button type="button" class="dropdown-item active" data-bs-theme-value="light" aria-pressed="true">
                            <span class="theme-icon">
                                <i class="fa-solid fa-sun"></i>
                            </span>
                            <span class="ps-2">Light</span>
                        </button>
                    </li>
                    <li>
                        <button type="button" class="dropdown-item" data-bs-theme-value="dark" aria-pressed="false">
                            <span class="theme-icon">
                                <i class="fa-solid fa-moon"></i>
                            </span>
                            <span class="ps-2">Dark</span>
                        </button>
                    </li>

                </ul>
            </li>



            <li class="nav-item dropdown text-end">
                <a href="#" class="nav-link dropdown-toggle" data-bs-toggle="dropdown">
            <i class="fa-solid fa-user fa-fw" aria-hidden="true"></i> admin
                </a>

                    <ul class="dropdown-menu dropdown-menu-end">
                        <li>
                <a class="dropdown-item" href="/Admin">
                    <i class="fa fa-desktop fa-fw" aria-hidden="true"></i> Dashboard
                </a>
            </li>


                <li>
                    <a class="dropdown-item" href="/Admin/Users/Edit?returnUrl=%2FOrchardCore.Contents%2FItem%2FDisplay10%3FcontentItemId%3D4q765c25nxgzk66xt76mhg1d5k">
                        <i class="far fa-address-card" aria-hidden="true"></i> Profile
                    </a>
                </li>
            <li>
                <a class="dropdown-item" href="/ChangePassword?returnUrl=%2FOrchardCore.Contents%2FItem%2FDisplay10%3FcontentItemId%3D4q765c25nxgzk66xt76mhg1d5k">
                    <i class="fa-solid fa-key" aria-hidden="true"></i> Change password
                </a>
            </li>
            <li>
                <form method="post" class="no-multisubmit" action="/Users/LogOff">
                    <button type="submit" class="dropdown-item">
                        <i class="fa-solid fa-sign-out-alt" aria-hidden="true"></i> Log off
                    </button>
                <input name="__RequestVerificationToken" type="hidden" value="CfDJ8NLL3ORGvItBmKq3MIVFf24s_qY4uS8v10SAsNxmoDzzRXRMFq7VWNpMg8FwncXbiibLitUZAlPIgDEbXfw3HtsWrSEBwxv73UNyYeGFrRU1nXbW4nhWKjgcPkZNPJACxlyLtRE5itCELyS3K5EeG9t6zWv7D79dkWCMsPbVfCh-4_hF85rJiQwAkxUE60YdoA" /></form>
            </li>

                    </ul>
            </li>


            </ul>

                            </div>
                        </div>
                    </div>
                </nav>

                <main class="container">



            <article class="content-item article">
                <header>

            <h1>About</h1>

                </header>



            <div class="field field-type-textfield field-name-article-subtitle">
                This is what I do.
            </div>
                <div class="field field-type-mediafield field-name-article-image">
                    <span class="name">Banner Image</span>
                            <img src="/media/about-bg.jpg" alt="about-bg.jpg">
                </div>
            <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit. Saepe nostrum ullam eveniet pariatur voluptates odit, fuga atque ea nobis sit soluta odio, adipisci quas excepturi maxime quae totam ducimus consectetur?</p><p>Lorem ipsum dolor sit amet, consectetur adipisicing elit. Eius praesentium recusandae illo eaque architecto error, repellendus iusto reprehenderit, doloribus, minus sunt. Numquam at quae voluptatum in officia voluptas voluptatibus, minus!</p><p>Lorem ipsum dolor sit amet, consectetur adipisicing elit. Nostrum molestiae debitis nobis, quod sapiente qui voluptatum, placeat magni repudiandae accusantium fugit quas labore non rerum possimus, corrupti enim modi! Et.</p>

            </article>


                </main>
                    <footer>
                        <div class="container">


            <div class="widget-container">

                <div class="widget widget-raw-html">
                <div class="widget-body">


            <div class="field field-type-htmlfield field-name-raw-html-content">
                <!-- This widget is configured in the layers section -->
            <div class="container px-4 px-lg-5">
                <div class="row gx-4 gx-lg-5 justify-content-center">
                    <div class="col-md-10 col-lg-8 col-xl-7">
                        <ul class="list-inline text-center">
                            <li class="list-inline-item">
                                <a href="#!">
                                    <span class="fa-stack fa-lg">
                                        <i class="fas fa-circle fa-stack-2x" aria-hidden="true"></i>
                                        <i class="fab fa-x-twitter fa-stack-1x fa-inverse" aria-hidden="true"></i>
                                    </span>
                                </a>
                            </li>
                            <li class="list-inline-item">
                                <a href="#!">
                                    <span class="fa-stack fa-lg">
                                        <i class="fas fa-circle fa-stack-2x" aria-hidden="true"></i>
                                        <i class="fab fa-facebook-f fa-stack-1x fa-inverse" aria-hidden="true"></i>
                                    </span>
                                </a>
                            </li>
                            <li class="list-inline-item">
                                <a href="#!">
                                    <span class="fa-stack fa-lg">
                                        <i class="fas fa-circle fa-stack-2x" aria-hidden="true"></i>
                                        <i class="fab fa-github fa-stack-1x fa-inverse" aria-hidden="true"></i>
                                    </span>
                                </a>
                            </li>
                        </ul>
                        <div class="small text-center text-muted fst-italic">Copyright &copy; Your Website 2021</div>
                    </div>
                </div>
            </div>
            </div>

                </div>
            </div>

            </div>

                        </div>
                    </footer>
                <script src="/OrchardCore.Resources/Scripts/popper.js?v=fEQD1BtQJTSMts4pwOAwgZdlDGnv8YN6auh6P6CFoLc"></script>
            <script src="/OrchardCore.Resources/Scripts/bootstrap.js?v=OOCraMD8CQj03tp_2XAeqWDplf7LmlPGf78FjUmLEuA"></script>

            <!-- Visual Studio Browser Link -->
            <script type="text/javascript" src="/_vs/browserLink" async="async" id="__browserLink_initializationData" data-requestId="0a3b113af2004eb983e90d0800ccfc42" data-requestMappingFromServer="false" data-connectUrl="http://localhost:58709/d504a9a52e924213a9f09b7ec92b149e/browserLink"></script>
            <!-- End Browser Link -->
            <script src="/_framework/aspnetcore-browser-refresh.js"></script></body>
            </html>
            
            """;

        public IActionResult Display0(string contentItemId, string jsonPath)
        {
            return Content(contentItemId);
        }

        public async Task<IActionResult> Preview(string contentItemId)
        {
            if (contentItemId == null)
            {
                return NotFound();
            }

            var versionOptions = VersionOptions.Latest;

            var contentItem = await _contentManager.GetAsync(contentItemId, versionOptions);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PreviewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return View(model);
        }
    }
}
