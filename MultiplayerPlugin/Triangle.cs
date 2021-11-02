using System;
using System.Collections.Generic;
using System.Text;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Primitives;

namespace MultiplayerPlugin
{
    public class Triangle : INode
    {
        public IList<IEdge> Incoming { get; }
        public IList<IEdge> Outgoing { get; }
        public Position Position { get; }
        public readonly Vector3[] points;
        public readonly Vector2[] points2D;
        public readonly Triangle[] connectedTriangles;
        //public readonly Vector3 pointA;
        //public readonly Vector2 pointA2D;
        //public readonly Vector3 pointB;
        //public readonly Vector2 pointB2D;
        //public readonly Vector3 pointC;
        //public readonly Vector2 pointC2D;
        public readonly Vector3 middle;
        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            points = new Vector3[3];
            points2D = new Vector2[3];
            points[0] = pointA;
            points2D[0] = new Vector2(pointA.x, pointA.z);
            points[1] = pointB;
            points2D[1] = new Vector2(pointB.x, pointB.z);
            points[2] = pointC;
            points2D[2] = new Vector2(pointC.x, pointC.z);
            middle = (pointA + pointB + pointC) / 3f;
            connectedTriangles = new Triangle[3];
            //this.pointA = pointA;
            //this.pointB = pointB;
            //this.pointC = pointC;
            //pointA2D = 
            //pointB2D = new Vector2(pointB.x, pointB.z);
            //pointC2D = new Vector2(pointC.x, pointC.z);

            Incoming = new List<IEdge>();
            Outgoing = new List<IEdge>();
            
            Position = new Position(middle.x, middle.z);
        }

        private void Connect(INode node, Velocity traversalVelocity)
        {
            var edge = new Edge(this, node, traversalVelocity);
            this.Outgoing.Add(edge);
            node.Incoming.Add(edge);
        }
        public bool TryConnect(Triangle triangle, Velocity traversalVelocity)
        {
            if (triangle == this) return false;
            foreach (var t in connectedTriangles) if (t == triangle) return false;

            bool contains_0 = false;
            bool contains_1 = false;
            bool contains_2 = false;

            foreach (var p in triangle.points2D)
            {
                if (p == points2D[0]) contains_0 = true;
                else if (p == points2D[1]) contains_1 = true;
                else if (p == points2D[2]) contains_2 = true;
            }

            if (contains_0 && contains_1) { connectedTriangles[0] = triangle; Connect(triangle, traversalVelocity); return true; }
            else if (contains_1 && contains_2) { connectedTriangles[1] = triangle; Connect(triangle, traversalVelocity); return true; }
            else if (contains_2 && contains_0) { connectedTriangles[2] = triangle; Connect(triangle, traversalVelocity); return true; }
            else return false;
        }
        //public void Disconnect(INode node)
        //{
        //    for (var i = this.Outgoing.Count - 1; i >= 0; i--)
        //    {
        //        var edge = this.Outgoing[i];
        //        if (edge.End == node)
        //        {
        //            this.Outgoing.Remove(edge);
        //            node.Incoming.Remove(edge);
        //        }
        //    }
        //}
        //public bool IsConnectedWith(INode node)
        //{
        //    foreach (var outgoingEdge in Outgoing)
        //    {
        //        if (outgoingEdge.End == node) return true;
        //    }
        //    return false;
        //}

        //public bool SharesEdgeWith(Triangle triangle)
        //{
        //    Vector3 sumA = pointA + pointB + pointC;
        //    Vector3 sumB = triangle.pointA + triangle.pointB + triangle.pointC;
        //    Vector3 diff = sumA - sumB;
        //    return
        //        (diff == pointA - triangle.pointA) ||
        //        (diff == pointA - triangle.pointB) ||
        //        (diff == pointA - triangle.pointC) ||

        //        (diff == pointB - triangle.pointA) ||
        //        (diff == pointB - triangle.pointB) ||
        //        (diff == pointB - triangle.pointC) ||

        //        (diff == pointC - triangle.pointA) ||
        //        (diff == pointC - triangle.pointB) ||
        //        (diff == pointC - triangle.pointC);
        //    //return
        //    //    (pointA == triangle.pointA && pointB == triangle.pointB && pointC != triangle.pointC) ||
        //    //    (pointA == triangle.pointA && pointB == triangle.pointC && pointC != triangle.pointB) ||
        //    //    (pointA == triangle.pointA && pointC == triangle.pointB && pointB != triangle.pointC) ||
        //    //    (pointA == triangle.pointA && pointC == triangle.pointC && pointB != triangle.pointB) ||

        //    //    (pointA == triangle.pointB && pointB == triangle.pointA && pointC != triangle.pointC) ||
        //    //    (pointA == triangle.pointB && pointB == triangle.pointC && pointC != triangle.pointA) ||
        //    //    (pointA == triangle.pointB && pointC == triangle.pointA && pointB != triangle.pointC) ||
        //    //    (pointA == triangle.pointB && pointC == triangle.pointC && pointB != triangle.pointA) ||

        //    //    (pointA == triangle.pointC && pointB == triangle.pointA && pointC != triangle.pointB) ||
        //    //    (pointA == triangle.pointC && pointB == triangle.pointB && pointC != triangle.pointA) ||
        //    //    (pointA == triangle.pointC && pointC == triangle.pointA && pointB != triangle.pointB) ||
        //    //    (pointA == triangle.pointC && pointC == triangle.pointB && pointB != triangle.pointA);
        //}
    }
}
