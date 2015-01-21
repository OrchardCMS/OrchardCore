using Microsoft.Data.Entity;
using System;

namespace OrchardVNext.Data {
    public interface IDbContextLocator : IDependency {
        DbContext For(Type entityType);
    }
}