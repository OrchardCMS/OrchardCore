using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.OpenId.YesSql.Services
{
    public class OpenIdAuthorizationStore : IOpenIdAuthorizationStore
    {
        private readonly ISession _session;

        public OpenIdAuthorizationStore(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Determines the number of authorizations that exist in the database.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of authorizations in the database.
        /// </returns>
        public virtual async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<OpenIdAuthorization>().CountAsync();
        }

        /// <summary>
        /// Determines the number of authorizations that match the specified query.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of authorizations that match the specified query.
        /// </returns>
        public virtual Task<long> CountAsync<TResult>(Func<IQueryable<OpenIdAuthorization>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Creates a new authorization.
        /// </summary>
        /// <param name="authorization">The authorization to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task CreateAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(authorization);
            return _session.CommitAsync();
        }

        /// <summary>
        /// Removes an existing authorization.
        /// </summary>
        /// <param name="authorization">The authorization to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task DeleteAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Delete(authorization);

            return _session.CommitAsync();
        }

        /// <summary>
        /// Retrieves the authorizations corresponding to the specified
        /// subject and associated with the application identifier.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="client">The client associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the subject/client.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdAuthorization>> FindAsync(
            string subject, string client, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client cannot be null or empty.", nameof(client));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ImmutableArray.CreateRange(
                await _session.Query<OpenIdAuthorization, OpenIdAuthorizationIndex>(
                    index => index.ApplicationId == client &&
                             index.Subject == subject).ListAsync());
        }

        /// <summary>
        /// Retrieves the authorizations matching the specified parameters.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="client">The client associated with the authorization.</param>
        /// <param name="status">The status associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the subject/client.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdAuthorization>> FindAsync(
            string subject, string client, string status, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(client));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ImmutableArray.CreateRange(
                await _session.Query<OpenIdAuthorization, OpenIdAuthorizationIndex>(
                    index => index.ApplicationId == client && index.Subject == subject && index.Status == status).ListAsync());
        }

        /// <summary>
        /// Retrieves the authorizations matching the specified parameters.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="client">The client associated with the authorization.</param>
        /// <param name="status">The status associated with the authorization.</param>
        /// <param name="type">The type associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the subject/client.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdAuthorization>> FindAsync(
            string subject, string client,
            string status, string type, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The type cannot be null or empty.", nameof(client));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ImmutableArray.CreateRange(
                await _session.Query<OpenIdAuthorization, OpenIdAuthorizationIndex>(
                    index => index.ApplicationId == client && index.Subject == subject &&
                             index.Status == status && index.Type == type).ListAsync());
        }

        /// <summary>
        /// Retrieves an authorization using its unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization corresponding to the identifier.
        /// </returns>
        public virtual Task<OpenIdAuthorization> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<OpenIdAuthorization, OpenIdAuthorizationIndex>(index => index.AuthorizationId == identifier).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves an authorization using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization corresponding to the identifier.
        /// </returns>
        public virtual Task<OpenIdAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.GetAsync<OpenIdAuthorization>(int.Parse(identifier, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retrieves all the authorizations corresponding to the specified subject.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the specified subject.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdAuthorization>> FindBySubjectAsync(
            string subject, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return (await _session.Query<OpenIdAuthorization, OpenIdAuthorizationIndex>(
                index => index.Subject == subject).ListAsync()).ToImmutableArray();
        }

        /// <summary>
        /// Retrieves the optional application identifier associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the application identifier associated with the authorization.
        /// </returns>
        public virtual Task<string> GetApplicationIdAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(authorization.ApplicationId?.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Executes the specified query and returns the first element.
        /// </summary>
        /// <typeparam name="TState">The state type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="state">The optional state.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the first element returned when executing the query.
        /// </returns>
        public virtual Task<TResult> GetAsync<TState, TResult>(
            Func<IQueryable<OpenIdAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Retrieves the unique identifier associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the unique identifier associated with the authorization.
        /// </returns>
        public virtual Task<string> GetIdAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(authorization.AuthorizationId);
        }

        /// <summary>
        /// Retrieves the physical identifier associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the authorization.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(authorization.Id.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retrieves the additional properties associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose
        /// result returns all the additional properties associated with the authorization.
        /// </returns>
        public virtual Task<JObject> GetPropertiesAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(authorization.Properties ?? new JObject());
        }

        /// <summary>
        /// Retrieves the scopes associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the scopes associated with the specified authorization.
        /// </returns>
        public virtual Task<ImmutableArray<string>> GetScopesAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(authorization.Scopes);
        }

        /// <summary>
        /// Retrieves the status associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the status associated with the specified authorization.
        /// </returns>
        public virtual Task<string> GetStatusAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(authorization.Status);
        }

        /// <summary>
        /// Retrieves the subject associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the subject associated with the specified authorization.
        /// </returns>
        public virtual Task<string> GetSubjectAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(authorization.Subject);
        }

        /// <summary>
        /// Retrieves the type associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the type associated with the specified authorization.
        /// </returns>
        public virtual Task<string> GetTypeAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(authorization.Type);
        }

        /// <summary>
        /// Instantiates a new authorization.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result
        /// returns the instantiated authorization, that can be persisted in the database.
        /// </returns>
        public virtual Task<OpenIdAuthorization> InstantiateAsync(CancellationToken cancellationToken)
            => Task.FromResult(new OpenIdAuthorization { AuthorizationId = Guid.NewGuid().ToString("n") });

        /// <summary>
        /// Executes the specified query and returns all the corresponding elements.
        /// </summary>
        /// <param name="count">The number of results to return.</param>
        /// <param name="offset">The number of results to skip.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdAuthorization>> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            var query = _session.Query<OpenIdAuthorization>();

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            return ImmutableArray.CreateRange(await query.ListAsync());
        }

        /// <summary>
        /// Executes the specified query and returns all the corresponding elements.
        /// </summary>
        /// <typeparam name="TState">The state type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="state">The optional state.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        public virtual Task<ImmutableArray<TResult>> ListAsync<TState, TResult>(
            Func<IQueryable<OpenIdAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Lists the ad-hoc authorizations that are marked as invalid or have no
        /// valid token attached and that can be safely removed from the database.
        /// </summary>
        /// <param name="count">The number of results to return.</param>
        /// <param name="offset">The number of results to skip.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdAuthorization>> ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            IQuery<OpenIdAuthorization> query = _session.Query<OpenIdAuthorization, OpenIdAuthorizationIndex>(
                authorization => authorization.Status != OpenIddictConstants.Statuses.Valid ||
               (authorization.Type == OpenIddictConstants.AuthorizationTypes.AdHoc &&
                authorization.AuthorizationId.IsNotIn<OpenIdTokenIndex>(
                    token => token.AuthorizationId,
                    token => token.Status == OpenIddictConstants.Statuses.Valid)));

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            return ImmutableArray.CreateRange(await query.ListAsync());
        }

        /// <summary>
        /// Sets the application identifier associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="identifier">The unique identifier associated with the client application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetApplicationIdAsync(OpenIdAuthorization authorization,
            string identifier, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (string.IsNullOrEmpty(identifier))
            {
                authorization.ApplicationId = null;
            }
            else
            {
                authorization.ApplicationId = identifier;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the additional properties associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="properties">The additional properties associated with the authorization </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetPropertiesAsync(OpenIdAuthorization authorization, JObject properties, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Properties = properties;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the scopes associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="scopes">The scopes associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetScopesAsync(OpenIdAuthorization authorization,
            ImmutableArray<string> scopes, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Scopes = scopes;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the status associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="status">The status associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetStatusAsync(OpenIdAuthorization authorization,
            string status, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Status = status;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the subject associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetSubjectAsync(OpenIdAuthorization authorization,
            string subject, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Subject = subject;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the type associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="type">The type associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetTypeAsync(OpenIdAuthorization authorization,
            string type, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Type = type;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates an existing authorization.
        /// </summary>
        /// <param name="authorization">The authorization to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task UpdateAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(authorization);

            return _session.CommitAsync();
        }

        // Note: the following methods are deliberately implemented as explicit methods so they are not
        // exposed by Intellisense. Their logic MUST be limited to dealing with casts and downcasts.
        // Developers who need to customize the logic SHOULD override the methods taking concretes types.

        // ---------------------------------------------------------------
        // Methods defined by the IOpenIddictAuthorizationStore interface:
        // ---------------------------------------------------------------

        Task<long> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        Task<long> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.CountAsync<TResult>(Func<IQueryable<IOpenIdAuthorization>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.CreateAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => CreateAsync((OpenIdAuthorization) authorization, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.DeleteAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdAuthorization) authorization, cancellationToken);

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindAsync(string subject, string client, CancellationToken cancellationToken)
            => (await FindAsync(subject, client, cancellationToken)).CastArray<IOpenIdAuthorization>();

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindAsync(string subject, string client, string status, CancellationToken cancellationToken)
            => (await FindAsync(subject, client, status, cancellationToken)).CastArray<IOpenIdAuthorization>();

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindAsync(
            string subject, string client, string status, string type, CancellationToken cancellationToken)
            => (await FindAsync(subject, client, status, type, cancellationToken)).CastArray<IOpenIdAuthorization>();

        async Task<IOpenIdAuthorization> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindBySubjectAsync(string subject, CancellationToken cancellationToken)
            => (await FindBySubjectAsync(subject, cancellationToken)).CastArray<IOpenIdAuthorization>();

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetApplicationIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetApplicationIdAsync((OpenIdAuthorization) authorization, cancellationToken);

        Task<TResult> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetIdAsync((OpenIdAuthorization) authorization, cancellationToken);

        Task<JObject> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetPropertiesAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetPropertiesAsync((OpenIdAuthorization) authorization, cancellationToken);

        Task<ImmutableArray<string>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetScopesAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetScopesAsync((OpenIdAuthorization) authorization, cancellationToken);

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetStatusAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetStatusAsync((OpenIdAuthorization) authorization, cancellationToken);

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetSubjectAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetSubjectAsync((OpenIdAuthorization) authorization, cancellationToken);

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetTypeAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetTypeAsync((OpenIdAuthorization) authorization, cancellationToken);

        async Task<IOpenIdAuthorization> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdAuthorization>();

        Task<ImmutableArray<TResult>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListInvalidAsync(count, offset, cancellationToken)).CastArray<IOpenIdAuthorization>();

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetApplicationIdAsync(IOpenIdAuthorization authorization,
            string identifier, CancellationToken cancellationToken)
            => SetApplicationIdAsync((OpenIdAuthorization) authorization, identifier, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetPropertiesAsync(IOpenIdAuthorization authorization, JObject properties, CancellationToken cancellationToken)
            => SetPropertiesAsync((OpenIdAuthorization) authorization, properties, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetScopesAsync(IOpenIdAuthorization authorization,
            ImmutableArray<string> scopes, CancellationToken cancellationToken)
            => SetScopesAsync((OpenIdAuthorization) authorization, scopes, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetStatusAsync(IOpenIdAuthorization authorization,
            string status, CancellationToken cancellationToken)
            => SetStatusAsync((OpenIdAuthorization) authorization, status, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetSubjectAsync(IOpenIdAuthorization authorization,
            string subject, CancellationToken cancellationToken)
            => SetSubjectAsync((OpenIdAuthorization) authorization, subject, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetTypeAsync(IOpenIdAuthorization authorization,
            string type, CancellationToken cancellationToken)
            => SetTypeAsync((OpenIdAuthorization) authorization, type, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.UpdateAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdAuthorization) authorization, cancellationToken);

        // -----------------------------------------------------------
        // Methods defined by the IOpenIdAuthorizationStore interface:
        // -----------------------------------------------------------

        async Task<IOpenIdAuthorization> IOpenIdAuthorizationStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<string> IOpenIdAuthorizationStore.GetPhysicalIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((OpenIdAuthorization) authorization, cancellationToken);
    }
}
