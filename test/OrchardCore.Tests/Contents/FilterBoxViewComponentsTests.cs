using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewComponents;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Email;
using OrchardCore.Environment.Extensions;
using OrchardCore.Tests.Stubs;
using OrchardCore.Users.Models;
using Xunit;

namespace OrchardCore.Tests.Contents
{
    public class FilterBoxViewComponentsTests
    {
        [Fact]
        public async Task ShouldReturnNullWhenPassedNullViewModel()
        {
            // Arrange
            var filterBoxVC = GetDefaultFilterBoxViewComponent();


            // Act
            var result = await filterBoxVC.InvokeAsync(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldReturnNullWhenUserIsNull()
        {
            // Arrange
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(hca => hca.HttpContext.User).Returns<User>(null);

            var filterBoxVC = new FilterBoxViewComponent(
                Mock.Of<IContentManager>(),
                Mock.Of<IContentDefinitionManager>(),
                Mock.Of<IAuthorizationService>(),
                mockHttpContextAccessor.Object,
                Mock.Of<IUserContentTypesProvider>(),
                Mock.Of<IStringLocalizer<FilterBoxViewComponent>>());

            // Act
            var result = await filterBoxVC.InvokeAsync(Mock.Of<FilterBoxViewModel>());


            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldReturnNullWhenOptionsAreNull()
        {
            // Arrange
            var vm = new FilterBoxViewModel { Options = null };
            var filterBoxVC = GetDefaultFilterBoxViewComponent();

            // Act
            var result = await filterBoxVC.InvokeAsync(vm);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldProduceACorrectContentStatusSelectList()
        {
            // Arrange
            var filterBoxVM = new FilterBoxViewModel { Options = new ContentOptions { ContentsStatus = ContentsStatus.Draft } };
            var filterBoxVC = GetDefaultFilterBoxViewComponent();

            // Act
            var result = await filterBoxVC.InvokeAsync(filterBoxVM);

            // Assert            
            var viewComponentResult = Assert.IsType<ViewViewComponentResult>(result);
            var model = Assert.IsType<FilterBoxViewModel>(viewComponentResult.ViewData.Model);
            List<SelectListItem> Statuses = Assert.IsType<List<SelectListItem>>(model.ContentStatuses);
            Assert.Single(Statuses.Where(s => s.Selected == true));
            Assert.Equal(ContentsStatus.Draft.ToString(), Statuses.Where(s => s.Selected == true).FirstOrDefault().Value);
        }

        [Fact]
        public async Task ShouldProduceACorrectContentSortsSelectList()
        {
            // Arrange

            var filterBoxVM = new FilterBoxViewModel {

                Options = new ContentOptions { OrderBy = ContentsOrder.Modified }
            };
            var filterBoxVC = GetDefaultFilterBoxViewComponent();

            // Act
            var result = await filterBoxVC.InvokeAsync(filterBoxVM);

            // Assert            
            var viewComponentResult = Assert.IsType<ViewViewComponentResult>(result);
            var model = Assert.IsType<FilterBoxViewModel>(viewComponentResult.ViewData.Model);
            List<SelectListItem> ContentSorts = Assert.IsType<List<SelectListItem>>(model.ContentSorts);
            Assert.Single(ContentSorts.Where(s => s.Selected == true));
            Assert.Equal(ContentsOrder.Modified.ToString(), ContentSorts.Where(s => s.Selected == true).FirstOrDefault().Value);
        }

        [Fact]
        public async Task ShouldProduceACorrectSortDirectionsSelectList()
        {
            // Arrange
            var filterBoxVM = new FilterBoxViewModel { Options = new ContentOptions { SortDirection = SortDirection.Ascending } };
            var filterBoxVC = GetDefaultFilterBoxViewComponent();

            // Act
            var result = await filterBoxVC.InvokeAsync(filterBoxVM);

            // Assert            
            var viewComponentResult = Assert.IsType<ViewViewComponentResult>(result);
            var model = Assert.IsType<FilterBoxViewModel>(viewComponentResult.ViewData.Model);
            List<SelectListItem> SortDirections = Assert.IsType<List<SelectListItem>>(model.SortDirections);
            Assert.Single(SortDirections.Where(s => s.Selected == true));
            Assert.Equal(SortDirection.Ascending.ToString(), SortDirections.Where(s => s.Selected == true).FirstOrDefault().Value);
        }

        [Fact]
        public async Task ShouldProduceACorrectContentTypesSelectList()
        {
            // Arrange
            var filterBoxVM = new FilterBoxViewModel { Options = new ContentOptions { TypeName = "listable2" } };
            var filterBoxVC = GetDefaultFilterBoxViewComponent();  // One of its listable content types will be named "listable2"

            // Act
            var result = await filterBoxVC.InvokeAsync(filterBoxVM);

            // Assert            
            var viewComponentResult = Assert.IsType<ViewViewComponentResult>(result);
            var model = Assert.IsType<FilterBoxViewModel>(viewComponentResult.ViewData.Model);
            List<SelectListItem> ContentTypes = Assert.IsType<List<SelectListItem>>(model.ContentTypes);
            Assert.Single(ContentTypes.Where(s => s.Selected == true));
            Assert.Equal("listable2", ContentTypes.Where(s => s.Selected == true).FirstOrDefault().Value);
        }

        [Fact]
        public async Task ShouldKeepFilterByOwnerSelection()
        {
            // Arrange
            var filterBoxVM = new FilterBoxViewModel { Options = { OwnedByMe = true } };
            var filterBoxVC = GetDefaultFilterBoxViewComponent();

            // Act
            var result = await filterBoxVC.InvokeAsync(filterBoxVM);

            // Assert            
            var viewComponentResult = Assert.IsType<ViewViewComponentResult>(result);
            var model = Assert.IsType<FilterBoxViewModel>(viewComponentResult.ViewData.Model);
            bool OwnedByMe = Assert.IsType<bool>(model.Options.OwnedByMe);
            Assert.True(OwnedByMe);            
        }

        [Fact]
        public async Task ShouldInitializeOwnedByMeToFalseByDefault()
        {
            // Arrange
            var filterBoxVM = new FilterBoxViewModel();
            var filterBoxVC = GetDefaultFilterBoxViewComponent();

            // Act
            var result = await filterBoxVC.InvokeAsync(filterBoxVM);

            // Assert            
            var viewComponentResult = Assert.IsType<ViewViewComponentResult>(result);
            var model = Assert.IsType<FilterBoxViewModel>(viewComponentResult.ViewData.Model);
            bool OwnedByMe = Assert.IsType<bool>(model.Options.OwnedByMe);
            Assert.False(OwnedByMe);
        }
        // Should Initialize OwnedByMe to false by default
        #region helpers


        private FilterBoxViewComponent GetDefaultFilterBoxViewComponent()
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(hca => hca.HttpContext.User).Returns(Mock.Of<ClaimsPrincipal>());

            // Setting up string localizer to avoid NRE's
            var mockStringLocalizer = new Mock<IStringLocalizer<FilterBoxViewComponent>>();
            mockStringLocalizer.Setup(s => s[It.IsAny<string>()])
                           .Returns<string>(s => new LocalizedString(s, s));


            var filterBoxVC = new FilterBoxViewComponent(
                Mock.Of<IContentManager>(),
                Mock.Of<IContentDefinitionManager>(),
                Mock.Of<IAuthorizationService>(),
                mockHttpContextAccessor.Object,
                new MockContentTypesProvider(),
                mockStringLocalizer.Object);

            return filterBoxVC;
        }

        private class MockContentTypesProvider : IUserContentTypesProvider
        {
            public Task<IEnumerable<ContentTypeDefinition>> GetCreatableTypesAsync(ClaimsPrincipal user)
            {
                throw new NotImplementedException();
            }

            public  Task<IEnumerable<ContentTypeDefinition>> GetListableTypesAsync(ClaimsPrincipal user)
            {
                List<ContentTypeDefinition> contentTypes = new List<ContentTypeDefinition>();
                contentTypes.Add(new ContentTypeDefinition("listable1", "listable1"));
                contentTypes.Add(new ContentTypeDefinition("listable2", "listable2"));
                contentTypes.Add(new ContentTypeDefinition("listable3", "listable3"));
                return Task.FromResult(contentTypes.AsEnumerable());
            }
        }

        #endregion
    }
}
