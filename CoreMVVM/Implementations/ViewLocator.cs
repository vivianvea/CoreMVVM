﻿using CoreMVVM.IOC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreMVVM.Implementations
{
    [Scope(ComponentScope.Singleton)]
    public sealed class ViewLocator : IViewLocator
    {
        private readonly List<IViewProvider> _viewProviders = new List<IViewProvider>();

        private readonly Dictionary<Type, Type> _viewCache = new Dictionary<Type, Type>();
        private readonly List<Action<object, object>> _onResolveActions = new List<Action<object, object>>();

        private readonly IContainer _container;

        public ViewLocator(IContainer container, DefaultViewProvider viewProvider)
        {
            _container = container;

            _viewProviders.Add(viewProvider);
        }

        #region IViewLocator

        public object GetView<TViewModel>() where TViewModel : class
        {
            LoggerHelper.Debug($"View for view model '{typeof(TViewModel)} requested.");

            Type viewType = GetViewType<TViewModel>();

            TViewModel viewModel = _container.Resolve<TViewModel>();
            return CreateView(viewType, viewModel);
        }

        public object GetView(object viewModel)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            LoggerHelper.Debug($"View for view model '{viewModel.GetType()} requested.");

            Type viewModelType = viewModel.GetType();
            Type viewType = GetViewType(viewModelType);

            return CreateView(viewType, viewModel);
        }

        public Type GetViewType<TViewModel>()
        {
            if (_viewCache.TryGetValue(typeof(TViewModel), out var viewType))
                return viewType;

            var result = LocateViewType((provider, context) => provider.FindView<TViewModel>(context));
            if (result is null)
            {
                LoggerHelper.Error($"Failed to find view for view model of type '{typeof(TViewModel)}'.");
                throw new InvalidOperationException($"No view found for view model of type '{typeof(TViewModel)}'.");
            }

            if (result.CacheView)
                _viewCache[typeof(TViewModel)] = result.ViewType;

            return result.ViewType;
        }

        public Type GetViewType(Type viewModelType)
        {
            if (_viewCache.TryGetValue(viewModelType, out var viewType))
                return viewType;

            var result = LocateViewType((provider, context) => provider.FindView(viewModelType, context));
            if (result is null)
            {
                LoggerHelper.Error($"Failed to find view for view model of type '{viewModelType}'.");
                throw new InvalidOperationException($"No view found for view model of type '{viewModelType}'.");
            }

            if (result.CacheView)
                _viewCache[viewModelType] = result.ViewType;

            return result.ViewType;
        }

        public void AddViewProvider<TViewProvider>() where TViewProvider : class, IViewProvider
        {
            var viewProvider = _container.Resolve<TViewProvider>();
            _viewProviders.Add(viewProvider);
        }

        public void AddViewProvider(IViewProvider viewProvider)
        {
            if (viewProvider is null)
                throw new ArgumentNullException(nameof(viewProvider));

            _viewProviders.Add(viewProvider);
        }

        #endregion IViewLocator

        #region Methods

        /// <summary>
        /// Adds an action that gets performed on the resolved view before it's returned.
        /// </summary>
        /// <param name="action">The action to perform. The first argument is the view model, the second is the view.</param>
        /// <remarks>
        /// Actions are executed in the order they are added.
        /// </remarks>
        public void AddOnResolve(Action<object, object> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            _onResolveActions.Add(action);
        }

        #endregion Methods

        #region Helpers

        private ViewProviderContext LocateViewType(Func<IViewProvider, ViewProviderContext, bool> locator)
        {
            ViewProviderContext context = new ViewProviderContext();
            foreach (var viewProvider in Enumerable.Reverse(_viewProviders))
            {
                // Keep going until a provider finds the view.
                if (!locator(viewProvider, context))
                    continue;

                if (context.ViewType is null)
                    throw new InvalidOperationException($"View provider '{viewProvider.GetType()}' returned true, but no view was provided.");

                return context;
            }

            return null;
        }

        private object CreateView(Type viewType, object viewModel)
        {
            object view = _container.Resolve(viewType);
            LoggerHelper.Debug($"Resolved to instance of '{view.GetType()}'.");

            _onResolveActions.ForEach(a => a(viewModel, view));

            return view;
        }

        #endregion Helpers
    }
}