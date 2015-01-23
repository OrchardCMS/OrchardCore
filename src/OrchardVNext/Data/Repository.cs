using Microsoft.Data.Entity;
using OrchardVNext.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OrchardVNext.Data {
    public class Repository<T> : IRepository<T> where T : class {
        public Repository(IDbContextLocator contentLocator) {
            DbSet = contentLocator.For(typeof(T)).Set<T>();
        }

        public virtual DbSet<T> DbSet { get; private set; }

        public virtual IQueryable<T> Table {
            get {
                return DbSet.AsQueryable();
            }
        }

        #region IRepository<T> Members

        void IRepository<T>.Create(T entity) {
            Create(entity);
        }

        void IRepository<T>.Update(T entity) {
            Update(entity);
        }

        void IRepository<T>.Delete(T entity) {
            Delete(entity);
        }

        void IRepository<T>.Copy(T source, T target) {
            Copy(source, target);
        }

        T IRepository<T>.Get(int id) {
            return Get(id);
        }

        T IRepository<T>.Get(Expression<Func<T, bool>> predicate) {
            return Get(predicate);
        }

        IQueryable<T> IRepository<T>.Table {
            get { return Table; }
        }

        int IRepository<T>.Count(Expression<Func<T, bool>> predicate) {
            return Count(predicate);
        }

        IEnumerable<T> IRepository<T>.Fetch(Expression<Func<T, bool>> predicate) {
            return Fetch(predicate).ToReadOnlyCollection();
        }

        IEnumerable<T> IRepository<T>.Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order) {
            return Fetch(predicate, order).ToReadOnlyCollection();
        }

        IEnumerable<T> IRepository<T>.Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip,
                                            int count) {
            return Fetch(predicate, order, skip, count).ToReadOnlyCollection();
        }

        #endregion

        public virtual T Get(int id) {
            throw new NotImplementedException();
            //return _dbSet.SingleOrDefault(x => x.Id == id);
        }

        public virtual T Get(Expression<Func<T, bool>> predicate) {
            return Fetch(predicate).SingleOrDefault();
        }

        public virtual void Create(T entity) {
            Logger.Debug("Create {0}", entity);
            DbSet.Add(entity);
        }

        public virtual void Update(T entity) {
            Logger.Debug("Update {0}", entity);
            DbSet.Update(entity);
        }

        public virtual void Delete(T entity) {
            Logger.Debug("Delete {0}", entity);
            DbSet.Remove(entity);
        }

        public virtual void Copy(T source, T target) {
            Logger.Debug("Copy {0} {1}", source, target);
        }

        public virtual int Count(Expression<Func<T, bool>> predicate) {
            return DbSet.Count(predicate);
        }

        public virtual IQueryable<T> Fetch(Expression<Func<T, bool>> predicate) {
            return Table.Where(predicate);
        }

        public virtual IQueryable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order) {
            var orderable = new Orderable<T>(Fetch(predicate));
            order(orderable);
            return orderable.Queryable;
        }

        public virtual IQueryable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip,
                                           int count) {
            return Fetch(predicate, order).Skip(skip).Take(count);
        }
    }
}