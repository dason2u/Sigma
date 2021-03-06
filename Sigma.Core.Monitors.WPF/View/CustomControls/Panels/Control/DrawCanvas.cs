﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace Sigma.Core.Monitors.WPF.View.CustomControls.Panels.Control
{
	public class DrawCanvas : RectangleCanvas, IDisposable
	{
		public delegate void InputChangedEventHandler(DrawCanvas canvas);

		public event InputChangedEventHandler InputChangedEvent;

		/// <summary>
		/// The colour that is used as drawing colour
		/// </summary>
		public Brush DrawColour
		{
			get { return (Brush)GetValue(DrawColourProperty); }
			set { SetValue(DrawColourProperty, value); }
		}

		/// <summary>
		/// The dependency property for <see ref="DrawColour"/>.
		/// </summary>
		public static readonly DependencyProperty DrawColourProperty =
			DependencyProperty.Register("DrawColour", typeof(Brush), typeof(DrawCanvas), new PropertyMetadata(Brushes.Black));

		/// <summary>
		///  Decide if a drawcolourchange affects currently drawn shapes.
		/// </summary>
		public bool UpdateColours
		{
			get { return (bool)GetValue(UpdateColoursProperty); }
			set { SetValue(UpdateColoursProperty, value); }
		}

		public static readonly DependencyProperty UpdateColoursProperty =
			DependencyProperty.Register("UpdateColours", typeof(bool), typeof(DrawCanvas), new PropertyMetadata(true));

		public bool SoftDrawing
		{
			get { return (bool)GetValue(SoftDrawingProperty); }
			set { SetValue(SoftDrawingProperty, value); }
		}

		public static readonly DependencyProperty SoftDrawingProperty =
			DependencyProperty.Register("SoftDrawing", typeof(bool), typeof(DrawCanvas), new PropertyMetadata(true));

		public double SoftFactor
		{
			get { return (double)GetValue(SoftFactorProperty); }
			set { SetValue(SoftFactorProperty, value); }
		}

		public static readonly DependencyProperty SoftFactorProperty =
			DependencyProperty.Register("SoftFactor", typeof(double), typeof(DrawCanvas), new PropertyMetadata(0.125));

		private Point _currentPoint;

		public DrawCanvas()
		{
			MouseDown += Canvas_MouseDown;
			MouseMove += Canvas_MouseMove;
			DependencyPropertyDescriptor.FromProperty(DrawColourProperty, typeof(DrawCanvas)).AddValueChanged(this, OnDrawColourChanged);
		}

		private void OnDrawColourChanged(object sender, EventArgs eventArgs)
		{
			UpdateDrawnRectangles(DrawColour);
		}

		#region RectangleBoundries

		private bool _drawing;

		private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				_currentPoint = e.GetPosition(this);
				_drawing = true;
			}
			else if (e.RightButton == MouseButtonState.Pressed)
			{
				_currentPoint = e.GetPosition(this);
				_drawing = false;
			}
		}

		private void Canvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
			{
				int opacity = _drawing ? 1 : 0;
				int factor = _drawing ? 1 : -1;
				Brush fill = _drawing ? DrawColour : null;

				LineSegment line = new LineSegment
				{
					X1 = _currentPoint.X,
					Y1 = _currentPoint.Y,
					X2 = e.GetPosition(this).X,
					Y2 = e.GetPosition(this).Y
				};

				for (int row = 0; row < Rectangles.GetLength(0); row++)
				{
					for (int column = 0; column < Rectangles.GetLength(1); column++)
					{
						Rectangle rect = Rectangles[row, column];
						double x = GetLeft(rect);
						double y = GetTop(rect);

						if (RectIntersectsLine(new Rect(x, y, rect.Width, rect.Height), line))
						{
							rect.Opacity = opacity;
							rect.Fill = fill;
							if (SoftDrawing)
							{
								UpdateNeighbours(GetNeighbours(Rectangles, row, column), factor, fill);
							}
						}
					}

					InputChangedEvent?.Invoke(this);
				}

				_currentPoint = e.GetPosition(this);
			}
		}

		private void UpdateNeighbours(Rectangle[] neighbours, int factor, Brush fill)
		{
			if (neighbours.Length != 8) throw new ArgumentException("May only contain 8 elements.", nameof(neighbours));

			for (int i = 0; i < neighbours.Length; i++)
			{
				if (neighbours[i] != null)
				{
					double currentSoft = SoftFactor;

					neighbours[i].Fill = fill;

					if (i % 2 == 0)
					{
						currentSoft *= currentSoft;
					}

					neighbours[i].Opacity = Math.Max(0, Math.Min(neighbours[i].Opacity + currentSoft * factor, 1));
				}
			}
		}

		private class LineSegment
		{
			public double X1 { get; set; }
			public double X2 { get; set; }
			public double Y1 { get; set; }
			public double Y2 { get; set; }
		}

		private static bool SegmentsIntersect(LineSegment a, LineSegment b)
		{
			double x1 = a.X1, x2 = a.X2, x3 = b.X1, x4 = b.X2;
			double y1 = a.Y1, y2 = a.Y2, y3 = b.Y1, y4 = b.Y2;

			double denominator = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

			if (denominator == 0)
			{
				return false;
			}

			double ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denominator;
			double ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denominator;

			return (ua > 0 && ua < 1 && ub > 0 && ub < 1);
		}

		private static bool RectIntersectsLine(Rect a, LineSegment b)
		{
			return SegmentsIntersect(b, new LineSegment { X1 = a.X, Y1 = a.Y, X2 = a.X, Y2 = a.Y + a.Height }) ||
					SegmentsIntersect(b, new LineSegment { X1 = a.X, Y1 = a.Y + a.Height, X2 = a.X + a.Width, Y2 = a.Y + a.Height }) ||
					SegmentsIntersect(b, new LineSegment { X1 = a.X + a.Width, Y1 = a.Y + a.Height, X2 = a.X + a.Width, Y2 = a.Y }) ||
					SegmentsIntersect(b, new LineSegment { X1 = a.X + a.Width, Y1 = a.Y, X2 = a.X, Y2 = a.Y }) ||
					RectContainsPoint(a, new Point(b.X1, b.Y1)) ||
					RectContainsPoint(a, new Point(b.X2, b.Y2));
		}

		private static bool RectContainsPoint(Rect a, Point b)
		{
			return b.X > a.X && b.X < a.X + a.Width && b.Y > a.Y && b.Y < a.Y + a.Height;
		}

		#endregion RectangleBoundries

		private void UpdateDrawnRectangles(Brush newColour)
		{
			if (UpdateColours)
			{
				if (Rectangles != null)
				{
					foreach (Rectangle rectangle in Rectangles)
					{
						if (rectangle.Fill != null) { rectangle.Fill = newColour; }
					}
				}
			}
		}

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose()
		{
			MouseDown -= Canvas_MouseDown;
			MouseMove -= Canvas_MouseMove;
		}
	}
}