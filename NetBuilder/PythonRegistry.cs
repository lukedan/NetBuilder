﻿using System;
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

			public Tuple<DataType, NodeStructure> Type(
				string name, ConstantType constantType, params dynamic[] constantParams
			) {
				DataType type = new DataType { Name = name };
				registry.Registry.RegisterDataType(type);
				NodeStructure structure = null;
				if (constantType != ConstantType.None) {
					structure = new NodeStructure {
						Name = name + "Constant",
						ConstantType = constantType,
						ConstantParams = constantParams.Cast<string>().ToList(),
						Inputs = new List<Variable>(),
						Outputs = new List<Variable> {
							new Variable(type, "value")
						}
					};
					registry.Registry.RegisterNodeStructure(structure);
				}
				return new Tuple<DataType, NodeStructure>(type, structure);
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
			public void Generator(string name, string nullString, IDictionary<object, dynamic> dict) {
				CodeGenerator gen = new CodeGenerator {
					NullString = nullString,
					Generators = new Dictionary<NodeStructure, dynamic>()
				};
				foreach (var pair in dict) {
					gen.Generators[(NodeStructure)pair.Key] = pair.Value;
				}
				registry.Registry.CodeGenerators.Add(name, gen);
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

			source.Execute(scope);

			scope.GetVariable("RegisterAll")(new RegisterProxy(this));
		}
	}
}
