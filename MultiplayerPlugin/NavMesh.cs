using Roy_T.AStar.Graphs;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace MultiplayerPlugin
{
    public static class NavMesh
    {
        private static Vector3[] vertices;
        private static int[] indices;
        private static Velocity traversalVelocity;
        public static Triangle[] triangles;
        private static List<PathRequest> pathRequests;
        public static void Init(Vector3[] _vertices, int[] _indices, Velocity _traversalVelocity)
        {
            pathRequests = new List<PathRequest>();
            vertices = _vertices;
            indices = _indices;
            traversalVelocity = _traversalVelocity;
            ConstructNodes(vertices, indices, traversalVelocity);
        }
        private static void ConstructNodes(Vector3[] vertices, int[] indices, Velocity traversalVelocity)
        {
            triangles = new Triangle[indices.Length / 3];
            for (int i = 0; i < triangles.Length; i++)
            {
                int indexA = 3 * i;
                int indexB = 3 * i + 1;
                int indexC = 3 * i + 2;
                triangles[i] = new Triangle(vertices[indices[indexA]], vertices[indices[indexB]], vertices[indices[indexC]]);
            }

            foreach (var t1 in triangles)
            {
                foreach (var t2 in triangles)
                {
                    t1.TryConnect(t2, traversalVelocity);
                    //if (t1 != t2 && t1.SharesEdgeWith(t2) && !t1.IsConnectedWith(t2))
                    //{                        
                    //    t1.Connect(t2,traversalVelocity);
                    //}
                }
            }
        }
        public static void FindAllPathsParallel()
        {
            lock (pathRequests)
            {
                if (pathRequests.Count > 0)
                {
                    if (pathRequests.Count == 1)
                    {              
                        PathRequest request = pathRequests[0];
                        NavMeshPath path = HandlePathRequest(request);
                        request.agent.SetPath(path);
                    }
                    else
                    {
                        Parallel.For(0, pathRequests.Count, (index) =>
                        {
                            PathRequest request = pathRequests[index];
                            NavMeshPath path = HandlePathRequest(request);
                            request.agent.SetPath(path);
                        });
                    }

                    pathRequests.Clear();
                }
            }
        }
        public static void CreatePathRequest(Vector3 startPosition, Vector3 endPosition, PathFinder pathFinder, NavMeshAgent agent)
        {
            PathRequest pathRequest = new PathRequest(startPosition, endPosition, pathFinder, agent);
            pathRequests.Add(pathRequest);
        }
        private static NavMeshPath HandlePathRequest(PathRequest request)
        {
            Triangle startTriangle = MapManager.WorldPosToNavMeshTriangle(request.startPosition);
            Triangle endTriangle = MapManager.WorldPosToNavMeshTriangle(request.endPosition);
            if (startTriangle == null) { Console.Error.WriteLine("No such start triangle!"); return null; }
            if (endTriangle == null) { Console.Error.WriteLine("No such end triangle!"); return null; }


            NavMeshPath navMeshPath;
            if (startTriangle != endTriangle)
            {
                Path path = request.pathFinder.FindPath(startTriangle, endTriangle, traversalVelocity);
                if (path == null || path.Edges.Count == 0) Console.Error.WriteLine("Could not find a path between triangles!");
     
                Triangle[] triangles = new Triangle[path.Edges.Count + 1];
                for (int i = 0; i < path.Edges.Count; i++)
                {
                    triangles[i] = path.Edges[i].Start as Triangle;
                }
                triangles[triangles.Length - 1] = path.Edges[path.Edges.Count - 1].End as Triangle;
   
                List<Vector3> steeringTargets = StringPulling(request.startPosition, request.endPosition, triangles);
                navMeshPath = new NavMeshPath(steeringTargets.ToArray());
            }
            else
            {
                Vector3[] steeringTargets = new Vector3[2];
                steeringTargets[0] = request.startPosition;
                steeringTargets[1] = request.endPosition;
                navMeshPath = new NavMeshPath(steeringTargets);
            }

            return navMeshPath;
        }
    
        private static List<Vector3> StringPulling(Vector3 startPosition, Vector3 endPosition, Triangle[] nodes)
        {           
            List<Vector3> steeringTargets = new List<Vector3>();
            Vector3[] leftVertices = new Vector3[nodes.Length + 1];
            Vector3[] rightVertices = new Vector3[nodes.Length + 1];
            Vector3 apex = startPosition;
            int left = 0;
            int right = 0;

            steeringTargets.Add(startPosition);
            // Initialise portal vertices.
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                for (int j = 0; j < nodes[i].connectedTriangles.Length; j++)
                {
                    if (nodes[i].connectedTriangles[j] == nodes[i + 1])
                    {
                        Triangle triangle = nodes[i];
                        int k = j + 1 >= triangle.connectedTriangles.Length ? 0 : j + 1;

                        leftVertices[i + 1] = triangle.points[j];
                        rightVertices[i + 1] = triangle.points[k];
                        break;
                    }
                }
            }

            leftVertices[0] = startPosition;
            rightVertices[0] = startPosition;
            leftVertices[leftVertices.Length-1] = endPosition;
            rightVertices[rightVertices.Length-1] = endPosition;           

            //// Step through channel.
            for (int i = 1; i < nodes.Length+1; i++)
            {                
                Vector3 newSide = leftVertices[i] - apex;
                if (CheckVectorsOrder(leftVertices[left] - apex, newSide))
                {
                    if (leftVertices[i] == leftVertices[left] || CheckVectorsOrder(newSide, rightVertices[right] - apex))
                    {
                        left = i;
                    }
                    else
                    {
                        steeringTargets.Add(rightVertices[right]);
                        apex = rightVertices[right];                        
                        left = right;
                        i = right;                     
                        continue;
                    }
                }                

                newSide = rightVertices[i] - apex;
                if (CheckVectorsOrder(newSide, rightVertices[right] - apex))
                {
                    if (rightVertices[i] == rightVertices[right] || CheckVectorsOrder(leftVertices[left] - apex, newSide))
                    {
                        right = i;
                    }
                    else
                    { 
                        steeringTargets.Add(leftVertices[left]);
                        apex = leftVertices[left];                 
                        right = left;
                        i = left;                  
                        continue;
                    }
                }                
            }

            steeringTargets.Add(endPosition);
            return steeringTargets;
        }
        public static bool CheckVectorsOrder(Vector3 left, Vector3 right)
        {
            return CheckVectorsOrder(new Vector2(left.x, left.z), new Vector2(right.x, right.z));
        }
        public static bool CheckVectorsOrder(Vector2 left, Vector2 right)
        {
            return right.x * left.y - right.y * left.x >= 0f;
        }
    }

    public class NavMeshPath
    {
        public Vector3[] steeringTargets;
        public NavMeshPath(Vector3[] steeringTargets)
        {
            this.steeringTargets = steeringTargets;
        }
    }
    public class PathRequest
    {
        public Vector3 startPosition;
        public Vector3 endPosition;
        public PathFinder pathFinder;
        public NavMeshAgent agent;

        public PathRequest(Vector3 startPosition, Vector3 endPosition, PathFinder pathFinder, NavMeshAgent agent)
        {
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.pathFinder = pathFinder;
            this.agent = agent;
        }
    }
}
