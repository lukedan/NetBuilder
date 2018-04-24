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
	public class DataSocketEventArgs : EventArgs {
		public DataSocketEventArgs(DataSocket socket) {
			Socket = socket;
		}

		public DataSocket Socket { get; private set; }
	}

	public partial class DataSocketArray : UserControl {
		private Binding typeBinding;


		public DataSocketArray() {
			InitializeComponent();

			typeBinding = new Binding {
				Path = new PropertyPath(ArrayTypeProperty),
				Source = this
			};
		}


		public List<DataSocket> Sockets { get; private set; } = new List<DataSocket>();

		public List<Variable> SocketVariables {
			get { return (List<Variable>)GetValue(SocketVariablesProperty); }
			set { SetValue(SocketVariablesProperty, value); }
		}
		public static readonly DependencyProperty SocketVariablesProperty = DependencyProperty.Register(
			"SocketVariables", typeof(List<Variable>), typeof(DataSocketArray), new PropertyMetadata(
				(obj, args) => {
					if (obj is DataSocketArray array) {
						array.OnSocketVariablesChanged(args);
					}
				}
			)
		);
		private void OnSocketVariablesChanged(DependencyPropertyChangedEventArgs args) {
			// TODO try to preserve links?
			UIElementCollection children = mainGrid.Children;
			children.Clear();
			Sockets.Clear();
			if (args.NewValue is List<Variable> list) {
				foreach (Variable variable in list) {
					DataSocket socket = new DataSocket { LinkVariable = variable };
					socket.SetBinding(DataSocket.SocketTypeProperty, typeBinding);
					children.Add(socket);
					Sockets.Add(socket);
				}
			}
		}

		public VariableType ArrayType {
			get { return (VariableType)GetValue(ArrayTypeProperty); }
			set { SetValue(ArrayTypeProperty, value); }
		}
		public static readonly DependencyProperty ArrayTypeProperty = DependencyProperty.Register(
			"ArrayType", typeof(VariableType), typeof(DataSocketArray)
		);
	}
}
