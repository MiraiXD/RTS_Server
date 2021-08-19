using System;
using System.Collections.Generic;
using System.Text;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
using System.Threading.Tasks;
using Roy_T.AStar.Graphs;

namespace MultiplayerPlugin
{
    public enum PathStatus
    {
        PathComplete, PathNotFound, PathPending
    }
    public class PathRequest
    {
        public INode startNode;
        public INode endNode;
        public NavGridAgent agent;
        public INode excludedNode;
        public PathRequest(INode startNode, INode endNode, NavGridAgent agent, INode excludedNode)
        {
            this.startNode = startNode;
            this.endNode = endNode;
            this.agent = agent;
            this.excludedNode = excludedNode;
        }
        public PathRequest(INode startNode, INode endNode, NavGridAgent agent)
        {
            this.startNode = startNode;
            this.endNode = endNode;
            this.agent = agent;
            this.excludedNode = null;
        }
    }
    public class Pathfinding
    {
        private NavGrid navGrid;
        private List<PathRequest> pathRequests;        
        private PathFinder[] pathFinders;
        public Pathfinding(NavGrid navGrid)
        {
            this.navGrid = navGrid;
            pathRequests = new List<PathRequest>();
            pathFinders = new PathFinder[10];
            for (int i = 0; i < pathFinders.Length; i++)
            {
                pathFinders[i] = new PathFinder();
            }                
        }        
        public void FindAllPathsParallel()
        {          
                if(pathRequests.Count > 0)
                {
                    if(pathRequests.Count == 1)
                    {                        
                        Path path = pathFinders[0].FindPath(pathRequests[0].startNode, pathRequests[0].endNode, Velocity.FromMetersPerSecond(1f));
                        pathRequests[0].agent.SetPath(path);
                    }
                    else
                    {
                        Parallel.For(0, pathRequests.Count, (index) => {
                            Path path = pathFinders[index].FindPath(pathRequests[index].startNode, pathRequests[index].endNode, Velocity.FromMetersPerSecond(1f));
                            pathRequests[index].agent.SetPath(path);
                        });
                    }

                    pathRequests.Clear();          
            }
        }
        public void CreatePathRequest(INode startNode, INode endNode, NavGridAgent agent, INode excludedNode)
        {
            if (navGrid.IsNodeAnObstacle(endNode))
            {
                Console.WriteLine("End posiiton is an obstacle. Returning");
                return;
            }            
            PathRequest pathRequest = new PathRequest(startNode, endNode, agent, excludedNode);
            pathRequests.Add(pathRequest);
        }
        public void CreatePathRequest(INode startNode, INode endNode, NavGridAgent agent)
        {
            if (navGrid.IsNodeAnObstacle(endNode))
            {
                Console.WriteLine("End posiiton is an obstacle. Returning");
                return;
            }
            PathRequest pathRequest = new PathRequest(startNode, endNode, agent);
            pathRequests.Add(pathRequest);
        }

    }
}
