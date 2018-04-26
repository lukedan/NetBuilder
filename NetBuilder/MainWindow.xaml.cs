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

			graphArea.Registry = new PythonRegistry("test.py").Registry;
		}

		private void SaveGraphEvent(object sender, RoutedEventArgs e) {
			GraphData data;
			try {
				data = graphArea.DumpGraph();
			} catch (Exception exp) {
				MessageBox.Show(exp.Message, "Failed");
				return;
			}
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GraphData));
			using (FileStream stream = File.Open("test.json", FileMode.Create, FileAccess.Write)) {
				serializer.WriteObject(stream, data);
			}
		}
		private void LoadGraphEvent(object sender, RoutedEventArgs e) {
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GraphData));
			using (FileStream stream = File.Open("test.json", FileMode.Open, FileAccess.Read)) {
				graphArea.SetGraph((GraphData)serializer.ReadObject(stream));
			}
		}
		private void AddNodeEvent(object sender, RoutedEventArgs e) {
			if (structureList.SelectedItem != null) {
				graphArea.AddNode((string)structureList.SelectedItem);
			}
		}
		private void GenCodeEvent(object sender, RoutedEventArgs e) {
			using (StreamWriter writer = new StreamWriter("result.py")) {
				graphArea.Registry.CodeGenerators["tensorflow-py"].GenerateCode(graphArea.SortDAG(), writer);
			}
		}
		private void RegistryChangedEvent(object sender, EventArgs e) {
			structureList.Items.Clear();
			if (graphArea.Registry != null) {
				foreach (NodeStructure structure in graphArea.Registry.NodeStructures) {
					structureList.Items.Add(structure.Name);
				}
			}
		}
	}
}
