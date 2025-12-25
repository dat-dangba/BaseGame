using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.Internals.Compatibility;

namespace Teo.AutoReference.Internals {
    internal static class TypeResolver {
        /// Contains known values, prepopulated with common types to C# aliases
        private static readonly Dictionary<Type, string> NameCache = new Dictionary<Type, string>() {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(void), "void" }
        };

        private static string GetArrayName(Type type) {
            using var results = TempList<string>.Get();
            // We do this in a loop instead of recursion to maintain the proper order of nested arrays.
            while (type!.IsArray) {
                var rank = type.GetArrayRank();
                results.Add(rank == 1 ? "[]" : $"[{new string(',', rank - 1)}]");
                type = type.GetElementType();
            }

            return $"{ResolveCSharpName(type, false)}{string.Concat(results)}";
        }

        private static string GetPointerName(Type type) {
            type = type.GetElementType();
            return $"{ResolveCSharpName(type, false)}*";
        }

        private static string GetGenericName(Type type) {
            var genericType = type.GetGenericTypeDefinition();

            if (genericType == Types.Nullable) {
                // Special handling for nullable types, but only when the generic parameters are known.
                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType != null) {
                    return $"{ResolveCSharpName(Nullable.GetUnderlyingType(type), false)}?";
                }
            }

            var typeName = type.Name;

            // Generic type names are in the format of TypeName`GenericArgumentCount
            var genericNameParts = typeName.Split('`');

            typeName = genericNameParts[0];

            if (genericNameParts.Length < 2 || !int.TryParse(genericNameParts[1], out var genericParamCount)) {
                genericParamCount = 0;
            }

            if (genericParamCount == 0) {
                // Can happen when this type does not explicitly take generic arguments but its declaring type is.
                return typeName;
            }

            var allParams = type.GetGenericArguments();
            var genericParams = allParams.Slice(allParams.Length - genericParamCount, genericParamCount);
            var formattedGenericParams = string.Join(", ", genericParams.Select(GetGenericParameterName));

            if (IsValueTuple(type) && genericParams.Any(g => !g.IsGenericParameter)) {
                // Special handling for value tuples, but only when the generic parameters are known.
                return $"({formattedGenericParams})";
            }

            return $"{typeName}<{formattedGenericParams}>";
        }

        /// <summary>
        /// Parses a parameter and adds covariance or contravariance when applicable.
        /// </summary>
        private static string GetGenericParameterName(Type type) {
            var name = ResolveCSharpName(type, false);
            if (!type.IsGenericParameter) {
                return name;
            }

            var modifiers = type.GenericParameterAttributes;

            if ((modifiers & GenericParameterAttributes.Contravariant) != 0) {
                return $"in {name}";
            }

            if ((modifiers & GenericParameterAttributes.Covariant) != 0) {
                return $"out {name}";
            }

            return name;
        }

        private static bool IsValueTuple(Type type) {
#if UNITY_2021_2_OR_NEWER
            return type.IsValueType && Types.AnyTuple.IsAssignableFrom(type);
#else
            return type.IsValueType;
#endif
        }

        /// <summary>
        /// Resolves and returns the C# name for the specified <see cref="Type"/>.
        /// </summary>
        public static string ResolveCSharpName(Type type, bool includeNamespace) {
            if (type == null) {
                return string.Empty;
            }

            if (NameCache.TryGetValue(type, out var value)) {
                return value;
            }

            var declaringType = GetActualDeclaringType(type);
            var declaringPrefix = declaringType == null ? null : ResolveCSharpName(declaringType, false);

            string name;
            if (type.IsArray) {
                name = GetArrayName(type);
            } else if (type.IsPointer) {
                name = GetPointerName(type);
            } else if (type.IsGenericType) {
                name = GetGenericName(type);
            } else {
                name = type.Name;
            }

            if (declaringPrefix != null) {
                name = $"{declaringPrefix}.{name}";
            }

            NameCache[type] = name;

            if (!includeNamespace) {
                return name;
            }

            var spacename = type.Namespace;
            return !string.IsNullOrWhiteSpace(spacename) ? $"{spacename}.{name}" : name;
        }

        /// <summary>
        /// Gets the declaring type of the specified type but also converts it to generic whenever applicable, and
        /// discards it when the specified type is a generic parameter.
        /// </summary>
        private static Type GetActualDeclaringType(Type type) {
            var declaringType = type.DeclaringType;

            if (declaringType == null || type.IsGenericParameter) {
                return null;
            }

            if (!declaringType.IsGenericType) {
                return declaringType;
            }

            // We now get a generic version of declaringType. We do this by extrapolating the concrete types of the
            // declaring type from the concrete types of the current type.

            // These do not contain concrete types, only generic:
            var baseTypeArgsCount = declaringType.GetGenericArguments().Length;

            // But these do, and they contain both the declaring type's and this type's generic parameters:
            var typeArgs = type.GetGenericArguments();

            if (baseTypeArgsCount > 0) {
                typeArgs = typeArgs.Slice(0, baseTypeArgsCount);
            }

            return declaringType.MakeGenericType(typeArgs);
        }
    }
}
