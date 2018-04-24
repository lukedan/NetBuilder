using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
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
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			editor.GraphArea.Registry = new PythonRegistry("test.py").Registry;
		}

		private void SaveGraphEvent(object sender, RoutedEventArgs e) {
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GraphData));
			using (FileStream stream = File.Open("test.json", FileMode.Create, FileAccess.Write)) {
				serializer.WriteObject(stream, editor.GraphArea.DumpGraph());
			}
		}
		private void LoadGraphEvent(object sender, RoutedEventArgs e) {
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GraphData));
			using (FileStream stream = File.Open("test.json", FileMode.Open, FileAccess.Read)) {
				editor.GraphArea.SetGraph((GraphData)serializer.ReadObject(stream));
			}
		}
	}
}
