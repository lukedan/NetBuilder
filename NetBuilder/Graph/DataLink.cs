using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace NetBuilder {
	public class DataLink {
		public DataLink() {
		}
		public DataLink(DataSocket from, DataSocket to) {
			FromSocket = from;
			ToSocket = to;
		}
		public DataLink(DataSocket from, Point to) {
			FromSocket = from;
			ToPosition = to;
		}
		public DataLink(Point from, DataSocket to) {
			FromPosition = from;
			ToSocket = to;
		}

		public DataSocket FromSocket { get; set; }
		public DataSocket ToSocket { get; set; }

		public Point FromPosition { get; set; }
		public Point ToPosition { get; set; }
	}
}
