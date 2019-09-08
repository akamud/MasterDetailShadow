using CoreGraphics;
using MasterDetailShadow.iOS;
using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using PointF = CoreGraphics.CGPoint;

[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(CustomMenuRenderer), UIUserInterfaceIdiom.Phone)]
namespace MasterDetailShadow.iOS
{
    public class CustomMenuRenderer : PhoneMasterDetailRenderer
    {
        bool _disposed;
        UIPanGestureRecognizer _panGesture;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ((MasterDetailPage)Element).PropertyChanged += HandlePropertyChanged;

            var model = (MasterDetailPage)Element;
            if (!model.IsGestureEnabled)
            {
                if (_panGesture != null)
                    View.RemoveGestureRecognizer(_panGesture);
                return;
            }

            if (_panGesture != null)
            {
                View.AddGestureRecognizer(_panGesture);
                return;
            }

            UITouchEventArgs shouldReceive = (g, t) => !(t.View is UISlider);
            var center = new PointF();
            _panGesture = new UIPanGestureRecognizer(g =>
            {
                var detailRenderer = Platform.GetRenderer(((MasterDetailPage)Element).Detail);
                var masterRenderer = Platform.GetRenderer(((MasterDetailPage)Element).Master);
                var translation = g.TranslationInView(View).X;
                var presented = ((MasterDetailPage)Element).IsPresented;
                double openProgress = 0;
                double openLimit = masterRenderer.ViewController.View.Frame.Width;

                if (presented)
                {
                    openProgress = 1 - (-translation / openLimit);
                }
                else
                {
                    openProgress = translation / openLimit;
                }

                openProgress = Math.Min(Math.Max(openProgress, 0.0), 1.0);
                switch (g.State)
                {
                    case UIGestureRecognizerState.Changed:
                        LayoutViews(View.Bounds, (nfloat)openProgress, detailRenderer.ViewController.View);
                        break;
                }
            })
            {
                ShouldReceiveTouch = shouldReceive,
                MaximumNumberOfTouches = 2
            };
            _panGesture.ShouldRecognizeSimultaneously = (gesture1, gesture2) => true;
            View.AddGestureRecognizer(_panGesture);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && !_disposed)
            {
                Element.PropertyChanged -= HandlePropertyChanged;
            }
        }

        void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Xamarin.Forms.MasterDetailPage.IsPresentedProperty.PropertyName)
            {
                var presented = ((MasterDetailPage)Element).IsPresented;
                var detailRenderer = Platform.GetRenderer(((MasterDetailPage)Element).Detail);

                if (presented)
                {
                    LayoutViews(View.Bounds, 1f, detailRenderer.ViewController.View);
                }
                else
                {
                    LayoutViews(View.Bounds, 0f, detailRenderer.ViewController.View);
                }
            }
        }

        private void LayoutViews(CGRect bounds, nfloat percent, UIView detail)
        {
            detail.Superview.BackgroundColor = Xamarin.Forms.Color.Black.ToUIColor();
            detail.Frame = bounds;
            var opacity = (nfloat)(0.5 + (0.5 * (1 - percent)));
            detail.Layer.Opacity = (float)opacity;
        }
    }
}
