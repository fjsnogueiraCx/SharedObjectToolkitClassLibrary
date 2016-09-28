using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator;

namespace SharedObjectToolkitClassLibrary.VirtualObject {

    public static class New<T> where T : new() {
        public static readonly Func<T, IntPtr> Instance = Expression.Lambda<Func<T, IntPtr>>
                                                  (
                                                   Expression.New(typeof(T))
                                                  ).Compile();
    }

    public static unsafe class VirtualObjectFactory {
        private static ConstructorInfo[] _constructors = new ConstructorInfo[ushort.MaxValue];
        private static Func<IntPtr, object>[] _delegates = new Func<IntPtr, object>[ushort.MaxValue];

        public static void RecordType(Type t, FactoryTypeIdentifier tid) {
            _constructors[tid.TypeCode] = t.GetConstructor(new[] { typeof(IntPtr) });
            if (_constructors[tid.TypeCode] == null)
                throw new Exception("Byte* constructor not found.");
            _delegates[tid.TypeCode] = GetConstructorDelegate<IntPtr>(t);
        }

        public static object Rebirth(byte* data) {
            var ctor = _constructors[MemoryAllocator.TypeIdentifierOf(data).TypeCode];
            if (ctor != null)
                return _delegates[MemoryAllocator.TypeIdentifierOf(data).TypeCode].Invoke((IntPtr)data);
            throw new Exception("Constructor not registered.");
        }

        public static Func<TArg1, object> GetConstructorDelegate<TArg1>(Type type) {
            var argumentTypes = new[] { typeof(TArg1) };
            Type[] constructorArgumentTypes = argumentTypes.Where(t => t != typeof(int)).ToArray();
            var constructor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                CallingConventions.HasThis,
                constructorArgumentTypes,
                new ParameterModifier[0]);
            var lamdaParameterExpressions = new[] {
                                                      Expression.Parameter(typeof (TArg1), "param1")
                                                  };
            var constructorParameterExpressions = lamdaParameterExpressions
                .Take(constructorArgumentTypes.Length)
                .ToArray();
            var constructorCallExpression = Expression.New(constructor, constructorParameterExpressions);
            var constructorCallingLambda = Expression
                .Lambda<Func<TArg1, object>>(constructorCallExpression, lamdaParameterExpressions)
                .Compile();
            return constructorCallingLambda;
        }
    }
}