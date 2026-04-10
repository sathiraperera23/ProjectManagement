using Moq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using TaskManagementApi.Application.Interfaces;
using System.Reflection;
using System.Collections;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementApi.Tests
{
    public static class MoqExtensions
    {
        public static void SetupAsyncQueryable<TEntity>(this Mock<IRepository<TEntity>> mock, IQueryable<TEntity> data) where TEntity : class
        {
            var asyncQueryable = new TestAsyncQueryable<TEntity>(data);
            mock.Setup(r => r.Query()).Returns(asyncQueryable);
        }
    }

    internal class TestAsyncQueryable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IOrderedQueryable<T>
    {
        public TestAsyncQueryable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncQueryable(Expression expression) : base(expression) { }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
        public T Current => _inner.Current;
        public ValueTask DisposeAsync() { _inner.Dispose(); return new ValueTask(); }
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
        public IQueryable CreateQuery(Expression expression) => new TestAsyncQueryable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncQueryable<TElement>(expression);
        public object? Execute(Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult);
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var expectedResultType = resultType.GetGenericArguments()[0];
                object? executionResult;
                try
                {
                    executionResult = _inner.Execute(expression);
                }
                catch (InvalidOperationException)
                {
                    // Fallback for some expressions that EnumerableQueryProvider might struggle with
                    // but usually it's fine for our mock data
                    throw;
                }

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(expectedResultType).Invoke(null, new[] { executionResult })!;
            }

            return _inner.Execute<TResult>(expression);
        }
    }
}
