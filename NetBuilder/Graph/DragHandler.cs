using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NetBuilder {
	class DragHandler {
		public TranslateTransform DraggingTransform { get; private set; } = null;
		public Vector DraggingOffset { get; set; }
		public bool IsDragging {
			get {
				return DraggingTransform != null;
			}
		}

		public void StartDrag(TranslateTransform transform, Point mousePosition) {
			DraggingTransform = transform;
			DraggingOffset = new Point(DraggingTransform.X, DraggingTransform.Y) - mousePosition;
		}
		public TranslateTransform StartDragNew(Point mousePosition) {
			DraggingTransform = new TranslateTransform();
			DraggingOffset = new Vector();
			return DraggingTransform;
		}

		public void OnMouseMove(Point newPosition) {
			if (DraggingTransform != null) {
				Point newTarget = newPosition + DraggingOffset;
				DraggingTransform.X = newTarget.X;
				DraggingTransform.Y = newTarget.Y;
			}
		}

		public void StopDrag() {
			DraggingTransform = null;
		}
	}
}
