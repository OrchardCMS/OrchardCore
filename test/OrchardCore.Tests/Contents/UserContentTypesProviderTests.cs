using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents;
using OrchardCore.Contents.Services;
using Xunit;

namespace OrchardCore.Tests.Contents
{
    public class UserContentTypesProviderTests
    {
        [Fact]
        public async Task ShouldReturnEmptyListWhenNullUserIsPassed()
        {
            // Arrange
            IUserContentTypesProvider userContentTypesProvider = new UserContentTypesProvider(
                Mock.Of<IContentDefinitionManager>(),
                Mock.Of<IAuthorizationService>(),
                Mock.Of<IContentManager>());

            // Act
            var creatable = await userContentTypesProvider.GetCreatableTypesAsync(null);
            var listable = await userContentTypesProvider.GetListableTypesAsync(null);
            // Assert
            Assert.IsAssignableFrom<IEnumerable<ContentTypeDefinition>>(creatable);
            Assert.Empty(creatable);

            Assert.IsAssignableFrom<IEnumerable<ContentTypeDefinition>>(listable);
            Assert.Empty(listable);

        }

        [Fact]
        public async Task ShouldReturnCreatableAndListableContentTypes()
        {
            // Arrange
            var mockContentDefinitionManager = new Mock<IContentDefinitionManager>();
            var defs = new List<ContentTypeDefinition>();
            defs.Add(new ContentTypeDefinitionBuilder().Creatable().Named("creatable1").Build());
            defs.Add(new ContentTypeDefinitionBuilder().Creatable().Named("creatable2").Build());
            defs.Add(new ContentTypeDefinitionBuilder().Listable().Named("listable1").Build());
            defs.Add(new ContentTypeDefinitionBuilder().Listable().Named("listable2").Build());
            defs.Add(new ContentTypeDefinitionBuilder().Named("a").Build());            
            defs.Add(new ContentTypeDefinitionBuilder().Named("b").Build());            
            defs.Add(new ContentTypeDefinitionBuilder().Named("c").Build());            
            defs.Add(new ContentTypeDefinitionBuilder().Named("d").Build());

            mockContentDefinitionManager.Setup(m => m.ListTypeDefinitions()).Returns(defs);
            

            IUserContentTypesProvider userContentTypesProvider = new UserContentTypesProvider(
                mockContentDefinitionManager.Object,
                new AlwaysSuccessAuthorizationService(),
                Mock.Of<IContentManager>());

            // Act
            IEnumerable<ContentTypeDefinition> creatable = await userContentTypesProvider.GetCreatableTypesAsync(Mock.Of<ClaimsPrincipal>());
            IEnumerable<ContentTypeDefinition> listable = await userContentTypesProvider.GetListableTypesAsync(Mock.Of<ClaimsPrincipal>());

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ContentTypeDefinition>>(creatable);
            Assert.Equal(2, creatable.ToList().Count());
            Assert.Equal("creatable1", creatable.ToList()[0].Name);
            Assert.Equal("creatable2", creatable.ToList()[1].Name);

            Assert.IsAssignableFrom<IEnumerable<ContentTypeDefinition>>(listable);
            Assert.Equal(2, listable.ToList().Count());
            Assert.Equal("listable1", listable.ToList()[0].Name);
            Assert.Equal("listable2", listable.ToList()[1].Name);
        }

        // Todo: tests for user permissions.
        
        #region helpers

        private class AlwaysSuccessAuthorizationService : IAuthorizationService
        {
            public  Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
            {
                return Task.FromResult( AuthorizationResult.Success());
            }

            public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
            {
                return Task.FromResult(AuthorizationResult.Success());
            }
        }
        #endregion
    }
}
