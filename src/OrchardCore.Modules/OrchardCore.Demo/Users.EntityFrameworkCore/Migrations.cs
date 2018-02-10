using System;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace OrchardCore.Demo.Users.EntityFrameworkCore
{
    [Feature("OrchardCore.Demo.Users.EntityFrameworkCore")]
    public class Migrations : DataMigration
    {
        public int Create()
        {
            string userRoleAssociacionTableName = "AspNetUserRoles",
                roleTablename = "AspNetRoles",
                userTokenTableName = "AspNetUserTokens",
                userTablename = "AspNetUsers",
                roleClaimTablename = "AspNetRoleClaims",
                userClaimTablename = "AspNetUserClaims",
                userLoginTablename = "AspNetUserLogins";


            SchemaBuilder
                .CreateTable(roleTablename,
                    table => table
                        .Column<int>(nameof(IdentityRole.Id), column => column.PrimaryKey().Identity())
                        .Column<string>(nameof(IdentityRole.ConcurrencyStamp), column => column.Nullable())
                        .Column<string>(nameof(IdentityRole.Name), column => column.NotNull().WithLength(256))
                        .Column<string>(nameof(IdentityRole.NormalizedName), column => column.WithLength(256))
                );

            SchemaBuilder
                .CreateTable(userTokenTableName,
                    table => table
                        .Column<int>("UserId", column => column.NotNull())
                        .Column<string>("LoginProvider", column => column.NotNull().WithLength(256))
                        .Column<string>("Name", column => column.NotNull().WithLength(256))
                        .Column<string>("Value", column => column.WithLength(256))
                );

            SchemaBuilder.AlterTable(userTokenTableName,
                command => command.CreateIndex("PK_AspNetUserTokens", "UserId", "LoginProvider", "Name"));


            SchemaBuilder.CreateTable(userTablename, table => table
                .Column<int>(nameof(IdentityUser.Id), column => column.PrimaryKey().Identity())
                .Column<int>(nameof(IdentityUser.AccessFailedCount), column=>column.NotNull())
                .Column<string>(nameof(IdentityUser.ConcurrencyStamp), column=>column.Nullable())
                .Column<string>(nameof(IdentityUser.Email), column => column.Nullable().WithLength(256))
                .Column<bool>(nameof(IdentityUser.EmailConfirmed), column => column.NotNull())
                .Column<bool>(nameof(IdentityUser.LockoutEnabled), column => column.NotNull())
                .Column<DateTimeOffset>(nameof(IdentityUser.LockoutEnd), column => column.Nullable())
                .Column<string>(nameof(IdentityUser.NormalizedEmail), column => column.Nullable().WithLength(256))
                .Column<string>(nameof(IdentityUser.NormalizedUserName), column => column.Nullable().WithLength(256))
                .Column<string>(nameof(IdentityUser.PasswordHash), column => column.Nullable())
                .Column<string>(nameof(IdentityUser.PhoneNumber), column => column.Nullable())
                .Column<bool>(nameof(IdentityUser.PhoneNumberConfirmed), column => column.NotNull().WithDefault(0))
                .Column<string>(nameof(IdentityUser.SecurityStamp), column => column.Nullable())
                .Column<bool>(nameof(IdentityUser.TwoFactorEnabled), column => column.NotNull().WithDefault(0))
                .Column<string>(nameof(IdentityUser.UserName), column => column.Nullable().WithLength(256))
                );


            SchemaBuilder
                .CreateTable(roleClaimTablename,
                    table => table
                        .Column<int>("Id", column => column.PrimaryKey().Identity())
                        .Column<string>("ClaimType", column => column.NotNull().WithLength(128))
                        .Column<string>("ClaimValue", column => column.NotNull().WithLength(128))
                        .Column<int>("RoleId", column => column.NotNull())
                )
                .CreateForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", roleClaimTablename, new[] { "RoleId" }, roleTablename, new[] { "Id" });//should be Cascade deletion

            SchemaBuilder
                .CreateTable(userClaimTablename,
                    table => table
                        .Column<int>("Id", column => column.PrimaryKey().Identity())
                        .Column<string>("ClaimType", column => column.NotNull().WithLength(128))
                        .Column<string>("ClaimValue", column => column.NotNull().WithLength(128))
                        .Column<int>("UserId", column => column.NotNull())
                )
                .CreateForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", userClaimTablename, new[] { "UserId" }, userTablename, new[] { "Id" });//should be Cascade deletion

            SchemaBuilder
                .CreateTable(userLoginTablename,
                    table => table
                        .Column<string>("LoginProvider", column => column.NotNull())
                        .Column<string>("ProviderKey", column => column.NotNull())
                        .Column<string>("ProviderDisplayName", column => column.Nullable())
                        .Column<int>("UserId", column => column.NotNull())
                )
                .CreateForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", userLoginTablename, new[] { "UserId" }, userTablename, new[] { "Id" });

            SchemaBuilder.AlterTable(userLoginTablename,
                command => command.CreateIndex("PK_AspNetUserLogins", "LoginProvider", "ProviderKey"));//should be Cascade deletion

            SchemaBuilder.CreateTable(userRoleAssociacionTableName, table => table
                    .Column<int>("UserId", command => command.NotNull().PrimaryKey().NotNull())
                    .Column<int>("RoleId", command => command.NotNull().PrimaryKey().NotNull()))
                .CreateForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", userRoleAssociacionTableName,
                    new[] {"RoleId"}, roleTablename, new[] {"Id"}) //should be Cascade deletion
                .CreateForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", userRoleAssociacionTableName,
                    new[] {"UserId"}, userTablename, new[] {"Id"});//should be Cascade deletion

            return 1;
        }
    }
}
