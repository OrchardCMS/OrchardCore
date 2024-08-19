using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Forms.Helpers;

public static class ModelStateHelpers
{
    public static string SerializeModelState(ModelStateDictionary modelState)
    {
        var errorList = modelState.Select(x => new ModelStateTransferValue
        {
            Key = x.Key,
            AttemptedValue = x.Value.AttemptedValue,
            RawValue = x.Value.RawValue,
            ErrorMessages = x.Value.Errors.Select(err => err.ErrorMessage).ToList(),
        });

        return JConvert.SerializeObject(errorList);
    }

    public static ModelStateDictionary DeserializeModelState(string serialisedErrorList)
    {
        var errorList = JConvert.DeserializeObject<List<ModelStateTransferValue>>(serialisedErrorList);
        var modelState = new ModelStateDictionary();

        foreach (var item in errorList)
        {
            item.RawValue = item.RawValue is JsonArray jarray ? jarray.ToObject<object[]>() : item.RawValue;
            modelState.SetModelValue(item.Key, item.RawValue, item.AttemptedValue);
            foreach (var error in item.ErrorMessages)
            {
                modelState.AddModelError(item.Key, error);
            }
        }
        return modelState;
    }

    private sealed class ModelStateTransferValue
    {
        public string Key { get; set; }
        public string AttemptedValue { get; set; }
        public object RawValue { get; set; }
        public ICollection<string> ErrorMessages { get; set; } = [];
    }
}
