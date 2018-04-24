using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetBuilder {
	class VariableNameConverter : IValueConverter {
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is Variable variable) {
				return string.Format("{0}: {1}", variable.Name, variable.VariableType.Name);
			}
			return "<?>";
		}
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ToolTipPlacementConverter : IValueConverter {
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is VariableType type && type == VariableType.Input) {
				return PlacementMode.Left;
			}
			return PlacementMode.Right;
		}
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public partial class DataSocket : UserControl {
		public DataSocket() {
			InitializeComponent();
		}


		public DataSocket InputSource {
			get { return (DataSocket)GetValue(InputSourceProperty); }
			set { SetValue(InputSourceProperty, value); }
		}
		public static readonly DependencyProperty InputSourceProperty = DependencyProperty.Register(
			"InputSource", typeof(DataSocket), typeof(DataSocket)
		);

		public Variable LinkVariable {
			get { return (Variable)GetValue(LinkVariableProperty); }
			set { SetValue(LinkVariableProperty, value); }
		}
		public static readonly DependencyProperty LinkVariableProperty = DependencyProperty.Register(
			"LinkVariable", typeof(Variable), typeof(DataSocket)
		);

		public VariableType SocketType {
			get { return (VariableType)GetValue(SocketTypeProperty); }
			set { SetValue(SocketTypeProperty, value); }
		}
		public static readonly DependencyProperty SocketTypeProperty = DependencyProperty.Register(
			"SocketType", typeof(VariableType), typeof(DataSocket)
		);
	}
}
