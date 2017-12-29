using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public virtual Task<long> CountAsync<TResult>(Func<IQueryable<IOpenIdAuthorization>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Creates a new authorization.
        /// </summary>
        /// <param name="authorization">The authorization to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result returns the authorization.
        /// </returns>
        public virtual async Task<IOpenIdAuthorization> CreateAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(authorization);
            await _session.CommitAsync();

            return authorization;
        }

        /// <summary>
        /// Removes an existing authorization.
        /// </summary>
        /// <param name="authorization">The authorization to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task DeleteAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
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
        public virtual async Task<ImmutableArray<IOpenIdAuthorization>> FindAsync(string subject, string client, CancellationToken cancellationToken)
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

            return ImmutableArray.CreateRange<IOpenIdAuthorization>(
                await _session.Query<OpenIdAuthorization, OpenIdAuthorizationIndex>(
                    index => index.ApplicationId == client &&
                             index.Subject == subject).ListAsync());
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
        public virtual async Task<IOpenIdAuthorization> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<OpenIdAuthorization, OpenIdAuthorizationIndex>(index => index.AuthorizationId == identifier).FirstOrDefaultAsync();
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
        public virtual async Task<IOpenIdAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _session.GetAsync<OpenIdAuthorization>(int.Parse(identifier, CultureInfo.InvariantCulture));
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
        public virtual Task<string> GetApplicationIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(((OpenIdAuthorization) authorization).ApplicationId?.ToString(CultureInfo.InvariantCulture));
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
            Func<IQueryable<IOpenIdAuthorization>, TState, IQueryable<TResult>> query,
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
        public virtual Task<string> GetIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(((OpenIdAuthorization) authorization).AuthorizationId);
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
        public virtual Task<string> GetPhysicalIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(((OpenIdAuthorization) authorization).Id.ToString(CultureInfo.InvariantCulture));
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
        public virtual Task<ImmutableArray<string>> GetScopesAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(ImmutableArray.CreateRange(((OpenIdAuthorization) authorization).Scopes));
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
        public virtual Task<string> GetStatusAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(((OpenIdAuthorization) authorization).Status);
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
        public virtual Task<string> GetSubjectAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(((OpenIdAuthorization) authorization).Subject);
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
        public virtual Task<string> GetTypeAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Task.FromResult(((OpenIdAuthorization) authorization).Type);
        }

        /// <summary>
        /// Instantiates a new authorization.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result
        /// returns the instantiated authorization, that can be persisted in the database.
        /// </returns>
        public virtual Task<IOpenIdAuthorization> InstantiateAsync(CancellationToken cancellationToken)
            => Task.FromResult<IOpenIdAuthorization>(new OpenIdAuthorization { AuthorizationId = Guid.NewGuid().ToString("n") });

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
        public virtual async Task<ImmutableArray<IOpenIdAuthorization>> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
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

            return ImmutableArray.CreateRange<IOpenIdAuthorization>(await query.ListAsync());
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
            Func<IQueryable<IOpenIdAuthorization>, TState, IQueryable<TResult>> query,
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
        public virtual async Task<ImmutableArray<IOpenIdAuthorization>> ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
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

            return ImmutableArray.CreateRange<IOpenIdAuthorization>(await query.ListAsync());
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
        public virtual Task SetApplicationIdAsync(IOpenIdAuthorization authorization,
            string identifier, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (string.IsNullOrEmpty(identifier))
            {
                ((OpenIdAuthorization) authorization).ApplicationId = null;
            }
            else
            {
                ((OpenIdAuthorization) authorization).ApplicationId = identifier;
            }

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
        public virtual Task SetScopesAsync(IOpenIdAuthorization authorization,
            ImmutableArray<string> scopes, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            ((OpenIdAuthorization) authorization).Scopes = new HashSet<string>(scopes);

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
        public virtual Task SetStatusAsync(IOpenIdAuthorization authorization,
            string status, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            ((OpenIdAuthorization) authorization).Status = status;

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
        public virtual Task SetSubjectAsync(IOpenIdAuthorization authorization,
            string subject, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            ((OpenIdAuthorization) authorization).Subject = subject;

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
        public virtual Task SetTypeAsync(IOpenIdAuthorization authorization,
            string type, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            ((OpenIdAuthorization) authorization).Type = type;

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
        public virtual Task UpdateAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(authorization);

            return _session.CommitAsync();
        }
    }
}
