using System;

namespace MonkeyKart.Common
{
    public readonly struct Result<V, E>
    {
        private readonly bool _success;
        public readonly V Value;
        public readonly E Error;

        private Result(V v, E e, bool success)
        {
            Value = v;
            Error = e;
            _success = success;
        }

        public bool IsOk => _success;

        public static Result<V, E> Ok(V v)
        {
            return new(v, default(E), true);
        }

        public static Result<V, E> Err(E e)
        {
            return new(default(V), e, false);
        }

        public static implicit operator Result<V, E>(V v) => new(v, default(E), true);
        public static implicit operator Result<V, E>(E e) => new(default(V), e, false);


        public U MapBoth<U>(Func<V, U> success, Func<E, U> failure)
        {
            if (_success) return success(Value);
            else return failure(Error);
        }
        
        public Result<V, E> OnSuccess(Action<V> action)
        {
            if(_success) action(Value);
            return this;
        }

        public Result<V, E> OnFailure(Action<E> action)
        {
            if (!_success) action(Error);
            return this;
        }

        public Result<U, E> Map<U>(Func<V, U> transform)
        {
            if (!_success) return new(default(U), Error, false);
            return transform(Value);
        }

        public Result<U, E> AndThen<U>(Func<V, Result<U, E>> transform)
        {
            if (_success) return transform(Value);
            return new(default(U), Error, false);
        }
    }
}