//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using OrchardVNext.ContentManagement;
//using OrchardVNext.ContentManagement.Records;

//namespace OrchardVNext.Data.EF {
//    public class EFContentIndexProvider : IContentIndexProvider {
//        private readonly IEnumerable<IContentQueryExpressionHandler> _contentQueryExpressionHandlers;
//        private readonly DataContext _dataContext;

//        public EFContentIndexProvider(IEnumerable<IContentQueryExpressionHandler> contentQueryExpressionHandlers,
//            DataContext dataContext)
//        {
//            _contentQueryExpressionHandlers = contentQueryExpressionHandlers;
//            _dataContext = dataContext;
//        }

//        public void Index<T>(T content) where T : DocumentRecord {
//            // GetAsync Lambda and store this content.
//            var data = content.Infoset.Data;

//            foreach (var handler in _contentQueryExpressionHandlers) {
//                Expression<Func<T, bool>> filter = handler.OnCreating<T>();

//                var canReduce = filter.CanReduce;
                
//                // TODO - Sanitize in to usefulname
//                var indexName = filter.Body.ToString();
//                Logger.Debug("Adding {0} to index {1}", typeof(T).Name, indexName);


//                //_contentDocumentStore.Query<T>(filter);
//            }
//        }

//        public void DeIndex<T>(T content) where T : DocumentRecord {
//            //throw new NotImplementedException();
//        }
//        public IEnumerable<T> Query<T>() where T : DocumentRecord {
//            return _dataContext.Set<T>().ToList();
//        }

//        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> filter) where T : DocumentRecord {
//            return _dataContext.Set<T>().Where(filter).ToList();
//        }
//    }
//}