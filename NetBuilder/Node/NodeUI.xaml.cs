using System;
using System.Collections.Generic;
using System.Globalization;
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
	class ConstantUIElementVisibilityConverter : DependencyObject, IValueConverter {
		public ConstantType ExpectedType {
			get { return (ConstantType)GetValue(ExpectedTypeProperty); }
			set { SetValue(ExpectedTypeProperty, value); }
		}
		public static readonly DependencyProperty ExpectedTypeProperty = DependencyProperty.Register(
			"ExpectedType", typeof(ConstantType), typeof(ConstantUIElementVisibilityConverter)
		);

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is ConstantType type) {
				if (targetType == typeof(bool)) {
					return type == ExpectedType;
				}
				return type == ExpectedType ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

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

		public string ConstantValue {
			get { return (string)GetValue(ConstantValueProperty); }
			set { SetValue(ConstantValueProperty, value); }
		}
		public static readonly DependencyProperty ConstantValueProperty = DependencyProperty.Register(
			"ConstantValue", typeof(string), typeof(NodeUI)
		);

		public NodeStructure Structure {
			get { return (NodeStructure)GetValue(StructureProperty); }
			set { SetValue(StructureProperty, value); }
		}
		public static readonly DependencyProperty StructureProperty = DependencyProperty.Register(
			"Structure", typeof(NodeStructure), typeof(NodeUI), new PropertyMetadata((sender, e) => {
				if (sender is NodeUI node) {
					node.OnStructureChangedEvent();
				}
			})
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


		public event EventHandler RequestDelete;

		private void RequestDeleteEvent(object sender, RoutedEventArgs e) {
			RequestDelete?.Invoke(this, EventArgs.Empty);
		}


		private void OnStructureChangedEvent() {
			enumSelector.Items.Clear();
			if (Structure.ConstantType == ConstantType.Enum) {
				foreach (string item in Structure.ConstantParams) {
					enumSelector.Items.Add(item);
				}
			}
		}
	}
}
