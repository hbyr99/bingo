using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Bingo
{
    class Program
    {
        private static RelationshipGraph rg;

        // Read RelationshipGraph whose filename is passed in as a parameter.
        // Build a RelationshipGraph in RelationshipGraph rg
        private static void ReadRelationshipGraph(string filename)
        {
            rg = new RelationshipGraph();                           // create a new RelationshipGraph object

            string name = "";                                       // name of person currently being read
            int numPeople = 0;
            string[] values;
            Console.Write("Reading file " + filename + "\n");
            try
            {
                string input = System.IO.File.ReadAllText(filename);// read file
                input = input.Replace("\r", ";");                   // get rid of nasty carriage returns 
                input = input.Replace("\n", ";");                   // get rid of nasty new lines
                string[] inputItems = Regex.Split(input, @";\s*");  // parse out the relationships (separated by ;)
                foreach (string item in inputItems) 
		{
                    if (item.Length > 2)                            // don't bother with empty relationships
                    {
                        values = Regex.Split(item, @"\s*:\s*");     // parse out relationship:name
                        if (values[0] == "name")                    // name:[personname] indicates start of new person
                        {
                            name = values[1];                       // remember name for future relationships
                            rg.AddNode(name);                       // create the node
                            numPeople++;
                        }
                        else
                        {               
                            rg.AddEdge(name, values[1], values[0]); // add relationship (name1, name2, relationship)

                            // handle symmetric relationships -- add the other way
                            if (values[0] == "hasSpouse" || values[0] == "hasFriend")
                                rg.AddEdge(values[1], name, values[0]);

                            // for parent relationships add child as well
                            else if (values[0] == "hasParent")
                                rg.AddEdge(values[1], name, "hasChild");
                            else if (values[0] == "hasChild")
                                rg.AddEdge(values[1], name, "hasParent");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Unable to read file {0}: {1}\n", filename, e.ToString());
            }
            Console.WriteLine(numPeople + " people read");
        }

        // Show the relationships a person is involved in
        private static void ShowPerson(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
                Console.Write(n.ToString());
            else
                Console.WriteLine("{0} not found", name);
        }

        // Show a person's friends
        private static void ShowFriends(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
            {
                Console.Write("{0}'s friends: ",name);
                List<GraphEdge> friendEdges = n.GetEdges("hasFriend");
                foreach (GraphEdge e in friendEdges) {
                    Console.Write("{0} ",e.To());
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);     
        }

        // Show orphans
        private static void ShowOrphans()
        {
            int _;

            // Check all nodes in rg
            foreach (GraphNode n in rg.nodes)
            {
                List<GraphEdge> nEdges = n.GetEdges();
                int isOrphan = 0;


            // Check all edges for hasParent label
                foreach (GraphEdge i in nEdges)
                {
                    if (i.Label == "hasParent") { isOrphan++; break; }
                }
                if (isOrphan == 0 && !int.TryParse(n.Name, out _))
                {
                    Console.Write("{0} is an orphan.\n", n.Name);
                }
            }
        }

        // Show a person's friends
        private static void ShowSiblings(string name)
        {
            GraphNode n = rg.GetNode(name);
            List<string> siblings = new List<string>();

            if (n != null)
            {
                Console.Write("{0}'s siblings: ", name);
                List<GraphEdge> parentEdges = n.GetEdges("hasParent");
                
                foreach (GraphEdge e in parentEdges)
                {
                    string parentName = e.To();
                    GraphNode parentNode = rg.GetNode(parentName);
                    List<GraphEdge> childrenEdges = parentNode.GetEdges("hasChild");
                    
                    foreach (GraphEdge i in childrenEdges)
                    {
                        if (!siblings.Contains(i.To()) && i.To() != name) { siblings.Add(i.To()); }
                    }
                }

                foreach (string sibName in siblings) { Console.Write("{0} ", sibName); }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);
        }

        // Show a person's descendants
        private static void ShowDescendants(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n == null)
            {
                Console.WriteLine("{0} not found", name);
                return;
            }

            List<GraphEdge> childEdges = n.GetEdges("hasChild");
            Dictionary<string, string> output = FindChildren(new Dictionary<string, string>(), childEdges, 0);
            if (output == null)
            {
                Console.WriteLine("No descendants found");
            }
            else
            {
                foreach (KeyValuePair<string, string> desc in output)
                {
                    Console.WriteLine("{0} is a {1}", desc.Key, desc.Value);
                }
            }
        }

        // Recursive function to find descendants
        private static Dictionary<string, string> FindChildren(Dictionary<string, string> dict, List<GraphEdge> lstEdges, int count)
        {
            if (!lstEdges.Any()) return null;

            string suffix = "child";
            if (count > 1) suffix = "grand" + suffix;
            if (count > 2 )
            {
                int i = count;
                while (i > 2 )
                {
                    suffix = "great " + suffix;
                    i--;
                }
            }

            foreach( GraphEdge edges in lstEdges)
            {
                dict.TryAdd(edges.To(), suffix);
                GraphNode temp = rg.GetNode(edges.To());
                if (temp != null)
                {
                    FindChildren(dict, temp.GetEdges("hasChild"), count++);
                }
            }
            return dict;
        }

        // Bingo function
        private static void Bingo(string name1, string name2)
        {
            List<GraphNode> path = rg.ShortestPath(name1, name2);
            if (path != null)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    foreach (GraphEdge e in path[i - 1].GetEdges())
                    {
                        if (rg.GetNode(e.To()) == path[i])
                        {
                            string e_String = "";
                            if (e.Label == "hasParent") e_String = "parent";
                            if (e.Label == "hasChild") e_String = "child";
                            if (e.Label == "hasSpouse") e_String = "spouse";
                            Console.WriteLine(path[i].Name + " is a " + e_String + " of " + path[i-1].Name);
                        }
                    }
                }
            }
            else
                Console.WriteLine(name1 + " has no relation to " + name2);
        }


        // accept, parse, and execute user commands
        private static void CommandLoop()
        {
            string command = "";
            string[] commandWords;
            Console.Write("Welcome to Harry's Dutch Bingo Parlor!\n");

            while (command != "exit")
            {
                Console.Write("\nEnter a command: ");
                command = Console.ReadLine();
                commandWords = Regex.Split(command, @"\s+");        // split input into array of words
                command = commandWords[0];

                if (command == "exit")
                    ;                                               // do nothing

                // read a relationship graph from a file
                else if (command == "read" && commandWords.Length > 1)
                    ReadRelationshipGraph(commandWords[1]);

                // show information for one person
                else if (command == "show" && commandWords.Length > 1)
                    ShowPerson(commandWords[1]);

                else if (command == "friends" && commandWords.Length > 1)
                    ShowFriends(commandWords[1]);

                else if (command == "orphans")
                    ShowOrphans();

                else if (command == "siblings" && commandWords.Length > 1)
                    ShowSiblings(commandWords[1]);

                else if (command == "descendants" && commandWords.Length > 1)
                    ShowDescendants(commandWords[1]);

                else if (command == "bingo" && commandWords.Length > 2)
                    Bingo(commandWords[1], commandWords[2]);

                // dump command prints out the graph
                else if (command == "dump")
                    rg.Dump();

                // illegal command
                else
                    Console.Write("\nLegal commands: read [filename], dump, show [personname],\n  friends [personname], orphans, siblings [personname],\n descendants [personname] exit\n");
            }
        }

        static void Main(string[] args)
        {
            CommandLoop();
        }
    }
}
