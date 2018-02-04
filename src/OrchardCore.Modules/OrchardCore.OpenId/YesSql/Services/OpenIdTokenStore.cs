using System;
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

namespace OrchardCore.OpenId.YesSql.Services
{
    public class OpenIdTokenStore : IOpenIdTokenStore
    {
        private readonly ISession _session;

        public OpenIdTokenStore(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Determines the number of tokens that exist in the database.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of applications in the database.
        /// </returns>
        public virtual async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<OpenIdToken>().CountAsync();
        }

        /// <summary>
        /// Determines the number of tokens that match the specified query.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of tokens that match the specified query.
        /// </returns>
        public virtual Task<long> CountAsync<TResult>(Func<IQueryable<OpenIdToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <param name="token">The token to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task CreateAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(token);
            return _session.CommitAsync();
        }

        /// <summary>
        /// Removes a token.
        /// </summary>
        /// <param name="token">The token to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>A <see cref="Task"/> that can be used to monitor the asynchronous operation.</returns>
        public virtual Task DeleteAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Delete(token);

            return _session.CommitAsync();
        }

        /// <summary>
        /// Retrieves the list of tokens corresponding to the specified application identifier.
        /// </summary>
        /// <param name="identifier">The application identifier associated with the tokens.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the tokens corresponding to the specified application.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdToken>> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ImmutableArray.CreateRange(
                await _session.Query<OpenIdToken, OpenIdTokenIndex>(
                    index => index.ApplicationId == identifier).ListAsync());
        }

        /// <summary>
        /// Retrieves the list of tokens corresponding to the specified authorization identifier.
        /// </summary>
        /// <param name="identifier">The authorization identifier associated with the tokens.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the tokens corresponding to the specified authorization.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdToken>> FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ImmutableArray.CreateRange(
                await _session.Query<OpenIdToken, OpenIdTokenIndex>(
                    index => index.AuthorizationId == identifier).ListAsync());
        }

        /// <summary>
        /// Retrieves the list of tokens corresponding to the specified reference identifier.
        /// Note: the reference identifier may be hashed or encrypted for security reasons.
        /// </summary>
        /// <param name="identifier">The reference identifier associated with the tokens.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the tokens corresponding to the specified reference identifier.
        /// </returns>
        public virtual Task<OpenIdToken> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<OpenIdToken, OpenIdTokenIndex>(index => index.ReferenceId == identifier).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a token using its unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the token corresponding to the unique identifier.
        /// </returns>
        public virtual Task<OpenIdToken> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<OpenIdToken, OpenIdTokenIndex>(index => index.TokenId == identifier).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a token using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the token corresponding to the physical identifier.
        /// </returns>
        public virtual Task<OpenIdToken> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.GetAsync<OpenIdToken>(int.Parse(identifier, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retrieves the list of tokens corresponding to the specified subject.
        /// </summary>
        /// <param name="subject">The subject associated with the tokens.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the tokens corresponding to the specified subject.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdToken>> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ImmutableArray.CreateRange(await _session.Query<OpenIdToken, OpenIdTokenIndex>(index => index.Subject == subject).ListAsync());
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
            Func<IQueryable<OpenIdToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Retrieves the optional application identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the application identifier associated with the token.
        /// </returns>
        public virtual Task<string> GetApplicationIdAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.ApplicationId?.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retrieves the optional authorization identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization identifier associated with the token.
        /// </returns>
        public virtual Task<string> GetAuthorizationIdAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.AuthorizationId?.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retrieves the creation date associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the creation date associated with the specified token.
        /// </returns>
        public virtual Task<DateTimeOffset?> GetCreationDateAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.CreationDate);
        }

        /// <summary>
        /// Retrieves the expiration date associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the expiration date associated with the specified token.
        /// </returns>
        public virtual Task<DateTimeOffset?> GetExpirationDateAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.ExpirationDate);
        }

        /// <summary>
        /// Retrieves the unique identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the unique identifier associated with the token.
        /// </returns>
        public virtual Task<string> GetIdAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.TokenId);
        }

        /// <summary>
        /// Retrieves the payload associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the payload associated with the specified token.
        /// </returns>
        public virtual Task<string> GetPayloadAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.Payload);
        }

        /// <summary>
        /// Retrieves the physical identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the token.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.Id.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retrieves the additional properties associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose
        /// result returns all the additional properties associated with the token.
        /// </returns>
        public virtual Task<JObject> GetPropertiesAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.Properties ?? new JObject());
        }

        /// <summary>
        /// Retrieves the reference identifier associated with a token.
        /// Note: depending on the manager used to create the token,
        /// the reference identifier may be hashed for security reasons.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the reference identifier associated with the specified token.
        /// </returns>
        public virtual Task<string> GetReferenceIdAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.ReferenceId);
        }

        /// <summary>
        /// Retrieves the status associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the status associated with the specified token.
        /// </returns>
        public virtual Task<string> GetStatusAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.Status);
        }

        /// <summary>
        /// Retrieves the subject associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the subject associated with the specified token.
        /// </returns>
        public virtual Task<string> GetSubjectAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.Subject);
        }

        /// <summary>
        /// Retrieves the token type associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the token type associated with the specified token.
        /// </returns>
        public virtual Task<string> GetTokenTypeAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.Type);
        }

        /// <summary>
        /// Instantiates a new token.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the instantiated token, that can be persisted in the database.
        /// </returns>
        public virtual Task<OpenIdToken> InstantiateAsync(CancellationToken cancellationToken)
            => Task.FromResult(new OpenIdToken { TokenId = Guid.NewGuid().ToString("n") } );

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
        public virtual async Task<ImmutableArray<OpenIdToken>> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            var query = _session.Query<OpenIdToken>();

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
            Func<IQueryable<OpenIdToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Lists the tokens that are marked as expired or invalid
        /// and that can be safely removed from the database.
        /// </summary>
        /// <param name="count">The number of results to return.</param>
        /// <param name="offset">The number of results to skip.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        public virtual async Task<ImmutableArray<OpenIdToken>> ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            IQuery<OpenIdToken> query = _session.Query<OpenIdToken, OpenIdTokenIndex>(
                token => token.ExpirationDate < DateTimeOffset.UtcNow ||
                         token.Status != OpenIddictConstants.Statuses.Valid);

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
        /// Sets the application identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="identifier">The unique identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetApplicationIdAsync(OpenIdToken token, string identifier, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(identifier))
            {
                token.ApplicationId = null;
            }
            else
            {
                token.ApplicationId = identifier;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the authorization identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="identifier">The unique identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetAuthorizationIdAsync(OpenIdToken token, string identifier, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(identifier))
            {
                token.AuthorizationId = null;
            }
            else
            {
                token.AuthorizationId = identifier;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the creation date associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="date">The creation date.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetCreationDateAsync(OpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.CreationDate = date?.UtcDateTime;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the expiration date associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="date">The expiration date.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetExpirationDateAsync(OpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.ExpirationDate = date?.UtcDateTime;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the payload associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="payload">The payload associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetPayloadAsync(OpenIdToken token, string payload, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.Payload = payload;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the additional properties associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="properties">The additional properties associated with the token </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetPropertiesAsync(OpenIdToken token, JObject properties, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.Properties = properties;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the reference identifier associated with a token.
        /// Note: depending on the manager used to create the token,
        /// the reference identifier may be hashed for security reasons.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="identifier">The reference identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetReferenceIdAsync(OpenIdToken token, string identifier, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.ReferenceId = identifier;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the status associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="status">The status associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetStatusAsync(OpenIdToken token, string status, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(status));
            }

            token.Status = status;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the subject associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="subject">The subject associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetSubjectAsync(OpenIdToken token, string subject, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            token.Subject = subject;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the token type associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="type">The token type associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetTokenTypeAsync(OpenIdToken token, string type, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The token type cannot be null or empty.", nameof(type));
            }

            token.Type = type;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates an existing token.
        /// </summary>
        /// <param name="token">The token to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task UpdateAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(token);

            return _session.CommitAsync();
        }

        // Note: the following methods are deliberately implemented as explicit methods so they are not
        // exposed by Intellisense. Their logic MUST be limited to dealing with casts and downcasts.
        // Developers who need to customize the logic SHOULD override the methods taking concretes types.

        // -------------------------------------------------------
        // Methods defined by the IOpenIddictTokenStore interface:
        // -------------------------------------------------------

        Task<long> IOpenIddictTokenStore<IOpenIdToken>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        Task<long> IOpenIddictTokenStore<IOpenIdToken>.CountAsync<TResult>(Func<IQueryable<IOpenIdToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.CreateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => CreateAsync((OpenIdToken) token, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.DeleteAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdToken) token, cancellationToken);

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
            => (await FindByApplicationIdAsync(identifier, cancellationToken)).CastArray<IOpenIdToken>();

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
            => (await FindByAuthorizationIdAsync(identifier, cancellationToken)).CastArray<IOpenIdToken>();

        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByReferenceIdAsync(identifier, cancellationToken);

        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.FindBySubjectAsync(string subject, CancellationToken cancellationToken)
            => (await FindBySubjectAsync(subject, cancellationToken)).CastArray<IOpenIdToken>();

        Task<TResult> IOpenIddictTokenStore<IOpenIdToken>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetApplicationIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetApplicationIdAsync((OpenIdToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetAuthorizationIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetAuthorizationIdAsync((OpenIdToken) token, cancellationToken);

        Task<DateTimeOffset?> IOpenIddictTokenStore<IOpenIdToken>.GetCreationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetCreationDateAsync((OpenIdToken) token, cancellationToken);

        Task<DateTimeOffset?> IOpenIddictTokenStore<IOpenIdToken>.GetExpirationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetExpirationDateAsync((OpenIdToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetIdAsync((OpenIdToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetPayloadAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetPayloadAsync((OpenIdToken) token, cancellationToken);

        Task<JObject> IOpenIddictTokenStore<IOpenIdToken>.GetPropertiesAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetPropertiesAsync((OpenIdToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetReferenceIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetReferenceIdAsync((OpenIdToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetStatusAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetStatusAsync((OpenIdToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetSubjectAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetSubjectAsync((OpenIdToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetTokenTypeAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetTokenTypeAsync((OpenIdToken) token, cancellationToken);

        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdToken>();

        Task<ImmutableArray<TResult>> IOpenIddictTokenStore<IOpenIdToken>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListInvalidAsync(count, offset, cancellationToken)).CastArray<IOpenIdToken>();

        Task IOpenIddictTokenStore<IOpenIdToken>.SetApplicationIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetApplicationIdAsync((OpenIdToken) token, identifier, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetAuthorizationIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetAuthorizationIdAsync((OpenIdToken) token, identifier, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetCreationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetCreationDateAsync((OpenIdToken) token, date, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetExpirationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetExpirationDateAsync((OpenIdToken) token, date, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetPayloadAsync(IOpenIdToken token, string payload, CancellationToken cancellationToken)
            => SetPayloadAsync((OpenIdToken) token, payload, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetPropertiesAsync(IOpenIdToken token, JObject properties, CancellationToken cancellationToken)
            => SetPropertiesAsync((OpenIdToken) token, properties, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetReferenceIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetReferenceIdAsync((OpenIdToken) token, identifier, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetStatusAsync(IOpenIdToken token, string status, CancellationToken cancellationToken)
            => SetStatusAsync((OpenIdToken) token, status, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetSubjectAsync(IOpenIdToken token, string subject, CancellationToken cancellationToken)
            => SetSubjectAsync((OpenIdToken) token, subject, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetTokenTypeAsync(IOpenIdToken token, string type, CancellationToken cancellationToken)
            => SetTokenTypeAsync((OpenIdToken) token, type, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.UpdateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdToken) token, cancellationToken);

        // ---------------------------------------------------
        // Methods defined by the IOpenIdTokenStore interface:
        // ---------------------------------------------------

        async Task<IOpenIdToken> IOpenIdTokenStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<string> IOpenIdTokenStore.GetPhysicalIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((OpenIdToken) token, cancellationToken);
    }
}
