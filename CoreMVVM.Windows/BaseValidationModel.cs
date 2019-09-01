﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CoreMVVM.Windows
{
    /// <summary>
    /// A base class for ViewModels that require validation. Use <see cref="BaseModel"/> instead if no validation is required.
    /// </summary>
    public abstract class BaseValidationModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields

        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when the value of a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when the collection of validation errors changes.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        #endregion Events

        #region Public properties

        /// <summary>
        /// Gets a value indicating if this validation model has any validation errors.
        /// </summary>
        public bool HasErrors => _errors.Any();

        #endregion Public properties

        #region Public methods

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve validation errors for; or null or <see cref="string.Empty"/>,
        /// to retrieve entity-level errors.</param>
        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == null)
                return _errors.SelectMany(e => e.Value);
            else
                return _errors.ContainsKey(propertyName) ? _errors[propertyName] : new List<string>();
        }

        #endregion Public methods

        #region Protected methods

        /// <summary>
        /// Compares a variable to a value. If different, the reference variable is assigned the value.
        /// Invokes <see cref="PropertyChanged"/> and validates the value if it was different.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="property">A reference to the field of the property.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="propertyName">The name of the property. Leave as null when calling from the property's setter.</param>
        /// <returns>True if the property changed.</returns>
        /// <remarks>Uses the default EqualityComparer of the type of the property.</remarks>
        protected virtual bool SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
                return false;

            property = value;
            RaisePropertyChanged(propertyName);
            ValidateProperty(propertyName, value);
            return true;
        }

        /// <summary>
        /// Validates a property.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <param name="value">The value of the property.</param>
        protected virtual void ValidateProperty<T>(string propertyName, T value)
        {
            List<ValidationResult> results = new List<ValidationResult>();

            ValidationContext context = new ValidationContext(this) { MemberName = propertyName };
            Validator.TryValidateProperty(value, context, results);

            if (results.Count > 0)
                _errors[propertyName] = results.Select(c => c.ErrorMessage).ToList();
            else
                _errors.Remove(propertyName);

            RaiseErrorsChanged(propertyName);
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event on a property.
        /// </summary>
        /// <param name="name">The name of the property to invoke the event on.</param>
        protected void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event on the calling member (should be a property).
        /// </summary>
        protected void RasieThisPropertyChanged([CallerMemberName] string propertyName = null) => RaisePropertyChanged(propertyName);

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event on all properties.
        /// </summary>
        protected void RaiseAllPropertiesChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        /// <summary>
        /// Invokes the <see cref="ErrorsChanged"/> event on a property.
        /// </summary>
        /// <param name="name">The name of the property to invoke the event on.</param>
        protected void RaiseErrorsChanged(string name) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(name));

        #endregion Protected methods
    }
}