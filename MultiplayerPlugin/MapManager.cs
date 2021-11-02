using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MultiplayerPlugin
{
    public static class MapManager
    {
        public static Vector3 mapOrigin;
        public static float mapWidth, mapHeight;
        public static Sector[,] sectors;
        public static int horizontalSectorCount = 10;
        public static int verticalSectorCount = 10;
        public static float sectorWidth, sectorHeight;
        public static void Init(Vector3 _mapOrigin, float _width, float _height)
        {
            mapOrigin = _mapOrigin;
            mapWidth = _width;
            mapHeight = _height;
            DivideMap();
        }
        private static void DivideMap()
        {
            sectors = new Sector[horizontalSectorCount, verticalSectorCount];
            sectorWidth = mapWidth / (float)horizontalSectorCount;
            sectorHeight = mapHeight / (float)verticalSectorCount;
            for (int i = 0; i < horizontalSectorCount; i++)
            {
                for (int j = 0; j < verticalSectorCount; j++)
                {
                    sectors[i, j] = new Sector(mapOrigin + i * sectorWidth * Vector3.right + j * sectorHeight * Vector3.forward, sectorWidth, sectorHeight);
                }
            }
        }
        public static void AssignTrianglesToSectors(Triangle[] triangles)
        {
            foreach (var sector in sectors)
            {
                Vector3 bottomLeft = sector.origin;
                Vector2 bottomLeft2D = new Vector2(bottomLeft.x, bottomLeft.z);
                Vector3 bottomRight = sector.origin + sector.width * Vector3.right;
                Vector2 bottomRight2D = new Vector2(bottomRight.x, bottomRight.z);
                Vector3 topLeft = sector.origin + sector.height * Vector3.forward;
                Vector2 topLeft2D = new Vector2(topLeft.x, topLeft.z);
                Vector3 topRight = sector.origin + sector.width * Vector3.right + sector.height * Vector3.forward;
                Vector2 topRight2D = new Vector2(topRight.x, topRight.z);

                List<Triangle> trianglesIntersectingSector = new List<Triangle>();
                foreach (var triangle in triangles)
                {
                    Vector2 pointA2D = triangle.points2D[0];
                    Vector2 pointB2D = triangle.points2D[1];
                    Vector2 pointC2D = triangle.points2D[2];


                    bool intersects = LineLineIntersection(bottomLeft2D, bottomRight2D, pointA2D, pointB2D) ||
                        LineLineIntersection(bottomLeft2D, bottomRight2D, pointA2D, pointC2D) ||
                        LineLineIntersection(bottomLeft2D, bottomRight2D, pointB2D, pointC2D) ||

                        LineLineIntersection(bottomLeft2D, topLeft2D, pointA2D, pointB2D) ||
                        LineLineIntersection(bottomLeft2D, topLeft2D, pointA2D, pointC2D) ||
                        LineLineIntersection(bottomLeft2D, topLeft2D, pointB2D, pointC2D) ||

                        LineLineIntersection(bottomRight2D, topRight2D, pointA2D, pointB2D) ||
                        LineLineIntersection(bottomRight2D, topRight2D, pointA2D, pointC2D) ||
                        LineLineIntersection(bottomRight2D, topRight2D, pointB2D, pointC2D) ||

                        LineLineIntersection(topLeft2D, topRight2D, pointA2D, pointB2D) ||
                        LineLineIntersection(topLeft2D, topRight2D, pointA2D, pointC2D) ||
                        LineLineIntersection(topLeft2D, topRight2D, pointB2D, pointC2D);

                    bool triangleInsideSector = PointInRectangle(pointA2D, bottomRight2D, bottomLeft2D, topLeft2D, topRight2D) || PointInRectangle(pointB2D, bottomRight2D, bottomLeft2D, topLeft2D, topRight2D) || PointInRectangle(pointC2D, bottomRight2D, bottomLeft2D, topLeft2D, topRight2D);                    
                    if (intersects || triangleInsideSector)
                    {
                        trianglesIntersectingSector.Add(triangle);
                    }
                }

                sector.navMeshTriangles = trianglesIntersectingSector.ToArray();
            }
        }
        public static Triangle WorldPosToNavMeshTriangle(Vector3 worldPos)
        {         
            Sector sector = WorldPosToSector(worldPos);
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.z);
            float difference = 0.001f;
            Triangle closestTriangle = null;
            foreach (var triangle in sector.navMeshTriangles)
            {
                if (PointInTriangle(worldPos2D, triangle.points2D[0], triangle.points2D[1], triangle.points2D[2], out float diff))
                    return triangle;
                else if (diff < difference)
                {
                    difference = diff;
                    closestTriangle = triangle;
                }
            }
            Console.WriteLine();
            Console.WriteLine("DIFFERENCE: " + difference);
            Console.WriteLine();
            return closestTriangle;            
        }
        private static Sector WorldPosToSector(Vector3 worldPos)
        {
            int x = (int)((worldPos - mapOrigin).x / sectorWidth);
            int z = (int)((worldPos - mapOrigin).z / sectorHeight);          
            return sectors[x, z];
        }
        public static bool PointInRectangle(Vector2 p, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 r4)
        {
            var AB = r2 - r1;
            var AM = p - r1;
            var BC = r3 - r2;
            var BM = p - r2;
            var dotABAM = Vector2.Dot(AB, AM);
            var dotABAB = Vector2.Dot(AB, AB);
            var dotBCBM = Vector2.Dot(BC, BM);
            var dotBCBC = Vector2.Dot(BC, BC);
            return 0 <= dotABAM && dotABAM <= dotABAB && 0 <= dotBCBM && dotBCBM <= dotBCBC;
        }
        public static bool PointInTriangle(Vector2 p, Vector2 t1, Vector2 t2, Vector2 t3, out float difference)
        {
            Vector2 v0 = t1;
            Vector2 v1 = t2 - t1;
            Vector2 v2 = t3 - t1;

            float a = (det(p, v2) - det(v0, v2)) / det(v1, v2);
            float b = -(det(p, v1) - det(v0, v1)) / det(v1, v2);       
            
            difference = 0f;
            //return a > -0.01f && b > -0.01f && a + b < 1.01f;
            if(a > 0f && b > 0f && a+b < 1f)
            {                
                return true;
            }
            else
            {                
                if (a <= 0f) difference += -a;
                if (b <= 0f) difference += -b;
                if (a + b >= 1f) difference += a + b - 1f;
                return false;
            }

            float det(Vector2 u, Vector2 v)
            {
                return u.x * v.y - u.y * v.x;
            }
        }
        //private static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
        //{
        //    return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        //}

        //public static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        //{
        //    float d1, d2, d3;
        //    bool has_neg, has_pos;

        //    d1 = sign(pt, v1, v2);
        //    d2 = sign(pt, v2, v3);
        //    d3 = sign(pt, v3, v1);

        //    has_neg = (d1 <= 0f) || (d2 <= 0f) || (d3 <= 0f);
        //    has_pos = (d1 >= 0f) || (d2 >= 0f) || (d3 >= 0f);

        //    return !(has_neg && has_pos);
        //}
        private static bool LineLineIntersection(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            Vector2 r = q1 - p1;
            Vector2 s = q2 - p2;
            float r_x_s = Vector2.Cross(r, s);
            Vector2 qmp = p2 - p1;
            float qmp_x_r = Vector2.Cross(qmp, r);
            float qmp_x_s = Vector2.Cross(qmp, s);
            float t = qmp_x_s / r_x_s;
            float u = qmp_x_r / r_x_s;
            if (r_x_s == 0f && qmp_x_r == 0f)
            {
                //colinear
                float t0 = Vector2.Dot(qmp, r) / Vector2.Dot(r, r);
                float t1 = t0 + Vector2.Dot(s, r) / Vector2.Dot(r, r);
                //Console.WriteLine(p1.x + " , " + p1.y);
                //Console.WriteLine(q1.x + " , " + q1.y);
                //Console.WriteLine(p2.x + " , " + p2.y);
                //Console.WriteLine(q2.x + " , " + q2.y);
                //Console.WriteLine("1");

                return false; // :/
            }
            else if (r_x_s == 0f && qmp_x_r != 0f)
            {
                // parallel and non intersecting
                return false;
            }
            else if (r_x_s != 0f && t >= 0f && t <= 1f && u >= 0f && u <= 1f)
            {
                Vector2 intersectionPoint = p1 + t * r;
                return true;
            }
            else
            {
                // not parallel but do not intersect
                return false;
            }
        }
    }
    public class Sector
    {
        public Vector3 origin;
        public float width, height;
        public Triangle[] navMeshTriangles;
        public Sector(Vector3 origin, float width, float height)
        {
            this.origin = origin;
            this.width = width;
            this.height = height;
        }
    }
}
