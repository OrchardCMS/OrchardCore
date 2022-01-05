using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Drivers
{
    public class FeatureProfileDisplayDriver : DisplayDriver<FeatureProfile>
    {
        private readonly FeatureProfilesManager _featureProfilesManager;
        private readonly IStringLocalizer S;

        public FeatureProfileDisplayDriver(FeatureProfilesManager featueProfilesManager,
            IStringLocalizer<FeatureProfileDisplayDriver> stringLocalizer)
        {
            _featureProfilesManager = featueProfilesManager;
            S = stringLocalizer;
        }

        public override Task<IDisplayResult> EditAsync(FeatureProfile model, BuildEditorContext context)
        {
            if (context.IsNew)
            {
                return Task.FromResult<IDisplayResult>(Initialize<FeatureProfileViewModel>(nameof(FeatureProfileViewModel), vm =>
                {
                    vm.Id = IdGenerator.GenerateId();
                }).Location("Content"));
            }

            return Task.FromResult<IDisplayResult>(Initialize<FeatureProfileViewModel>(nameof(FeatureProfileViewModel), vm =>
            {
                vm.Id = model.Id ?? model.Name;
                vm.Name = model.Name;
                vm.FeatureRules = JsonConvert.SerializeObject(model.FeatureRules, Formatting.Indented);
            }).Location("Content"));

        }

        public override async Task<IDisplayResult> UpdateAsync(FeatureProfile model, UpdateEditorContext context)
        {
            var vm = new FeatureProfileViewModel();

            if (await context.Updater.TryUpdateModelAsync(vm, Prefix))
            {
                var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

                if (FeatureExists(model, featureProfilesDocument, context.IsNew))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(FeatureProfileViewModel.Name), S["A feature profile with the same name already exists."]);
                }
                else
                {
                    try
                    {
                        model.Id = vm.Id;
                        model.Name = vm.Name;
                        model.FeatureRules = JsonConvert.DeserializeObject<List<FeatureRule>>(vm.FeatureRules);
                    }
                    catch (Exception)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(FeatureProfileViewModel.FeatureRules), S["Invalid json supplied."]);
                    }
                }
            }

            return Edit(model);
        }

        private static bool FeatureExists(FeatureProfile model, FeatureProfilesDocument featureProfilesDocument, bool isNew)
        {
            var profiles = featureProfilesDocument.FeatureProfiles.Where(x => (x.Value.Name ?? x.Key).Equals(model.Name, StringComparison.OrdinalIgnoreCase));

            if (isNew)
            {
                return profiles.Any();
            }

            return profiles.Any(x => x.Key != model.Id);
        }
    }
}
