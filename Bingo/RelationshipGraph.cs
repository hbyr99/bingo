using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bingo
{
    /// <summary>
    /// Represents a directed labeled graph with a string name at each node
    /// and a string Label for each edge.
    /// </summary>
    class RelationshipGraph
    {
        /*
         *  This data structure contains a list of nodes (each of which has
         *  an adjacency list) and a dictionary (hash table) for efficiently 
         *  finding nodes by name
         */
        public List<GraphNode> nodes { get; private set; }
        private Dictionary<String, GraphNode> nodeDict;

        // constructor builds empty relationship graph
        public RelationshipGraph()
        {
            nodes = new List<GraphNode>();
            nodeDict = new Dictionary<String,GraphNode>();
        }

        // AddNode creates and adds a new node if there isn't already one by that name
        public void AddNode(string name)
        {
            if (!nodeDict.ContainsKey(name))
            {
                GraphNode n = new GraphNode(name);
                nodes.Add(n);
                nodeDict.Add(name, n);
            }
        }

        // AddEdge adds the edge, creating endpoint nodes if necessary.
        // Edge is added to adjacency list of from edges.
        public void AddEdge(string name1, string name2, string relationship) 
        {
            AddNode(name1);                     // create the node if it doesn't already exist
            GraphNode n1 = nodeDict[name1];     // now fetch a reference to the node
            AddNode(name2);
            GraphNode n2 = nodeDict[name2];
            GraphEdge e = new GraphEdge(n1, n2, relationship);
            n1.AddIncidentEdge(e);
        }

        // Get a node by name using dictionary
        public GraphNode GetNode(string name)
        {
            if (nodeDict.ContainsKey(name))
                return nodeDict[name];
            else
                return null;
        }

        // Return a text representation of graph
        public void Dump()
        {
            foreach (GraphNode n in nodes)
            {
                Console.Write(n.ToString());
            }
        }

		public List<GraphNode> ShortestPath(String F, String T)
		{
			// BFS search for shortest path
			List<GraphNode> path = new List<GraphNode>();
			List<List<GraphNode>> levels = new List<List<GraphNode>>();
			List<GraphNode> level = new List<GraphNode>();
			Dictionary<String, Boolean> seen = new Dictionary<String, Boolean>();
			int levelCount = 0;
			GraphNode From = GetNode(F);
			GraphNode To = GetNode(T);

			if (From == null || To == null || From == To)
				return null;

			level.Add(From);
			levels.Add(level);
			while (true)
			{
				level = new List<GraphNode>();
				foreach (GraphNode n in levels[levels.Count - 1])
				{
					foreach (GraphEdge e in n.GetEdges())
					{
						if (e.To() == T) // End node -- stuck
							goto EndBuild;
						if (!seen.ContainsKey(e.To()))// Avoid equal keys
						{
							level.Add(GetNode(e.To()));
							seen.Add(e.To(), true);
						}
					}
				}
				levels.Add(level);
			}
		EndBuild:
			// Reverse path
			path.Add(To);
			levelCount = levels.Count - 1;
			try
			{
				while (path[path.Count - 1] != From && levelCount >= 0)
				{ 
					foreach (GraphEdge e in path[path.Count - 1].GetEdges())
					{
						if (levels[levelCount].Contains(GetNode(e.To())))
						{
							path.Add(GetNode(e.To()));
							goto NextInWhile;
						}
					}

					foreach (GraphNode n in levels[levelCount])
					{
						foreach (GraphEdge e in n.GetEdges())
						{
							if (GetNode(e.To()) == path[path.Count - 1])
							{
								path.Add(n);
								goto NextInWhile;
							}
						}
					}
				NextInWhile:
					levelCount--;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			path.Reverse();
			if (path.Count == 1)
				path = null;
			return path;
		}
	}
}
