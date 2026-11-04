using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Deployment.Endpoints.Management;

/// <summary>A bounded deployment plan query.</summary>
public sealed class DeploymentPlanListRequest
{
    /// <summary>Gets the optional name substring filter.</summary>
    [FromQuery(Name = "search")]
    public string Search { get; init; }
    /// <summary>Gets the zero-based offset, defaulting to zero.</summary>
    [FromQuery(Name = "skip"), Range(0, int.MaxValue)]
    public int? Skip { get; init; }
    /// <summary>Gets the page size, defaulting to 50 and limited to 200.</summary>
    [FromQuery(Name = "take"), Range(1, 200)]
    public int? Take { get; init; }
}

/// <summary>Creates an empty plan or changes only an existing plan's name.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DeploymentPlanNameRequest
{
    /// <summary>Gets the required name; no step configuration is changed.</summary>
    [Required]
    public string Name { get; init; }
}

/// <summary>Describes a plan without exposing step configuration or embedded data.</summary>
public sealed class DeploymentPlanResponse
{
    /// <summary>Gets the tenant-local plan identifier.</summary>
    public long Id { get; init; }
    /// <summary>Gets the plan name.</summary>
    public string Name { get; init; }
    /// <summary>Gets the number of ordered steps in the plan.</summary>
    public int StepCount { get; init; }
}

/// <summary>A page of safe plan descriptions.</summary>
public sealed class DeploymentPlanListResponse
{
    /// <summary>Gets the applied offset.</summary>
    public int Skip { get; init; }
    /// <summary>Gets the applied page size.</summary>
    public int Take { get; init; }
    /// <summary>Gets the matching count before paging.</summary>
    public int TotalCount { get; init; }
    /// <summary>Gets the plans on this page.</summary>
    public IReadOnlyList<DeploymentPlanResponse> Items { get; init; } = [];
}

/// <summary>Describes the accepted plan mutation and whether it changed state.</summary>
public sealed class DeploymentPlanWriteResponse
{
    /// <summary>Gets whether the operation changed stored state.</summary>
    public bool Changed { get; init; }
    /// <summary>Gets the resulting plan, or null after deletion.</summary>
    public DeploymentPlanResponse Plan { get; init; }
}
