//using System;
//using System.Collections.Generic;
//using System.Text;
//using Roy_T.AStar.Grids;
//using Roy_T.AStar.Paths;
//using Roy_T.AStar.Primitives;
//using System.Threading.Tasks;
//using Roy_T.AStar.Graphs;

//namespace MultiplayerPlugin
//{
//    public class PathRequest
//    {
//        public INode startNode;
//        public INode endNode;
//        public PathFinder pathFinder;
//        public NavGridAgent agent;
//        public List<INode> excludedNodes;
//        public PathRequest(INode startNode, INode endNode, PathFinder pathFinder, NavGridAgent agent, List<INode> excludedNodes = null)
//        {
//            this.startNode = startNode;
//            this.endNode = endNode;
//            this.pathFinder = pathFinder;
//            this.agent = agent;
//            this.excludedNodes = excludedNodes;
//        }
//    }
//    public class Pathfinding
//    {
//        private NavGrid navGrid;
//        private List<PathRequest> pathRequests;
//        public Pathfinding(NavGrid navGrid)
//        {
//            this.navGrid = navGrid;
//            pathRequests = new List<PathRequest>();
//        }
//        public void FindAllPathsParallel()
//        {
//            lock (pathRequests)
//            {
//                Console.WriteLine();
//                Console.WriteLine();
//                Console.WriteLine("Requests: " + pathRequests.Count);
//                Console.WriteLine();
//                Console.WriteLine();
//                if (pathRequests.Count > 0)
//                {
//                    if (pathRequests.Count == 1)
//                    {
//                        PathRequest request = pathRequests[0];
//                        Path path = request.pathFinder.FindPath(request.startNode, request.endNode, navGrid.traversalVelocity, request.excludedNodes);
//                        request.agent.SetPath(path);
//                    }
//                    else
//                    {
//                        Parallel.For(0, pathRequests.Count, (index) =>
//                        {
//                            PathRequest request = pathRequests[index];
//                            Path path = request.pathFinder.FindPath(request.startNode, request.endNode, navGrid.traversalVelocity, request.excludedNodes);
//                            request.agent.SetPath(path);
//                        });
//                    }

//                    pathRequests.Clear();
//                }
//            }
//        }
//        public void CreatePathRequest(INode startNode, INode endNode, PathFinder pathFinder, NavGridAgent agent, List<INode> excludedNodes = null)
//        {
//            if (navGrid.IsNodeAnObstacle(endNode))
//            {
//                Console.WriteLine("End posiiton is an obstacle. Returning");
//                return;
//            }
//            PathRequest pathRequest = new PathRequest(startNode, endNode, pathFinder, agent, excludedNodes);
//            pathRequests.Add(pathRequest);
//        }
//    }
//}
