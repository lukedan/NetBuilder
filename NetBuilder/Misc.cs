using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NetBuilder {
	static class Misc {
		public static T FindParent<T>(DependencyObject obj) where T : DependencyObject {
			for (; obj != null; obj = VisualTreeHelper.GetParent(obj)) {
				if (obj is T result) {
					return result;
				}
			}
			return null;
		}
		public static T HitTest<T>(Visual reference, Point position) where T : Visual {
			DependencyObject result = null;
			VisualTreeHelper.HitTest(reference, null, (current) => {
				result = current.VisualHit;
				return HitTestResultBehavior.Stop;
			}, new PointHitTestParameters(position));
			return FindParent<T>(result);
		}
	}
}
