// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

namespace Teo.AutoReference.System {
    /// <summary>
    /// Represents the result of a validation process with a boolean status and an error message if the
    /// validation failed.
    /// </summary>
    public readonly struct ValidationResult {
        private const string UnknownError = "An unknown error occurred";

        private const string UnknownWarning = "An unknown warning was encountered";

        /// <summary>
        /// Whether the value was a success without a warning or error.
        /// </summary>
        public bool IsOk { get; }

        /// <summary>
        /// The value indicating the validation completed with a warning.
        /// </summary>
        public bool IsWarning { get; }

        /// <summary>
        /// Whether the validation was a failure.
        /// </summary>
        public bool IsError => !IsOk && !IsWarning;

        /// <summary>
        /// Gets the string representing the message if the validation failed or completed with a warning.
        /// Empty for successful validation.
        /// </summary>
        public string Message { get; }

        private ValidationResult(bool isValid, string message) {
            if (isValid) {
                if (message != string.Empty) {
                    IsWarning = true;
                    IsOk = false;
                } else {
                    IsOk = true;
                    IsWarning = false;
                }
            } else {
                IsWarning = IsOk = false;
            }

            Message = message;
        }

        /// <summary>
        /// Represents a successful validation.
        /// </summary>
        public static ValidationResult Ok { get; } = new ValidationResult(true, string.Empty);

        /// <summary>
        /// Creates a new ValidationResult indicating a failed validation with a specific error message.
        /// </summary>
        public static ValidationResult Error(string message) {
            if (string.IsNullOrWhiteSpace(message)) {
                message = UnknownError;
            }

            return new ValidationResult(false, message);
        }

        public static ValidationResult Warning(string message) {
            if (string.IsNullOrWhiteSpace(message)) {
                message = UnknownWarning;
            }

            return new ValidationResult(true, message);
        }

        public static implicit operator bool(ValidationResult result) => !result.IsError;
    }
}
