using System;
using JetBrains.Annotations;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public class MvvmViewControllerMediator : IMvvmViewControllerMediator
    {
        #region Fields

        private readonly UIViewController _viewController;

        #endregion

        #region Constructors

        public MvvmViewControllerMediator([NotNull] UIViewController viewController)
        {
            Should.NotBeNull(viewController, "viewController");
            _viewController = viewController;
            var viewModel = ViewManager.GetDataContext(viewController) as IViewModel;
            if (viewModel == null || !viewModel.Settings.Metadata.Contains(ViewModelConstants.StateNotNeeded))
                viewController.InititalizeRestorationIdentifier();
        }

        #endregion

        #region Properties

        public UIViewController ViewController
        {
            get { return _viewController; }
        }

        #endregion

        #region Implementation of IMvvmViewControllerMediator

        public virtual void ViewWillAppear(Action<bool> baseViewWillAppear, bool animated)
        {
            baseViewWillAppear(animated);
            if (_viewController.View != null)
                ParentObserver.Raise(_viewController.View, true);
            UINavigationItem navigationItem = _viewController.NavigationItem;
            if (navigationItem != null)
            {
                SetParent(navigationItem);
                SetParent(navigationItem.LeftBarButtonItem);
                SetParent(navigationItem.LeftBarButtonItems);
                SetParent(navigationItem.RightBarButtonItem);
                SetParent(navigationItem.RightBarButtonItems);
            }
            SetParent(_viewController.EditButtonItem);
            SetParent(_viewController.ToolbarItems);
            var dialogViewController = _viewController as DialogViewController;
            if (dialogViewController != null)
                SetParent(dialogViewController.Root);
            var viewControllers = ViewController.ChildViewControllers;
            foreach (var controller in viewControllers)
                BindingExtensions.AttachedParentMember.Raise(controller, EventArgs.Empty);

            var tabBarController = ViewController as UITabBarController;
            if (tabBarController != null)
            {
                viewControllers = tabBarController.ViewControllers;
                if (viewControllers != null)
                {
                    foreach (var controller in viewControllers)
                    {
                        BindingExtensions.AttachedParentMember.Raise(controller, EventArgs.Empty);
                        controller.RestorationIdentifier = string.Empty;
                    }
                }
            }
            Raise(ViewWillAppearHandler, animated);
        }

        public virtual void ViewDidAppear(Action<bool> baseViewDidAppear, bool animated)
        {
            baseViewDidAppear(animated);
            Raise(ViewDidAppearHandler, animated);
        }

        public virtual void ViewDidDisappear(Action<bool> baseViewDidDisappear, bool animated)
        {
            baseViewDidDisappear(animated);
            Raise(ViewDidDisappearHandler, animated);
        }

        public virtual void ViewDidLoad(Action baseViewDidLoad)
        {
            baseViewDidLoad();
            Raise(ViewDidLoadHandler);
        }

        public virtual void ViewWillDisappear(Action<bool> baseViewWillDisappear, bool animated)
        {
            baseViewWillDisappear(animated);
            Raise(ViewWillDisappearHandler, animated);
        }

        public virtual void DecodeRestorableState(Action<NSCoder> baseDecodeRestorableState, NSCoder coder)
        {
            baseDecodeRestorableState(coder);
            PlatformExtensions.ApplicationStateManager.DecodeState(_viewController, coder);
            Raise(DecodeRestorableStateHandler, coder);
        }

        public virtual void EncodeRestorableState(Action<NSCoder> baseEncodeRestorableState, NSCoder coder)
        {
            baseEncodeRestorableState(coder);
            PlatformExtensions.ApplicationStateManager.EncodeState(_viewController, coder);
            Raise(EncodeRestorableStateHandler, coder);
        }

        public virtual void Dispose(Action<bool> baseDispose, bool disposing)
        {
            Raise(DisposeHandler);
            baseDispose(disposing);
        }

        public virtual event EventHandler ViewDidLoadHandler;

        public virtual event EventHandler<ValueEventArgs<NSCoder>> EncodeRestorableStateHandler;

        public virtual event EventHandler DisposeHandler;

        public virtual event EventHandler<ValueEventArgs<bool>> ViewWillAppearHandler;

        public virtual event EventHandler<ValueEventArgs<bool>> ViewDidAppearHandler;

        public virtual event EventHandler<ValueEventArgs<bool>> ViewDidDisappearHandler;

        public virtual event EventHandler<ValueEventArgs<bool>> ViewWillDisappearHandler;

        public virtual event EventHandler<ValueEventArgs<NSCoder>> DecodeRestorableStateHandler;

        #endregion

        #region Methods

        private void SetParent<T>(T[] items)
        {
            if (items == null)
                return;
            for (int index = 0; index < items.Length; index++)
                SetParent(items[index]);
        }

        private void SetParent(object item)
        {
            if (item != null)
                BindingExtensions.AttachedParentMember.SetValue(item, _viewController);
        }

        private void Raise(EventHandler handler)
        {
            if (handler != null)
                handler(_viewController, EventArgs.Empty);
        }

        private void Raise<T>(EventHandler<ValueEventArgs<T>> handler, T value)
        {
            if (handler != null)
                handler(_viewController, new ValueEventArgs<T>(value));
        }

        #endregion
    }
}