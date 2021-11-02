using System;
using System.Collections.Generic;
using System.Text;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
using System.IO;
using Roy_T.AStar.Graphs;

namespace MultiplayerPlugin
{
    public class NavGrid
    {
        public Grid grid;
        private Vector3 gridPosition;
        private Vector3 cellSize;

        //public List<INode> occupiedNodes;

        public Velocity traversalVelocity;
        public NavGrid(string pathToGridFile, Velocity traversalVelocity)
        {
            this.traversalVelocity = traversalVelocity;
            CreateGridFromFile(pathToGridFile);

            //occupiedNodes = new List<INode>();
        }
        public bool IsNodeAnObstacle(INode node)
        {
            return node.Outgoing.Count == 0 && node.Incoming.Count == 0;
            //return node.Outgoing.Count == 0 || node.Incoming.Count == 0;
        }
        //public bool IsNodeOccupied(INode node)
        //{            
        //    return node.IsOccupied;

        //}
        //public void SetNodeOccupied(INode node)
        //{
        //    node.IsOccupied = true;            
        //}
        //public void SetNodeUnoccupied(INode node)
        //{            
        //    node.IsOccupied = false;            
        //}

        public INode WorldToCell(Vector3 worldPos)
        {
            Vector3 pos = worldPos - gridPosition;
            int nodeX = (int)(((float)Math.Floor(pos.x / cellSize.x)) * cellSize.x);
            int nodeY = (int)(((float)Math.Floor(pos.z / cellSize.z)) * cellSize.z);

            return grid.GetNode(new GridPosition(nodeX, nodeY));
        }
        public Vector3 GetNodeCenterWorld(INode node)
        {
            return gridPosition + new Vector3(node.Position.X, 0f, node.Position.Y);
        }
        private void CreateGridFromFile(string pathToTxt)
        {
            using (StreamReader reader = File.OpenText(pathToTxt))
            {
                string line = reader.ReadLine();
                string gridPosXString = line.Split('=')[1].Trim();
                float gridPosX = float.Parse(gridPosXString);

                line = reader.ReadLine();
                string gridPosYString = line.Split('=')[1].Trim();
                float gridPosY = float.Parse(gridPosYString);

                line = reader.ReadLine();
                string gridPosZString = line.Split('=')[1].Trim();
                float gridPosZ = float.Parse(gridPosZString);

                gridPosition = new Vector3(gridPosX, gridPosY, gridPosZ);

                line = reader.ReadLine();
                string widthString = line.Split('=')[1].Trim();
                int width = int.Parse(widthString);

                line = reader.ReadLine();
                string heightString = line.Split('=')[1].Trim();
                int height = int.Parse(heightString);

                line = reader.ReadLine();
                string cellSizeXString = line.Split('=')[1].Trim();
                float cellSizeX = float.Parse(cellSizeXString);

                line = reader.ReadLine();
                string cellSizeYString = line.Split('=')[1].Trim();
                float cellSizeY = float.Parse(cellSizeYString);

                cellSize = new Vector3(cellSizeX, 0f, cellSizeY);
                grid = Grid.CreateGridWithLateralAndDiagonalConnections(new GridSize(width, height), new Size(Distance.FromMeters(cellSizeX), Distance.FromMeters(cellSizeY)), traversalVelocity);

                //line = reader.ReadLine();
                //string walkableNodesLengthString = line.Split('=')[1].Trim();
                //int walkableNodesLength = int.Parse(walkableNodesLengthString);

                //for (int i = 0; i < walkableNodesLength; i++)
                //{
                //    reader.ReadLine();
                //}

                line = reader.ReadLine();
                string obstaclesLengthString = line.Split('=')[1].Trim();
                int obstaclesLength = int.Parse(obstaclesLengthString);

                for (int i = 0; i < obstaclesLength; i++)
                {

                    line = reader.ReadLine();
                    var coordsString = line.Split(':');
                    int coordX = int.Parse(coordsString[0].Trim());
                    int coordY = int.Parse(coordsString[1].Trim());
                    GridPosition obstaclePosition = new GridPosition(coordX, coordY);
                    grid.DisconnectNode(obstaclePosition);
                }

            }
        }
    }
}

