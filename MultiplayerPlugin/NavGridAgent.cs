//using System;
//using System.Collections.Generic;
//using System.Text;
//using Roy_T.AStar.Graphs;
//using Roy_T.AStar.Grids;
//using Roy_T.AStar.Paths;
//using Roy_T.AStar.Primitives;
//namespace MultiplayerPlugin
//{
//    public class NavGridAgent
//    {
//        public Path path;
//        public NavGrid navGrid;
//        public Pathfinding pathfinding;
//        public PathFinder pathFinder;

//        public INode currentNode { get; private set; }
//        public INode nextNode { get; private set; }
//        public INode destinationNode { get; private set; }

//        private int currentPathNodeIndex;

//        public Vector3 currentSteeringTarget { get; set; }
//        public Vector3 currentPosition { get; private set; }
//        public Vector3 nextPosition { get; private set; }
//        public float speed { get; set; }
//        public int stoppingDistance { get; set; }

//        public Action onDestinationReached;
//        public NavGridAgent(NavGrid navGrid, Pathfinding pathfinding, INode startingNode, float speed)
//        {
//            this.navGrid = navGrid;
//            this.pathfinding = pathfinding;
//            this.speed = speed;
//            stoppingDistance = 0;
//            pathFinder = new PathFinder();
//            path = null;
//            currentNode = startingNode;
//            currentPosition = navGrid.GetNodeCenterWorld(currentNode);
//            nextPosition = currentPosition;
//            nextNode = null;
//            destinationNode = null;

//            currentPathNodeIndex = -1;

//            navGrid.SetNodeOccupied(currentNode);         
//        }
//        int N_Reroutes;
//        public void SetDestination(INode destinationNode, int stoppingDistance =0)
//        {
//            N_Reroutes = 0;
//            this.stoppingDistance = stoppingDistance;
//            this.destinationNode = destinationNode;
//            if (path != null)
//                CancelPath();

//            pathfinding.CreatePathRequest(currentNode, destinationNode, pathFinder, this, navGrid.grid.GetAdjacentOccupiedNodes(currentNode));
//        }
//        private void Reroute(INode destinationNode)
//        {
//            N_Reroutes++;
//            this.stoppingDistance = N_Reroutes / 7;
//            this.destinationNode = destinationNode;            

//            pathfinding.CreatePathRequest(currentNode, destinationNode, pathFinder, this, navGrid.grid.GetAdjacentOccupiedNodes(currentNode));
//        }
//        private void CancelPath()
//        {            
//            path = null;
//        }
//        private bool tryAgainFindingPath = false;
//        private int tryAgain_TickCounter;
//        public void SetPath(Path path)
//        {
//            this.path = path;
//            if (path.Edges.Count == 0)
//            {
//                path = null;
//                tryAgainFindingPath = true;
//                tryAgain_TickCounter = 10;
//            }
//            else
//            {
//                currentPathNodeIndex = 0;
//                nextNode = path.Edges[currentPathNodeIndex].End;
//                if (navGrid.IsNodeOccupied(nextNode))
//                {
//                    path = null;
//                    tryAgainFindingPath = true;
//                    tryAgain_TickCounter = 5;
//                }
//                else
//                {
//                    currentSteeringTarget = navGrid.GetNodeCenterWorld(nextNode);
//                    navGrid.SetNodeUnoccupied(currentNode);
//                    navGrid.SetNodeOccupied(nextNode);
//                }
//            }
//        }

//        private void FinishPath()
//        {
//            N_Reroutes = 0;
//            path = null;
//            tryAgainFindingPath = false;
//            destinationNode = null;
//            onDestinationReached?.Invoke();
//        }

//        public bool UpdateAgent(float deltaTime)
//        {
//            currentPosition = nextPosition;
//            if (path == null)
//            {
//                if (tryAgainFindingPath && --tryAgain_TickCounter <= 0)
//                {
//                    tryAgainFindingPath = false;
//                    Reroute(destinationNode);
//                }
//                return false;
//            }
//            else
//            {
//                float movementDelta = speed * deltaTime;
//                Vector3 newPosition = Vector3.MoveTowards(currentPosition, currentSteeringTarget, movementDelta);
//                if (newPosition == currentSteeringTarget)
//                {

//                    if (++currentPathNodeIndex < path.Edges.Count - stoppingDistance)
//                    {
//                        nextNode = path.Edges[currentPathNodeIndex].End;
//                        if (!navGrid.IsNodeOccupied(nextNode))
//                        {
//                            navGrid.SetNodeUnoccupied(currentNode);
//                            navGrid.SetNodeOccupied(nextNode);
//                            movementDelta -= (currentPosition - currentSteeringTarget).magnitude;
//                            currentSteeringTarget = navGrid.GetNodeCenterWorld(nextNode);
//                            newPosition = Vector3.MoveTowards(newPosition, currentSteeringTarget, movementDelta);
//                        }
//                        else
//                        {
//                            if (nextNode == destinationNode)
//                            {
//                                FinishPath();
//                            }
//                            else
//                            {
//                                path = null;
//                                tryAgainFindingPath = true;
//                                tryAgain_TickCounter = 2;
//                            }
//                        }
//                    }
//                    else
//                    {
//                        FinishPath();
//                    }
//                }
//                else if (nextNode != null && (currentSteeringTarget - newPosition).magnitude < (currentPosition - newPosition).magnitude)
//                {
//                    //navGrid.SetNodeUnoccupied(currentNode);
//                    currentNode = path.Edges[currentPathNodeIndex].End;
//                    nextNode = null;
//                }

//                nextPosition = newPosition;
//                return true;
//            }
//        }
//    }
//}









////using System;
////using System.Collections.Generic;
////using System.Text;
////using Roy_T.AStar.Graphs;
////using Roy_T.AStar.Grids;
////using Roy_T.AStar.Paths;
////using Roy_T.AStar.Primitives;
////namespace MultiplayerPlugin
////{
////    public class NavGridAgent 
////    {
////        public PathStatus pathStatus { get; private set; }
////        public Path path;
////        public NavGrid navGrid;
////        public Pathfinding pathfinding;
////        public PathFinder pathFinder;

////        public INode currentNode { get; private set; }        
////        public INode nextNode { get; private set; }
////        public INode destinationNode { get; private set; }

////        private int currentPathNodeIndex;

////        public Vector3 currentSteeringTarget { get; set; }
////        public Vector3 currentPosition { get; private set; }
////        public Vector3 nextPosition { get; private set; }
////        public float speed { get; set; } = 5f;
////        public int stoppingDistance { get; set; } = 0;

////        public Action onDestinationReached;
////        public NavGridAgent(NavGrid navGrid, Pathfinding pathfinding, INode startingNode, float speed)
////        {
////            path = null;
////            this.navGrid = navGrid;
////            pathFinder = new PathFinder();
////            this.pathfinding = pathfinding;
////            currentNode = startingNode;
////            nextNode = null;

////            currentPosition = navGrid.GetNodeCenterWorld(currentNode);
////            nextPosition = currentPosition;

////            currentPathNodeIndex = 0;
////            this.speed = speed;

////            navGrid.SetNodeOccupied(currentNode);
////        }

////        private bool tryAgainFindingPath = false;
////        private int tryAgain_TickCounter;
////        public void SetPath(Path path)
////        {
////            this.path = path;
////            if (path.Edges.Count == 0)
////            {
////                pathStatus = PathStatus.PathNotFound;
////                path = null;                
////                tryAgainFindingPath = true;
////                tryAgain_TickCounter = 5;
////            }
////            else
////            {
////                wait = false;
////                pathStatus = PathStatus.PathComplete;
////                //currentSteeringTarget = currentPosition;
////                //currentPathNodeIndex = -1;                
////                currentPathNodeIndex = 0;
////                currentNode = path.Edges[currentPathNodeIndex].Start;
////                nextNode = path.Edges[currentPathNodeIndex].End;
////                if (navGrid.IsNodeOccupied(nextNode))
////                {
////                    path = null;
////                    tryAgainFindingPath = true;
////                    tryAgain_TickCounter = 5;
////                }
////                else
////                {
////                    currentSteeringTarget = navGrid.GetNodeCenterWorld(nextNode);
////                    currentPosition = navGrid.GetNodeCenterWorld(currentNode);
////                    nextPosition = currentPosition;

////                    navGrid.SetNodeUnoccupied(currentNode);
////                    navGrid.SetNodeOccupied(nextNode);

////                }
////            }
////        }

////        public void SetDestination(INode destinationNode)
////        {
////            this.destinationNode = destinationNode;
////            if (path != null)
////                navGrid.SetNodeUnoccupied(nextNode);                

////            path = null;           
////            pathfinding.CreatePathRequest(currentNode, destinationNode, pathFinder, this);
////        }

////        private bool wait;
////        public bool UpdateAgent(float deltaTime)
////        {
////            currentPosition = nextPosition;
////            if (path == null)
////            {
////                if (tryAgainFindingPath)
////                {
////                    if (--tryAgain_TickCounter <= 0)
////                    {
////                        SetDestination(destinationNode);
////                        tryAgainFindingPath = false;
////                    }
////                }

////                nextPosition = currentPosition;
////                return false;
////            }
////            else
////            {
////                float movementDelta = deltaTime * speed;
////                nextPosition = Vector3.MoveTowards(currentPosition, currentSteeringTarget, movementDelta);
////                if (nextPosition == currentSteeringTarget)
////                {
////                    movementDelta -= (currentPosition - currentSteeringTarget).magnitude;
////                    if (!wait)
////                    {
////                        currentPathNodeIndex++;
////                        if (currentPathNodeIndex < path.Edges.Count - stoppingDistance)
////                        {
////                            currentNode = path.Edges[currentPathNodeIndex].Start;                            
////                            nextNode = path.Edges[currentPathNodeIndex].End;                            
////                        }
////                        else
////                        {
////                            path = null;                            
////                            onDestinationReached?.Invoke();
////                            currentNode = nextNode;
////                            currentPosition = nextPosition;
////                            return true;
////                        }
////                    }          
////                    if (navGrid.IsNodeOccupied(nextNode))
////                    {
////                        if (nextNode == destinationNode)
////                        {
////                                path = null;
////                            //nextPosition = currentPosition;
////                            //wait = true;
////                            if (nextPosition != currentPosition) return true;
////                            else return false;                            
////                        }
////                        else
////                        {
////                            path = null;

////                            pathfinding.CreatePathRequest(currentNode, destinationNode, pathFinder, this, nextNode);                            
////                            //nextNode = null;
////                            if (nextPosition != currentPosition)
////                            {
////                                currentPosition = nextPosition; 
////                                return true;
////                            }
////                            else return false;
////                        }
////                    }
////                    else
////                    {
////                        wait = false;

////                        navGrid.SetNodeUnoccupied(currentNode);
////                        navGrid.SetNodeOccupied(nextNode);                        

////                        currentSteeringTarget = navGrid.GetNodeCenterWorld(nextNode);

////                        nextPosition = Vector3.MoveTowards(nextPosition, currentSteeringTarget, movementDelta);
////                    }
////                }

////                return true;
////            }
////        }
////    }
////}
