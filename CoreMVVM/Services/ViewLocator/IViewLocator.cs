﻿using CoreMVVM.Implementations;
using CoreMVVM.IOC;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CoreMVVM
{
    /// <summary>
    /// Provides methods for retrieving a view instance from a given view model.
    /// </summary>
    [FallbackImplementation(typeof(ViewLocator))]
    public interface IViewLocator
    {
        #region ResolveView

        /// <summary>
        /// Gets the view for the view model of a given type.
        /// </summary>
        /// <exception cref="ResolveException">the view model or view cannot be resolved by the IOC container.</exception>
        /// <exception cref="ViewNotFoundException">no view found of the given view model type.</exception>
        object ResolveView(Type viewModelType);

        /// <summary>
        /// Gets the view for the given view model.
        /// </summary>
        /// <exception cref="ResolveException">the view cannot be resolved by the IOC container.</exception>
        /// <exception cref="ViewNotFoundException">no view found of the given view model type.</exception>
        object ResolveView(object viewModel);

        /// <summary>
        /// Gets the view type for the given view model.
        /// </summary>
        /// <exception cref="ViewNotFoundException">no view found of the given view model type.</exception>
        Type ResolveViewType(Type viewModelType);

        #endregion ResolveView

        #region TryResolveView

        /// <summary>
        /// Gets the view for the view model of a given type.
        /// </summary>
        bool TryResolveView(Type viewModelType, [NotNullWhen(true)] out object? view);

        /// <summary>
        /// Gets the view for the given view model.
        /// </summary>
        bool TryResolveView(object viewModel, [NotNullWhen(true)] out object? view);

        /// <summary>
        /// Gets the view type for the given view model.
        /// </summary>
        bool TryResolveViewType(Type viewModelType, [NotNullWhen(true)] out Type? viewType);

        #endregion TryResolveView

        /// <summary>
        /// Adds an action that gets performed on the resolved view before it's returned.
        /// </summary>
        /// <param name="action">The action to perform. The first argument is the view model, the second is the view.</param>
        void AddOnResolve(Action<object, object> action);

        /// <summary>
        /// Adds a view provider to the view locator.
        /// </summary>
        /// <param name="type">The type of the provider.</param>
        /// <exception cref="ResolveException">the view provider cannot be resolved by the IOC container.</exception>
        /// <exception cref="ArgumentException">type does not implement <see cref="IViewProvider"/>.</exception>
        /// <remarks>
        /// View providers are used to locate views belonging to a given view model.
        /// </remarks>
        void AddViewProvider(Type type);

        /// <summary>
        /// Adds a view provider to the view locator.
        /// </summary>
        /// <param name="viewProvider">The provider.</param>
        /// <remarks>
        /// View providers are used to locate views belonging to a given view model.
        /// </remarks>
        void AddViewProvider(IViewProvider viewProvider);
    }
}