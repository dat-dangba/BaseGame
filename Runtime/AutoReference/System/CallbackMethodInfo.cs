// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Linq;
using System.Reflection;
using Teo.AutoReference.Internals;
using UnityEngine;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference.System {
    public readonly struct CallbackMethodInfo {
        public bool IsOnBehaviour { get; }

        public ValidationResult Result { get; }

        public MethodInfo MethodInfo { get; }

        private CallbackMethodInfo(string error) {
            Result = ValidationResult.Error(error);
            IsOnBehaviour = false;
            MethodInfo = null;
        }

        public object Invoke(in FieldContext context, params object[] args) {
            return Invoke(context.Behaviour, args);
        }

        public T Invoke<T>(in FieldContext context, params object[] args) {
            return Invoke<T>(context.Behaviour, args);
        }

        public object Invoke(MonoBehaviour behaviour, params object[] args) {
            var instance = !MethodInfo.IsStatic && IsOnBehaviour ? behaviour : null;
            return MethodInfo.Invoke(instance, args);
        }

        public T Invoke<T>(MonoBehaviour behaviour, params object[] args) {
            var instance = !MethodInfo.IsStatic && IsOnBehaviour ? behaviour : null;
            return (T)MethodInfo.Invoke(instance, args);
        }

        private CallbackMethodInfo(MethodInfo method, bool isOnBehaviour) {
            MethodInfo = method;
            IsOnBehaviour = isOnBehaviour;

            Result = ValidationResult.Ok;
        }

        /// <summary>
        /// Create a human-readable string of a method given its return type, name, and arguments.
        /// </summary>
        private static string FormatMethod(Type returnType, string name, Type[] typeArgs) {
            var args = string.Join(", ", typeArgs.Select(arg => arg.FormatCSharpName()));
            return $"{returnType.FormatCSharpName()} {name}({args})";
        }

        private static string FormatParameter(ParameterInfo arg) {
            return arg.HasDefaultValue
                ? $"{arg.ParameterType.FormatCSharpName()}={arg.DefaultValue}"
                : arg.ParameterType.FormatCSharpName();

        }

        /// <summary>
        /// Format a <see cref="MethodInfo"/> into a human-readable string of the method it represents.
        /// </summary>
        public static string FormatMethod(MethodInfo info) {
            var returnType = info.ReturnType.FormatCSharpName();
            var staticPrefix = info.IsStatic ? "static " : "";

            var args = info.GetParameters();
            var formattedArgs = args.Select(FormatParameter);

            return $"{staticPrefix}{returnType} {info.Name}({string.Join(", ", formattedArgs)})";
        }

        public static CallbackMethodInfo Create(
            in FieldContext context,
            string methodName,
            Type returnType,
            params Type[] args
        ) {
            return Create(context.DeclaringType, null, methodName, returnType, args);
        }

        public static CallbackMethodInfo Create(
            in FieldContext context,
            Type callerType,
            string methodName,
            Type returnType,
            params Type[] args
        ) {
            return Create(context.DeclaringType, callerType, methodName, returnType, args);
        }

        private static string FormatTypes(Type[] args) {
            return $"({string.Join(", ", args.Select(a => a.FormatCSharpName()))})";
        }

        public static ValidationResult ValidateSignature(MethodInfo method, Type returnType, params Type[] args) {
            if (method == null) {
                return ValidationResult.Error("Method not found");
            }

            var argsAreValid = method.GetParameters().Select(p => p.ParameterType).SequenceEqual(args);
            if (!argsAreValid) {
                var signature = FormatMethod(method);
                var error = args.Length == 0
                    ? $"Method '{signature}' is expected to have no parameters"
                    : $"Method '{signature}' has invalid parameters; expected {FormatTypes(args)}";

                return ValidationResult.Error(error);
            }

            var returnIsValid = method.ReturnType == returnType;
            if (!returnIsValid) {
                var signature = FormatMethod(method);
                var error = $"Method '{signature}' has invalid return type; expected {returnType.FormatCSharpName()}";
                return ValidationResult.Error(error);
            }

            return ValidationResult.Ok;
        }

        internal static CallbackMethodInfo Create(
            Type behaviourType,
            Type callerType,
            string methodName,
            Type returnType,
            params Type[] args
        ) {
            var caller = callerType ?? behaviourType;

            if (string.IsNullOrWhiteSpace(methodName)) {
                return new CallbackMethodInfo($"Method name required for class-level 'OnAfterSync' attribute");
            }

            var method = caller.GetMethod(methodName, Types.CallbackFlags, Type.DefaultBinder, args, null);

            string signature;
            if (method == null) {
                // There might be a method with the same name but different arguments.
                method = caller.GetMethod(methodName);

                if (method == null) {
                    // Method is still null, which means it doesn't exist at all.
                    signature = FormatMethod(returnType, methodName, args);
                    return new CallbackMethodInfo($"Method '{signature}' not found");
                }
            }

            var signatureValidation = ValidateSignature(method, returnType, args);
            if (!signatureValidation) {
                return new CallbackMethodInfo(signatureValidation.Message);
            }

            if (method.IsStatic) {
                return new CallbackMethodInfo(method, false);
            }

            if (caller == behaviourType) {
                return new CallbackMethodInfo(method, true);
            }

            signature = FormatMethod(method);
            var error = $"Method '{signature}' on the external caller ('{caller.FormatCSharpName()}') must be static";
            return new CallbackMethodInfo(error);
        }
    }
}
