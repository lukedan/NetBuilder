using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	public partial class NodeGraphArea : UserControl {
		public const double ScaleRatio = 1.05;


		private TranslateTransform translate = new TranslateTransform();
		private ScaleTransform scale = new ScaleTransform();

		private DragHandler drag = new DragHandler();

		private Dictionary<DataSocket, List<DataLink>> forwardLinkMap =
			new Dictionary<DataSocket, List<DataLink>>();
		private Dictionary<string, int> countMapping = new Dictionary<string, int>();

		private VariableType dragTargetType;
		private DataSocket dragTempTarget;
		private List<DataLink> draggingLinks;


		public NodeGraphArea() {
			InitializeComponent();

			viewport.RenderTransform = new TransformGroup { Children = { scale, translate } };
		}


		public event EventHandler RegistryChanged;

		public Registry Registry {
			get { return (Registry)GetValue(RegistryProperty); }
			set { SetValue(RegistryProperty, value); }
		}
		public static readonly DependencyProperty RegistryProperty = DependencyProperty.Register(
			"Registry", typeof(Registry), typeof(NodeGraphArea), new PropertyMetadata((obj, e) => {
				if (obj is NodeGraphArea area) {
					area.Clear();
					area.RegistryChanged?.Invoke(area, EventArgs.Empty);
				}
			})
		);

		public Pen ConnectionStroke {
			get { return (Pen)GetValue(ConnectionStrokeProperty); }
			set { SetValue(ConnectionStrokeProperty, value); }
		}
		public static readonly DependencyProperty ConnectionStrokeProperty = DependencyProperty.Register(
			"ConnectionStroke", typeof(Pen), typeof(NodeGraphArea), new PropertyMetadata(new Pen(Brushes.Black, 2.0))
		);


		private void AddLink(DataSocket from, DataSocket to) {
			if (!forwardLinkMap.TryGetValue(from, out List<DataLink> list)) {
				forwardLinkMap.Add(from, list = new List<DataLink>());
			}
			list.Add(new DataLink(from, to));
			InvalidateVisual();
		}
		private void RemoveLink(DataSocket to) {
			List<DataLink> links = forwardLinkMap[to.InputSource];
			Debug.Assert(links.RemoveAll((obj) => {
				return obj.ToSocket == to;
			}) == 1);
			if (links.Count == 0) {
				forwardLinkMap.Remove(to.InputSource);
			}
			to.InputSource = null;
			InvalidateVisual();
		}

		public GraphData DumpGraph() {
			Dictionary<string, NodeData> nodeDict = new Dictionary<string, NodeData>();
			foreach (UIElement elem in nodeLayer.Children) {
				NodeUI node = (NodeUI)elem;
				NodeData nodeData = new NodeData {
					StructureName = node.Structure.Name,
					ConstantValue = node.ConstantValue,
					InputConnections = new Dictionary<string, ConnectionData>()
				};
				foreach (DataSocket socket in node.InputSockets) {
					if (socket.InputSource != null) {
						nodeData.InputConnections.Add(socket.LinkVariable.Name, new ConnectionData {
							NodeLabel = Misc.FindParent<NodeUI>(socket.InputSource).Label,
							SocketVariableName = socket.InputSource.LinkVariable.Name
						});
					}
				}
				try {
					nodeDict.Add(node.Label, nodeData);
				} catch (ArgumentException) {
					throw new InvalidOperationException("the graph contains nodes with the same label");
				}
			}
			return new GraphData { Nodes = nodeDict };
		}
		public List<NodeUI> SortDAG() {
			Dictionary<NodeUI, int> inDegrees = new Dictionary<NodeUI, int>();
			Dictionary<DataSocket, NodeUI> socketOwners = new Dictionary<DataSocket, NodeUI>();
			List<NodeUI> result = new List<NodeUI>();
			foreach (UIElement elem in nodeLayer.Children) {
				NodeUI node = (NodeUI)elem;
				int inDeg = 0;
				foreach (DataSocket socket in node.InputSockets) {
					if (socket.InputSource != null) {
						socketOwners.Add(socket, node);
						++inDeg;
					}
				}
				if (inDeg == 0) {
					result.Add(node);
				} else {
					inDegrees.Add(node, inDeg);
				}
			}
			for (int i = 0; i < result.Count; ++i) {
				NodeUI node = result[i];
				foreach (DataSocket socket in node.OutputSockets) {
					if (forwardLinkMap.TryGetValue(socket, out List<DataLink> links)) {
						foreach (DataLink link in links) {
							NodeUI toNode = socketOwners[link.ToSocket];
							if (--inDegrees[toNode] == 0) {
								result.Add(toNode);
								inDegrees.Remove(toNode);
							}
						}
					}
				}
			}
			if (inDegrees.Count > 0) {
				throw new InvalidOperationException("graph is not a DAG");
			}
			return result;
		}
		public void SetGraph(GraphData graph) {
			Clear();
			Registry reg = Registry;
			Dictionary<string, NodeUI> nodeReg = new Dictionary<string, NodeUI>();
			foreach (var pair in graph.Nodes) {
				NodeUI node = AddNode(reg.QueryNodeStructure(pair.Value.StructureName), pair.Key);
				node.ConstantValue = pair.Value.ConstantValue;
				nodeReg.Add(pair.Key, node);
			}
			foreach (var pair in graph.Nodes) {
				NodeUI nodeUI = nodeReg[pair.Key];
				foreach (var connection in pair.Value.InputConnections) {
					NodeUI fromNode = nodeReg[connection.Value.NodeLabel];
					DataSocket
						fromSocket = fromNode.OutputSockets.Find((socket) => {
							return socket.LinkVariable.Name == connection.Value.SocketVariableName;
						}),
						toSocket = nodeUI.InputSockets.Find((socket) => {
							return socket.LinkVariable.Name == connection.Key;
						});
					toSocket.InputSource = fromSocket;
					AddLink(fromSocket, toSocket);
				}
			}
		}
		public void Clear() {
			nodeLayer.Children.Clear();
			translate.X = translate.Y = 0.0;
			scale.ScaleX = scale.ScaleY = 1.0;
			drag.StopDrag();
			forwardLinkMap.Clear();
			countMapping.Clear();
			dragTempTarget = null;
			draggingLinks = null;
			InvalidateVisual();
		}

		public bool IsDraggingSocketLink {
			get {
				return draggingLinks != null;
			}
		}

		private void StartDragSocketLink(Point position, DataSocket socket) {
			if (socket.SocketType == VariableType.Input) {
				if (socket.InputSource != null) {
					dragTargetType = VariableType.Input;
					// the list of outgoing links that contains the target link
					List<DataLink> fullList = forwardLinkMap[socket.InputSource];
					// index of the target link
					int i = fullList.FindIndex((obj) => {
						return obj.ToSocket == socket;
					});
					// move to draggingLinks
					draggingLinks = new List<DataLink> { fullList[i] };
					fullList.RemoveAt(i);
					if (fullList.Count == 0) {
						forwardLinkMap.Remove(socket.InputSource);
					}
					dragTempTarget = socket;
					socket.InputSource = null;
				} else {
					dragTargetType = VariableType.Output;
					draggingLinks = new List<DataLink> {
						new DataLink(InvTransformPosition(position), socket)
					};
					dragTempTarget = null;
				}
			} else {
				if (forwardLinkMap.TryGetValue(socket, out List<DataLink> list)) {
					dragTargetType = VariableType.Output;
					draggingLinks = list;
					forwardLinkMap.Remove(socket);
					dragTempTarget = socket;
					foreach (DataLink link in list) {
						link.ToSocket.InputSource = null;
					}
				} else {
					dragTargetType = VariableType.Input;
					draggingLinks = new List<DataLink> {
						new DataLink(socket, InvTransformPosition(position))
					};
					dragTempTarget = null;
				}
			}
		}
		private void UpdateDragSocketLink(Point position) {
			DataSocket socket = Misc.HitTest<DataSocket>(this, position);
			if (socket != null && (
				socket.SocketType != dragTargetType ||
				(socket.SocketType == VariableType.Input && socket.InputSource != null)
			)) {
				socket = null;
			}
			if (socket != dragTempTarget) {
				if (dragTempTarget != null) {
					foreach (DataLink link in draggingLinks) {
						if (dragTargetType == VariableType.Input) {
							link.ToSocket = null;
						} else {
							link.FromSocket = null;
						}
					}
				}
				dragTempTarget = socket;
				if (dragTempTarget != null) {
					foreach (DataLink link in draggingLinks) {
						if (dragTargetType == VariableType.Input) {
							link.ToSocket = dragTempTarget;
						} else {
							link.FromSocket = dragTempTarget;
						}
					}
				}
			}
			if (dragTempTarget == null) {
				foreach (DataLink link in draggingLinks) {
					if (dragTargetType == VariableType.Input) {
						link.ToPosition = InvTransformPosition(position);
					} else {
						link.FromPosition = InvTransformPosition(position);
					}
				}
			}
		}
		private void StopDragSocketLink() {
			if (dragTempTarget != null) {
				// check data types
				List<DataLink> newLinks = new List<DataLink>();
				foreach (DataLink link in draggingLinks) {
					// TODO also check conversion
					if (link.FromSocket.LinkVariable.VariableType == link.ToSocket.LinkVariable.VariableType) {
						newLinks.Add(link);
					}
				}
				if (newLinks.Count > 0) {
					draggingLinks = newLinks;
					// establish links
					foreach (DataLink link in draggingLinks) {
						link.ToSocket.InputSource = link.FromSocket;
					}
					if (forwardLinkMap.TryGetValue(draggingLinks[0].FromSocket, out List<DataLink> original)) {
						original.AddRange(draggingLinks);
					} else {
						forwardLinkMap.Add(draggingLinks[0].FromSocket, draggingLinks);
					}
				}
			}
			draggingLinks = null;
			dragTempTarget = null;
		}


		private Point InvScaleMousePosition(Point p) {
			p.X /= scale.ScaleX;
			p.Y /= scale.ScaleY;
			return p;
		}
		private Point InvTransformPosition(Point p) {
			p.X -= translate.X;
			p.Y -= translate.Y;
			return InvScaleMousePosition(p);
		}

		protected override void OnMouseDown(MouseButtonEventArgs e) {
			base.OnMouseDown(e);

			if (e.ChangedButton == MouseButton.Left) {
				Point position = e.GetPosition(this);
				DataSocket socket = Misc.HitTest<DataSocket>(this, position);
				if (socket != null) {
					StartDragSocketLink(position, socket);
				} else {
					NodeUI node = Misc.HitTest<NodeUI>(this, position);
					if (node != null) {
						drag.StartDrag(
							(TranslateTransform)node.RenderTransform,
							InvScaleMousePosition(position)
						);
					} else {
						drag.StartDrag(translate, position);
					}
				}
				CaptureMouse();
				e.Handled = true;
			}
		}
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);

			if (drag.DraggingTransform == translate) {
				drag.OnMouseMove(e.GetPosition(this));
			} else if (drag.IsDragging) {
				drag.OnMouseMove(InvScaleMousePosition(e.GetPosition(this)));
				InvalidateVisual();
			} else if (IsDraggingSocketLink) {
				UpdateDragSocketLink(e.GetPosition(this));
				InvalidateVisual();
			}
		}
		protected override void OnLostMouseCapture(MouseEventArgs e) {
			base.OnLostMouseCapture(e);

			if (drag.IsDragging) {
				drag.StopDrag();
			} else if (IsDraggingSocketLink) {
				StopDragSocketLink();
				InvalidateVisual();
			}
		}
		protected override void OnMouseUp(MouseButtonEventArgs e) {
			base.OnMouseUp(e);

			if (e.ChangedButton == MouseButton.Left) {
				ReleaseMouseCapture();
			}
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e) {
			base.OnMouseWheel(e);

			double scaleDelta = Math.Pow(ScaleRatio, e.Delta / 120.0);
			Point pos = e.GetPosition(this);
			Vector diff = (scaleDelta - 1.0) * (pos - new Point(translate.X, translate.Y));
			scale.ScaleX *= scaleDelta;
			scale.ScaleY *= scaleDelta;
			translate.X -= diff.X;
			translate.Y -= diff.Y;
			if (drag.DraggingTransform == translate) {
				drag.DraggingOffset -= diff;
			}
		}


		private void UpdateLinkPositions(DataLink link) {
			if (link.FromSocket != null) {
				link.FromPosition = InvTransformPosition(link.FromSocket.TranslatePoint(
					new Point(link.FromSocket.ActualWidth, link.FromSocket.ActualHeight * 0.5), this
				));
			}
			if (link.ToSocket != null) {
				link.ToPosition = InvTransformPosition(link.ToSocket.TranslatePoint(
					new Point(0.0, link.ToSocket.ActualHeight * 0.5), this
				));
			}
		}
		private void UpdateAndDrawLink(DataLink link, StreamGeometryContext ctx) {
			UpdateLinkPositions(link);
			double midx = 0.5 * (link.ToPosition.X - link.FromPosition.X);
			// TODO magic numbers
			midx = Math.Max(Math.Min(50.0, 0.5 * Math.Abs(link.FromPosition.Y - link.ToPosition.Y)), midx);
			ctx.BeginFigure(link.FromPosition, false, false);
			ctx.BezierTo(
				new Point(link.FromPosition.X + midx, link.FromPosition.Y),
				new Point(link.ToPosition.X - midx, link.ToPosition.Y),
				link.ToPosition, true, false
			);
		}
		protected override void OnRender(DrawingContext dc) {
			base.OnRender(dc);

			StreamGeometry geometry = new StreamGeometry();
			using (StreamGeometryContext ctx = geometry.Open()) {
				foreach (var pair in forwardLinkMap) {
					foreach (DataLink link in pair.Value) {
						UpdateAndDrawLink(link, ctx);
					}
				}
				if (draggingLinks != null) {
					foreach (DataLink link in draggingLinks) {
						UpdateAndDrawLink(link, ctx);
					}
				}
			}
			geometry.Freeze();

			dc.PushTransform(viewport.RenderTransform);
			dc.DrawGeometry(null, ConnectionStroke, geometry);
			dc.Pop();
		}


		public NodeUI AddNode(string structureName, string name = null) {
			return AddNode(Registry.QueryNodeStructure(structureName), name);
		}
		public NodeUI AddNode(NodeStructure structure, string name = null) {
			Point position = InvTransformPosition(new Point(0.0, 0.0));
			TranslateTransform nodeTranslation = new TranslateTransform(position.X, position.Y);
			NodeUI node = new NodeUI {
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(),
				RenderTransform = nodeTranslation,
				Background = Brushes.Transparent,
				Structure = structure,
				Label = name ?? structure.Name
			};
			if (name != null) {
				node.Label = name;
			} else {
				HashSet<string> strs = new HashSet<string>();
				foreach (UIElement elem in nodeLayer.Children) {
					NodeUI nodeUI = (NodeUI)elem;
					strs.Add(nodeUI.Label);
				}
				string prefix = string.Format(
					"{0}{1}_", structure.Name[0].ToString().ToLower(), structure.Name.Substring(1)
				);
				if (!countMapping.TryGetValue(structure.Name, out int index)) {
					countMapping.Add(structure.Name, index);
				}
				for (; ; ++index) {
					string newStr = prefix + index.ToString();
					if (!strs.Contains(newStr)) {
						node.Label = newStr;
						countMapping[structure.Name] = index + 1;
						break;
					}
				}
			}
			node.SizeChanged += (sender, e) => {
				InvalidateVisual();
			};
			node.RequestDelete += (sender, e) => {
				foreach (DataSocket socket in node.InputSockets) {
					if (socket.InputSource != null) {
						RemoveLink(socket);
					}
				}
				foreach (DataSocket socket in node.OutputSockets) {
					if (forwardLinkMap.TryGetValue(socket, out List<DataLink> links)) {
						foreach (DataLink link in links) {
							link.ToSocket.InputSource = null;
						}
						forwardLinkMap.Remove(socket);
						InvalidateVisual();
					}
				}
				nodeLayer.Children.Remove(node);
			};
			nodeLayer.Children.Add(node);
			return node;
		}
	}
}
