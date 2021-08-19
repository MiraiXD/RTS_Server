using System;
using System.Collections.Generic;
using System.Text;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
namespace MultiplayerPlugin
{
    public class NavGridAgent 
    {
        public PathStatus pathStatus { get; private set; }
        public Path path;
        public NavGrid navGrid;
        public Pathfinding pathfinding;

        public INode currentNode { get; private set; }        
        public INode nextNode { get; private set; }
        public INode destinationNode { get; private set; }

        private int currentPathNodeIndex;
        
        public Vector3 currentSteeringTarget { get; set; }
        public Vector3 currentPosition { get; private set; }
        public Vector3 nextPosition { get; private set; }
        public float speed { get; set; } = 5f;
        public int stoppingDistance { get; set; } = 0;

        public Action onDestinationReached;
        public NavGridAgent(NavGrid navGrid, Pathfinding pathfinding, INode startingNode, float speed)
        {
            path = null;
            this.navGrid = navGrid;
            this.pathfinding = pathfinding;
            currentNode = startingNode;
            nextNode = null;

            currentPosition = navGrid.GetNodeCenterWorld(currentNode);
            nextPosition = currentPosition;

            currentPathNodeIndex = 0;
            this.speed = speed;

            navGrid.SetNodeOccupied(currentNode);
        }

        private bool tryAgainFindingPath = false;
        private int tryAgain_TickCounter;
        public void SetPath(Path path)
        {
            this.path = path;
            if (path == null)
            {
                pathStatus = PathStatus.PathNotFound;
                tryAgainFindingPath = true;
                tryAgain_TickCounter = 5;
            }
            else
            {
                pathStatus = PathStatus.PathComplete;
                currentSteeringTarget = currentPosition;
                currentPathNodeIndex = -1;

            }
        }

        public void SetDestination(INode destinationNode)
        {
            this.destinationNode = destinationNode;

            if (path != null)
                navGrid.SetNodeUnoccupied(nextNode);

            path = null;

            pathfinding.CreatePathRequest(currentNode, destinationNode, this);
        }


        public bool UpdateAgent(float deltaTime)
        {
            currentPosition = nextPosition;
            if (path == null)
            {
                if (tryAgainFindingPath)
                {
                    if (--tryAgain_TickCounter <= 0)
                    {
                        SetDestination(destinationNode);
                        tryAgainFindingPath = false;
                    }
                }

                nextPosition = currentPosition;
                return false;
            }
            else
            {
                float movementDelta = deltaTime * speed;
                nextPosition = Vector3.MoveTowards(currentPosition, currentSteeringTarget, movementDelta);
                if (nextPosition == currentSteeringTarget)
                {
                    movementDelta -= (currentPosition - currentSteeringTarget).magnitude;

                    currentPathNodeIndex++;
                    if (currentPathNodeIndex < path.Edges.Count - stoppingDistance)
                    {
                        //prevNode = path.nodes[Mathf.Clamp(currentPathNodeIndex - 1, 0, path.nodes.Length - 1)];
                        currentNode = path.Edges[currentPathNodeIndex].Start;
                        nextNode = path.Edges[currentPathNodeIndex].End;
                        //int nextNodeIndex = currentPathNodeIndex + 1;
                        //if (nextNodeIndex >= path.Edges.Count) nextNodeIndex = path.Edges.Count-1;
                        ////nextNode = path.Edges[Mathf.Clamp(currentPathNodeIndex + 1, 0, path.Edges.Count)];
                        //nextNode = path.Edges[nextNodeIndex].Start;
                    }
                    else
                    {
                        path = null;
                        onDestinationReached?.Invoke();
                        nextPosition = currentSteeringTarget;

                        //prevNode = currentNode;
                        currentNode = nextNode;
                        return true;
                    }

                    if (navGrid.IsNodeOccupied(nextNode))
                    {
                        if (nextNode == destinationNode)
                        {
                            path = null;
                            nextPosition = currentPosition;
                            return false;
                        }
                        else
                        {
                            path = null;
                            pathfinding.CreatePathRequest(currentNode, destinationNode, this, nextNode);
                            nextNode = null;
                            nextPosition = currentPosition;
                            return false;
                        }
                    }
                    else
                    {
                        navGrid.SetNodeOccupied(nextNode);
                        navGrid.SetNodeUnoccupied(currentNode);

                        currentSteeringTarget = navGrid.GetNodeCenterWorld(nextNode);

                        nextPosition = Vector3.MoveTowards(nextPosition, currentSteeringTarget, movementDelta);
                    }
                }

                return true;
            }
        }
    }
}
