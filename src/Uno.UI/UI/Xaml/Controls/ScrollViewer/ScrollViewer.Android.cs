﻿using Android.Views;
using Android.Widget;
using Uno.Extensions;
using Uno.Logging;
using Uno.UI.DataBinding;
using Windows.UI.Xaml.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Uno.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using System.Drawing;
using Uno.UI;
using Microsoft.Extensions.Logging;
using static Uno.UI.MathEx;

namespace Windows.UI.Xaml.Controls
{
	public partial class ScrollViewer : ContentControl
	{
		partial void InitializePartial()
		{
			base.SetClip();
		}

		internal static int GetMeasureValue(int value, ScrollBarVisibility scrollBarVisibility)
		{
			switch (scrollBarVisibility)
			{
				case ScrollBarVisibility.Auto:
				case ScrollBarVisibility.Hidden:
				case ScrollBarVisibility.Visible:
					return ViewHelper.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

				default:
				case ScrollBarVisibility.Disabled:
					return value;
			}
		}


		partial void ChangeViewScroll(double? horizontalOffset, double? verticalOffset, bool disableAnimation)
		{
			var physicalHorizontalOffset = ViewHelper.LogicalToPhysicalPixels(horizontalOffset ?? HorizontalOffset);
			var physicalVerticalOffset = ViewHelper.LogicalToPhysicalPixels(verticalOffset ?? VerticalOffset);

			const int maxScroll = int.MaxValue / 2;
			const int minScroll = -maxScroll;

			// Clamp values (again) to avoid overflow in UnoTwoDScrollView.java
			physicalHorizontalOffset = Clamp(physicalHorizontalOffset, minScroll, maxScroll);
			physicalVerticalOffset = Clamp(physicalVerticalOffset, minScroll, maxScroll);

			if (disableAnimation)
			{
				_sv.ScrollTo(physicalHorizontalOffset, physicalVerticalOffset);
			}
			else
			{
				_sv.SmoothScrollTo(physicalHorizontalOffset, physicalVerticalOffset);
			}
		}

		partial void ChangeViewZoom(float zoomFactor, bool disableAnimation)
		{
			if (!disableAnimation && this.Log().IsEnabled(LogLevel.Warning))
			{
				this.Log().Warn("ChangeView: Animated zoom not yet implemented for Android.");
			}
			if (_sv != null)
			{
				_sv.ZoomScale = zoomFactor;
			}
		}
		
		private void UpdateZoomedContentAlignment()
		{
			if (ZoomFactor != 1 && Content is IFrameworkElement fe && Content is View view)
			{
				float pivotX, pivotY;

				var scaledWidth = ZoomFactor * view.Width;
				var viewPortWidth = (this as View).Width;

				if (viewPortWidth <= scaledWidth)
				{
					pivotX = 0;
				}
				else
				{
					switch (fe.HorizontalAlignment)
					{
						case HorizontalAlignment.Left:
							pivotX = 0;
							break;
						case HorizontalAlignment.Right:
							pivotX = (viewPortWidth - scaledWidth) / (1 - ZoomFactor);
							break;
						case HorizontalAlignment.Center:
						case HorizontalAlignment.Stretch:
							pivotX = 0.5f * (viewPortWidth - scaledWidth) / (1 - ZoomFactor);
							break;
						default:
							throw new InvalidOperationException();
					}
				}

				var scaledHeight = ZoomFactor * view.Height;
				var viewportHeight = (this as View).Height;

				if (viewportHeight < scaledHeight)
				{
					pivotY = 0;
				}
				else
				{
					switch (fe.VerticalAlignment)
					{
						case VerticalAlignment.Top:
							pivotY = 0;
							break;
						case VerticalAlignment.Bottom:
							pivotY = (viewportHeight - scaledHeight) / (1 - ZoomFactor);
							break;
						case VerticalAlignment.Center:
						case VerticalAlignment.Stretch:
							pivotY = 0.5f * (viewportHeight - scaledHeight) / (1 - ZoomFactor);
							break;
						default:
							throw new InvalidOperationException();
					}
				}

				view.PivotX = pivotX;
				view.PivotY = pivotY;
			}
		}

		partial void OnZoomModeChangedPartial(ZoomMode zoomMode)
		{
			if (_sv != null)
			{
				_sv.IsZoomEnabled = zoomMode == ZoomMode.Enabled;

				// Apply these in case _sv was not initialized when they were set
				_sv.MinimumZoomScale = MinZoomFactor;
				_sv.MaximumZoomScale = MaxZoomFactor;
			}
		}

		partial void OnBringIntoViewOnFocusChangeChangedPartial(bool newValue)
		{
			if (_sv != null)
			{
				_sv.BringIntoViewOnFocusChange = newValue;
			}
		}
	}
}
