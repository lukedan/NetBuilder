using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	public partial class NodeGraphEditor : UserControl {
		public NodeGraphEditor() {
			InitializeComponent();
		}


		public NodeGraphArea GraphArea {
			get {
				return graphArea;
			}
		}

		private void AddNodeEvent(object sender, RoutedEventArgs e) {
			if (structureList.SelectedItem != null) {
				graphArea.AddNode((string)structureList.SelectedItem);
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
