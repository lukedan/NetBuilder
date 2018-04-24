using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetBuilder {
	public partial class NodeUI : UserControl {
		public NodeUI() {
			InitializeComponent();
		}


		public string Label {
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}
		public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
			"Label", typeof(string), typeof(NodeUI)
		);

		public NodeStructure Structure {
			get { return (NodeStructure)GetValue(StructureProperty); }
			set { SetValue(StructureProperty, value); }
		}
		public static readonly DependencyProperty StructureProperty = DependencyProperty.Register(
			"Structure", typeof(NodeStructure), typeof(NodeUI)
		);

		public List<DataSocket> InputSockets {
			get {
				return inputSockets.Sockets;
			}
		}
		public List<DataSocket> OutputSockets {
			get {
				return outputSockets.Sockets;
			}
		}
	}
}
