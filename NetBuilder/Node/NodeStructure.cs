using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NetBuilder {
	public class DataType : DependencyObject {
		public string Name {
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
			"Name", typeof(string), typeof(DataType)
		);

		public Color MarkColor {
			get { return (Color)GetValue(MarkColorProperty); }
			set { SetValue(MarkColorProperty, value); }
		}
		public static readonly DependencyProperty MarkColorProperty = DependencyProperty.Register(
			"MarkColor", typeof(Color), typeof(DataType), new PropertyMetadata(Colors.Black)
		);
	}

	public class Variable : DependencyObject {
		public Variable() {
		}
		public Variable(DataType type, string name) {
			VariableType = type;
			Name = name;
		}

		public DataType VariableType {
			get { return (DataType)GetValue(VariableTypeProperty); }
			set { SetValue(VariableTypeProperty, value); }
		}
		public static readonly DependencyProperty VariableTypeProperty = DependencyProperty.Register(
			"VariableType", typeof(DataType), typeof(Variable)
		);

		public string Name {
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
			"Name", typeof(string), typeof(Variable), new PropertyMetadata("variable")
		);
	}

	public class NodeStructure : DependencyObject {
		public string Name {
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
			"Name", typeof(string), typeof(NodeStructure), new PropertyMetadata("Node")
		);

		public List<Variable> Inputs {
			get { return (List<Variable>)GetValue(InputsProperty); }
			set { SetValue(InputsProperty, value); }
		}
		public static readonly DependencyProperty InputsProperty = DependencyProperty.Register(
			"Inputs", typeof(List<Variable>), typeof(NodeStructure)
		);

		public List<Variable> Outputs {
			get { return (List<Variable>)GetValue(OutputsProperty); }
			set { SetValue(OutputsProperty, value); }
		}
		public static readonly DependencyProperty OutputsProperty = DependencyProperty.Register(
			"Outputs", typeof(List<Variable>), typeof(NodeStructure)
		);
	}

	public enum VariableType {
		Input,
		Output
	}
}
