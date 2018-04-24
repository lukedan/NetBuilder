using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

using IronPython;
using IronPython.Hosting;
using System.Dynamic;

namespace NetBuilder {
	public class PythonRegistry {
		public Registry Registry { get; } = new Registry();

		public class RegisterProxy {
			private PythonRegistry registry;

			public RegisterProxy(PythonRegistry reg) {
				registry = reg;
			}

			public DataType Type(string name) {
				DataType type = new DataType { Name = name };
				registry.Registry.RegisterDataType(type);
				return type;
			}
			public NodeStructure Node(string name, dynamic inputs, dynamic outputs) {
				IEnumerable<Variable>
					inputVars = ((IEnumerable<object>)inputs).Cast<Variable>(),
					outputVars = ((IEnumerable<object>)outputs).Cast<Variable>();
				NodeStructure structure = new NodeStructure {
					Name = name,
					Inputs = inputVars.ToList(),
					Outputs = outputVars.ToList()
				};
				registry.Registry.RegisterNodeStructure(structure);
				return structure;
			}
		}


		private ScriptScope scope;
		private ScriptSource source;

		public PythonRegistry(string pythonPath) {
			ScriptEngine engine = Python.CreateEngine();
			ScriptRuntime runtime = engine.Runtime;
			scope = engine.CreateScope();
			source = engine.CreateScriptSourceFromFile(pythonPath);

			runtime.LoadAssembly(typeof(NodeStructure).Assembly);
			runtime.LoadAssembly(typeof(Variable).Assembly);

			scope.SetVariable("register", new RegisterProxy(this));
			source.Execute(scope);
		}
	}
}
