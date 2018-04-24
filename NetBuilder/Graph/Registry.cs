using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IronPython;

namespace NetBuilder {
	[DataContract]
	public struct ConnectionData {
		[DataMember]
		public string NodeLabel, SocketVariableName;
	}
	[DataContract]
	public struct NodeData {
		[DataMember]
		public string StructureName;
		[DataMember]
		public Dictionary<string, ConnectionData> InputConnections;
	}
	[DataContract]
	public struct GraphData {
		[DataMember]
		public Dictionary<string, NodeData> Nodes;
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

		public IEnumerable<NodeStructure> NodeStructures {
			get {
				return nodeStructures.Values;
			}
		}
	}
}
