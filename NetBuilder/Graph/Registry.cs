using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IronPython;
using System.IO;

namespace NetBuilder {
	[DataContract]
	public struct ConnectionData {
		[DataMember]
		public string NodeLabel, SocketVariableName;
	}
	[DataContract]
	public struct NodeData {
		[DataMember]
		public string StructureName, ConstantValue;
		[DataMember]
		public Dictionary<string, ConnectionData> InputConnections;
	}
	[DataContract]
	public struct GraphData {
		[DataMember]
		public Dictionary<string, NodeData> Nodes;
	}

	public struct CodeGenerator {
		public string NullString;
		public Dictionary<NodeStructure, dynamic> Generators;

		public void GenerateCode(List<NodeUI> dag, StreamWriter writer) {
			HashSet<string> varNames = new HashSet<string>();
			Dictionary<DataSocket, string> varNameMapping = new Dictionary<DataSocket, string>();
			foreach (NodeUI node in dag) {
				List<string> variables = new List<string>();
				// add input names
				foreach (DataSocket socket in node.InputSockets) {
					if (socket.InputSource != null) {
						variables.Add(varNameMapping[socket.InputSource]);
					}
				}
				// allocate & add output names
				foreach (DataSocket socket in node.OutputSockets) {
					string
						originalName = string.Format("{0}_{1}", node.Label, socket.LinkVariable.Name),
						name = originalName;
					for (int i = 0; varNames.Contains(name); ++i) {
						name = string.Format("{0}_{1}", originalName, i);
					}
					varNames.Add(name);
					varNameMapping.Add(socket, name);
					variables.Add(name);
				}
				if (node.Structure.ConstantType != ConstantType.None) {
					variables.Add(node.ConstantValue);
				}
				writer.Write(Generators[node.Structure](variables));
			}
		}
	}

	public class Registry {
		private Dictionary<string, DataType> dataTypes = new Dictionary<string, DataType>();
		private Dictionary<string, NodeStructure> nodeStructures = new Dictionary<string, NodeStructure>();

		public void RegisterDataType(DataType type) {
			// TODO set color
			dataTypes.Add(type.Name, type);
		}
		public DataType QueryDataType(string name) {
			dataTypes.TryGetValue(name, out DataType type);
			return null;
		}

		public void RegisterNodeStructure(NodeStructure structure) {
			nodeStructures.Add(structure.Name, structure);
		}
		public NodeStructure QueryNodeStructure(string name) {
			nodeStructures.TryGetValue(name, out NodeStructure structure);
			return structure;
		}

		public Dictionary<string, CodeGenerator> CodeGenerators { get; } = new Dictionary<string, CodeGenerator>();

		public IEnumerable<NodeStructure> NodeStructures {
			get {
				return nodeStructures.Values;
			}
		}
	}
}
