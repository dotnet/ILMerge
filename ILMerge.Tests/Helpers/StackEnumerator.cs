using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ILMerging.Tests.Helpers
{
    public static class StackEnumerator
    {
        public static StackEnumerator<T> Create<T>(params T[] initial) => new StackEnumerator<T>(initial);
        public static StackEnumerator<T> Create<T>(IEnumerable<T> initial) => new StackEnumerator<T>(initial);
        public static StackEnumerator<T> Create<T>(IEnumerator<T> initial) => new StackEnumerator<T>(initial);
        public static StackEnumerator<TContext, T> Create<TContext, T>(TContext initialContext, params T[] initial) => new StackEnumerator<TContext, T>(initialContext, initial);
        public static StackEnumerator<TContext, T> Create<TContext, T>(TContext initialContext, IEnumerable<T> initial) => new StackEnumerator<TContext, T>(initialContext, initial);
        public static StackEnumerator<TContext, T> Create<TContext, T>(TContext initialContext, IEnumerator<T> initial) => new StackEnumerator<TContext, T>(initialContext, initial);
    }

    public sealed class StackEnumerator<T> : IDisposable
    {
        private readonly Stack<IEnumerator<T>> stack = new Stack<IEnumerator<T>>();
        private IEnumerator<T> current;

        public bool MoveNext()
        {
            while (!current.MoveNext())
            {
                current.Dispose();
                if (stack.Count == 0) return false;
                current = stack.Pop();
            }

            return true;
        }

        public T Current => current.Current;

        public void Recurse(IEnumerator<T> newCurrent)
        {
            if (newCurrent == null) return;
            stack.Push(current);
            current = newCurrent;
        }
        public void Recurse(IEnumerable<T> newCurrent)
        {
            if (newCurrent == null) return;
            Recurse(newCurrent.GetEnumerator());
        }
        public void Recurse(params T[] newCurrent)
        {
            Recurse((IEnumerable<T>)newCurrent);
        }

        public StackEnumerator(IEnumerator<T> initial)
        {
            current = initial ?? System.Linq.Enumerable.Empty<T>().GetEnumerator();
        }
        public StackEnumerator(IEnumerable<T> initial) : this(initial?.GetEnumerator())
        {
        }
        public StackEnumerator(params T[] initial) : this((IEnumerable<T>)initial)
        {
        }

        // Foreach support
        [EditorBrowsable(EditorBrowsableState.Never)]
        public StackEnumerator<T> GetEnumerator()
        {
            return this;
        }

        public void Dispose()
        {
            current.Dispose();
            foreach (var item in stack)
                item.Dispose();
            stack.Clear();
        }
    }

    public sealed class StackEnumerator<TContext, T> : IDisposable
    {
        public struct ContextCurrent
        {
            public TContext Context { get; }

            public T Current { get; }

            public ContextCurrent(TContext context, T current)
            {
                Context = context;
                Current = current;
            }
        }

        private readonly Stack<Tuple<TContext, IEnumerator<T>>> stack = new Stack<Tuple<TContext, IEnumerator<T>>>();
        private Tuple<TContext, IEnumerator<T>> current;

        public bool MoveNext()
        {
            while (!current.Item2.MoveNext())
            {
                current.Item2.Dispose();
                if (stack.Count == 0) return false;
                current = stack.Pop();
            }

            return true;
        }

        public ContextCurrent Current => new ContextCurrent(current.Item1, current.Item2.Current);

        public void Recurse(TContext newContext, IEnumerator<T> newCurrent)
        {
            if (newCurrent == null) return;
            stack.Push(current);
            current = Tuple.Create(newContext, newCurrent);
        }
        public void Recurse(TContext newContext, IEnumerable<T> newCurrent)
        {
            if (newCurrent == null) return;
            Recurse(newContext, newCurrent.GetEnumerator());
        }
        public void Recurse(TContext newContext, params T[] newCurrent)
        {
            Recurse(newContext, (IEnumerable<T>)newCurrent);
        }

        public StackEnumerator(TContext initialContext, IEnumerator<T> initial)
        {
            current = Tuple.Create(initialContext, initial ?? System.Linq.Enumerable.Empty<T>().GetEnumerator());
        }
        public StackEnumerator(TContext initialContext, IEnumerable<T> initial) : this(initialContext, initial?.GetEnumerator())
        {
        }
        public StackEnumerator(TContext initialContext, params T[] initial) : this(initialContext, (IEnumerable<T>)initial)
        {
        }

        // Foreach support
        [EditorBrowsable(EditorBrowsableState.Never)]
        public StackEnumerator<TContext, T> GetEnumerator()
        {
            return this;
        }

        public void Dispose()
        {
            current.Item2.Dispose();
            foreach (var item in stack)
                item.Item2.Dispose();
            stack.Clear();
        }
    }
}
