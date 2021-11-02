using System;
using System.Collections.Generic;
using System.Text;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
namespace MultiplayerPlugin
{
    public class NavMeshAgent
    {
        public NavMeshPath currentPath;
        public PathFinder pathFinder;

        public Vector3 currentPosition { get; private set; }
        public Vector3 lastPosition { get; private set; }
        public Vector3 destination { get; private set; }
        public Vector3 currentSteeringTarget { get; set; }
        public Vector3 velocity;

        private int currentSteeringTargetIndex;

        public float speed { get; set; }
        public int stoppingDistance { get; set; }

        public Action onDestinationReached;

        private List<Collider> obstacles;
        public NavMeshAgent(Vector3 startingPosition, float speed)
        {
            currentPosition = startingPosition;
            this.speed = speed;
            stoppingDistance = 0;
            pathFinder = new PathFinder();
            currentPath = null;

            currentSteeringTargetIndex = -1;

            obstacles = new List<Collider>();
        }
        public void SetDestination(Vector3 destination, int stoppingDistance = 0)
        {
            this.stoppingDistance = stoppingDistance;
            this.destination = destination;
            if (currentPath != null)
                CancelPath();

            NavMesh.CreatePathRequest(currentPosition, destination, pathFinder, this);
            //pathfinding.CreatePathRequest(currentNode, destinationNode, pathFinder, this, navGrid.grid.GetAdjacentOccupiedNodes(currentNode));
        }
        private void CancelPath()
        {
            currentPath = null;
        }
        public void SetPath(NavMeshPath path)
        {
            currentPath = path;
            if (path == null || path.steeringTargets.Length == 0)
            {
                path = null;
                Console.Error.WriteLine("Path invalid!");
            }
            else
            {
                currentSteeringTargetIndex = 1;
                currentSteeringTarget = currentPath.steeringTargets[currentSteeringTargetIndex];
            }
        }

        private void FinishPath()
        {
            CancelPath();
            onDestinationReached?.Invoke();
        }

        //public bool UpdateAgent(float deltaTime)
        //{
        //    if (currentPath != null)
        //    {
        //        float movementDelta = speed * deltaTime;
        //        Vector3 nextPosition = Vector3.MoveTowards(currentPosition, currentSteeringTarget, movementDelta);
        //        if (nextPosition == currentSteeringTarget)
        //        {
        //            if (++currentSteeringTargetIndex < currentPath.steeringTargets.Length)
        //            {
        //                movementDelta -= (currentPosition - currentSteeringTarget).magnitude;
        //                currentSteeringTarget = currentPath.steeringTargets[currentSteeringTargetIndex];
        //                nextPosition = Vector3.MoveTowards(nextPosition, currentSteeringTarget, movementDelta);
        //            }
        //            else
        //            {
        //                FinishPath();
        //            }
        //        }

        //        currentPosition = nextPosition;
        //        return true;
        //    }
        //    else return false;
        //}
        public bool UpdateAgent(float deltaTime)
        {
            if (currentPath != null)
            {
                //if (Vector3.Distance(currentPosition, currentSteeringTarget) > 0.1f)
                //{
                //    Vector3 steering = new Vector3();
                //    steering += Seek(currentSteeringTarget);
                //    steering += AvoidObstacle();
                //    velocity += steering;

                //    Vector3 position = Vector3.MoveTowards(currentPosition, currentPosition + velocity, speed * deltaTime);       
                //    //position.y = 0f;
                //    currentPosition = position;
                //}
                //else
                //{
                //    if (++currentSteeringTargetIndex < currentPath.steeringTargets.Length)
                //    {
                //        currentSteeringTarget = currentPath.steeringTargets[currentSteeringTargetIndex];
                //        currentSteeringTarget.y = 0f;
                //    }
                //    else
                //    {
                //        transform.position = path.corners[path.corners.Length - 1];
                //        path = null;
                //    }

                //}


                //Vector3 steering = new Vector3();
                //steering += Seek(currentSteeringTarget);
                //steering += AvoidObstacle();
                //velocity += steering;
                //float movementDelta = speed * deltaTime;
                //Vector3 nextPosition = Vector3.MoveTowards(currentPosition, currentPosition + velocity, movementDelta);
                ////if (nextPosition == currentSteeringTarget)
                //if (Vector3.Distance(nextPosition, currentSteeringTarget) <= 0.3f)
                //{
                //    if (++currentSteeringTargetIndex < currentPath.steeringTargets.Length)
                //    {
                //        movementDelta -= (currentPosition - currentSteeringTarget).magnitude;
                //        currentSteeringTarget = currentPath.steeringTargets[currentSteeringTargetIndex];
                //        nextPosition = Vector3.MoveTowards(nextPosition, currentSteeringTarget, movementDelta);
                //    }
                //    else
                //    {
                //        nextPosition = currentPath.steeringTargets[currentPath.steeringTargets.Length - 1];
                //        FinishPath();
                //    }
                //}

                Vector3 desiredVelocity = Seek(currentSteeringTarget);
                desiredVelocity = AvoidObstacle(desiredVelocity);
                desiredVelocity.y = 0f;
                desiredVelocity = desiredVelocity.normalized * speed;
                float movementDelta = speed * deltaTime;
                Vector3 nextPosition = Vector3.MoveTowards(currentPosition, currentPosition + desiredVelocity, movementDelta);
                //if (nextPosition == currentSteeringTarget)
                if (Vector3.Distance(nextPosition, currentSteeringTarget) <= 0.3f)
                {
                    if (++currentSteeringTargetIndex < currentPath.steeringTargets.Length)
                    {
                        movementDelta -= (currentPosition - currentSteeringTarget).magnitude;
                        currentSteeringTarget = currentPath.steeringTargets[currentSteeringTargetIndex];
                        nextPosition = Vector3.MoveTowards(nextPosition, currentSteeringTarget, movementDelta);
                    }
                    else
                    {
                        nextPosition = currentPath.steeringTargets[currentPath.steeringTargets.Length - 1];
                        FinishPath();
                    }
                }
                lastPosition = currentPosition;
                currentPosition = nextPosition;
                return true;
            }
            else return false;
        }

        public float avoidanceWeight = 2.5f;
        private Vector3 AvoidObstacle(Vector3 desiredVelocity)
        {
            Vector3 avoidanceForce = Vector3.zero;
            float minAngle = 25f;
            float minDistance = float.MaxValue;
            Vector3 obstaclePosition = Vector3.zero;

            foreach (var o in obstacles)
            {
                float angle = (float)(Math.Acos((double)Vector3.Dot(desiredVelocity.normalized, (o.center - currentPosition).normalized)) * (180d / Math.PI));
                if (angle < minAngle)
                {
                    float distance = (o.center - currentPosition).magnitude;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        obstaclePosition = o.center;
                    }
                }
            }
            if (obstaclePosition != Vector3.zero)
            {
                //obstaclePosition.y = 0f;
                Vector3 forward = desiredVelocity;
                forward.y = 0f;
                forward = forward.normalized;
                Vector3 up = Vector3.up;
                Vector3 left = Vector3.Cross(forward, up);

                float distance = Vector3.Distance(currentPosition, obstaclePosition);           

                if (NavMesh.CheckVectorsOrder(forward, (obstaclePosition - currentPosition).normalized))
                    avoidanceForce = left * avoidanceWeight / distance;
                else
                    avoidanceForce = -left * avoidanceWeight / distance;
            }
            return (desiredVelocity + avoidanceForce).normalized;
        }
        //private Vector3 AvoidObstacle()
        //{
        //    Vector3 avoidanceForce = Vector3.zero;
        //    Vector3 force = Vector3.zero;
        //    float minAngle = 30f;
        //    Vector3 obstaclePosition = Vector3.zero;

        //    foreach (var o in obstacles)
        //    {                
        //        float angle = (float)Math.Acos((double)Vector3.Dot(velocity.normalized, (o.center - currentPosition).normalized));
        //        if (angle < minAngle)
        //        {
        //            minAngle = angle;
        //            obstaclePosition = o.center;
        //        }
        //    }
        //    if (obstaclePosition != Vector3.zero)
        //    {
        //        avoidanceForce = (currentPosition - obstaclePosition).normalized / Vector3.Distance(currentPosition, obstaclePosition);
        //    }
        //    return avoidanceForce * speed * avoidanceWeight;
        //}
        //private Vector3 AvoidObstacle()
        //{
        //    Vector3 avoidanceForce = Vector3.zero;
        //    Vector3 force = Vector3.zero;
        //    int counter = 0;
        //    foreach (var obstacle in obstacles)
        //    {
        //        float distance = Vector3.Distance(currentPosition, obstacle.center);
        //        if(distance < 6f)                
        //        {                    
        //            force += (currentPosition- obstacle.center).normalized / distance;
        //            counter++;
        //        }
        //    }
        //    if (counter > 0)
        //    {
        //        avoidanceForce = force / (float)counter;
        //    }
        //    return avoidanceForce * speed * avoidanceWeight;
        //}
        private Vector3 Seek(Vector3 target)
        {
            Vector3 desiredVelocity = (target - currentPosition);
            desiredVelocity.y = 0f;
            desiredVelocity = desiredVelocity.normalized;
            //desiredVelocity *= speed;
            //return desiredVelocity - velocity;
            return desiredVelocity;
        }
        public void AddObstacle(Collider obstacle)
        {
            obstacles.Add(obstacle);
        }
        public void RemoveObstacle(Collider obstacle)
        {
            obstacles.Remove(obstacle);
        }
    }
}