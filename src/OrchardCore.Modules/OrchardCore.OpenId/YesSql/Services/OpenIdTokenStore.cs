using System;
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
        public virtual Task<long> CountAsync<TResult>(Func<IQueryable<IOpenIdToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <param name="token">The token to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result returns the token.
        /// </returns>
        public virtual async Task<IOpenIdToken> CreateAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(token);
            await _session.CommitAsync();

            return token;
        }

        /// <summary>
        /// Removes a token.
        /// </summary>
        /// <param name="token">The token to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>A <see cref="Task"/> that can be used to monitor the asynchronous operation.</returns>
        public virtual Task DeleteAsync(IOpenIdToken token, CancellationToken cancellationToken)
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
        public virtual async Task<ImmutableArray<IOpenIdToken>> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ImmutableArray.CreateRange<IOpenIdToken>(
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
        public virtual async Task<ImmutableArray<IOpenIdToken>> FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ImmutableArray.CreateRange<IOpenIdToken>(
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
        public virtual async Task<IOpenIdToken> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<OpenIdToken, OpenIdTokenIndex>(index => index.ReferenceId == identifier).FirstOrDefaultAsync();
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
        public virtual async Task<IOpenIdToken> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _session.GetAsync<OpenIdToken>(int.Parse(identifier, CultureInfo.InvariantCulture));
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
        public virtual async Task<ImmutableArray<IOpenIdToken>> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ImmutableArray.CreateRange<IOpenIdToken>(await _session.Query<OpenIdToken, OpenIdTokenIndex>(index => index.Subject == subject).ListAsync());
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
            Func<IQueryable<IOpenIdToken>, TState, IQueryable<TResult>> query,
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
        public virtual Task<string> GetApplicationIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).ApplicationId?.ToString(CultureInfo.InvariantCulture));
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
        public virtual Task<string> GetAuthorizationIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).AuthorizationId?.ToString(CultureInfo.InvariantCulture));
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
        public virtual Task<DateTimeOffset?> GetCreationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).CreationDate);
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
        public virtual Task<DateTimeOffset?> GetExpirationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).ExpirationDate);
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
        public virtual Task<string> GetIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).TokenId);
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
        public virtual Task<string> GetPayloadAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).Payload);
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
        public virtual Task<string> GetReferenceIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).ReferenceId);
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
        public virtual Task<string> GetStatusAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).Status);
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
        public virtual Task<string> GetSubjectAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).Subject);
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
        public virtual Task<string> GetTokenTypeAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(((OpenIdToken) token).Type);
        }

        /// <summary>
        /// Instantiates a new token.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the instantiated token, that can be persisted in the database.
        /// </returns>
        public virtual Task<IOpenIdToken> InstantiateAsync(CancellationToken cancellationToken)
            => Task.FromResult<IOpenIdToken>(new OpenIdToken { TokenId = Guid.NewGuid().ToString("n") } );

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
        public virtual async Task<ImmutableArray<IOpenIdToken>> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
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

            return ImmutableArray.CreateRange<IOpenIdToken>(await query.ListAsync());
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
            Func<IQueryable<IOpenIdToken>, TState, IQueryable<TResult>> query,
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
        public virtual async Task<ImmutableArray<IOpenIdToken>> ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
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

            return ImmutableArray.CreateRange<IOpenIdToken>(await query.ListAsync());
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
        public virtual Task SetApplicationIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(identifier))
            {
                ((OpenIdToken) token).ApplicationId = null;
            }
            else
            {
                ((OpenIdToken) token).ApplicationId = identifier;
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
        public virtual Task SetAuthorizationIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(identifier))
            {
                ((OpenIdToken) token).AuthorizationId = null;
            }
            else
            {
                ((OpenIdToken) token).AuthorizationId = identifier;
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
        public virtual Task SetCreationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            ((OpenIdToken) token).CreationDate = date?.UtcDateTime;

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
        public virtual Task SetExpirationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            ((OpenIdToken) token).ExpirationDate = date?.UtcDateTime;

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
        public virtual Task SetPayloadAsync(IOpenIdToken token, string payload, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            ((OpenIdToken) token).Payload = payload;

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
        public virtual Task SetReferenceIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            ((OpenIdToken) token).ReferenceId = identifier;

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
        public virtual Task SetStatusAsync(IOpenIdToken token, string status, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(status));
            }

            ((OpenIdToken) token).Status = status;

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
        public virtual Task SetSubjectAsync(IOpenIdToken token, string subject, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            ((OpenIdToken) token).Subject = subject;

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
        public virtual Task SetTokenTypeAsync(IOpenIdToken token, string type, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The token type cannot be null or empty.", nameof(type));
            }

            ((OpenIdToken) token).Type = type;

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
        public virtual Task UpdateAsync(IOpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(token);

            return _session.CommitAsync();
        }
    }
}
