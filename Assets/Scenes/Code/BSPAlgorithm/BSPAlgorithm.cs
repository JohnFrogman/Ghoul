using UnityEngine;
using System.Collections.Generic;

public class BSPAlgorithm
{
    private Vector2Int m_minNodeSize;

    public class Parameters
    {
        public readonly int MinNumRooms;
        public readonly int MinRoomWidth;
        public readonly int MinRoomHeight;
        public readonly CorridorType CorridorType;

        public Parameters(int minNumRooms, int minRoomWidth, int minRoomHeight, CorridorType type)
        {
            MinNumRooms = minNumRooms;
            MinRoomWidth = minRoomWidth;
            MinRoomHeight = minRoomHeight;
            CorridorType = type;
        }

        public static Parameters Default
        {
            get
            {
                return new Parameters(5, 5, 5, CorridorType.Bent);
            }
        }
    }

    public enum CorridorType
    {
        Straight,
        Bent,
        ZigZag,
    }

    public static void Apply(Level level, Parameters parameters, int seed)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        BSPAlgorithm bsp = new BSPAlgorithm();

        bsp.m_minNodeSize = new Vector2Int(level.Grid.Tiles.GetLength(0) / parameters.MinNumRooms, level.Grid.Tiles.GetLength(1) / parameters.MinNumRooms);
        //Random.InitState(seed);

        var root = new Node(level.Grid.Tiles);
        bsp.DivideGrid(level.Grid, root);

        level.Rooms = new HashSet<Tile[,]>();
        var roomGenerator = new RoomGenerator(level, new Vector2Int(parameters.MinRoomWidth, parameters.MinRoomHeight));
        roomGenerator.PlaceRooms(root);

        var corridorGenerator = new CorridorGenerator(level.Grid, parameters.CorridorType);
        corridorGenerator.PlaceCorridors(root);

        stopwatch.Stop();
        Debug.Log("BSPAlgorithm took " + stopwatch.ElapsedMilliseconds + " milliseconds to complete.");
    }

	private void DivideGrid(Grid grid, Node node)
	{
		int splitPoint;

		var gridX = node.Size.x;	
		var gridY = node.Size.y;

		if (Random.Range(0, 2) == 0) 
		{
			splitPoint = Random.Range (m_minNodeSize.x, gridX-m_minNodeSize.x);
			node.LeftChild = new Node(Grid.GetSection(node.Grid, 0, splitPoint, 0, gridY));
			node.RightChild = new Node(Grid.GetSection(node.Grid, splitPoint, gridX, 0, gridY));
		} 

		else 
		{
			splitPoint = Random.Range (m_minNodeSize.y, gridY- m_minNodeSize.y);
			node.LeftChild = new Node (Grid.GetSection(node.Grid, 0, gridX, 0, splitPoint));
			node.RightChild = new Node(Grid.GetSection(node.Grid, 0, gridX, splitPoint, gridY));
		}

		if (node.LeftChild.Size.x > m_minNodeSize.x && node.LeftChild.Size.y > m_minNodeSize.y) 
			DivideGrid (grid, node.LeftChild);
		if (node.RightChild.Size.x > m_minNodeSize.x && node.RightChild.Size.y > m_minNodeSize.y)
			DivideGrid (grid, node.RightChild);
	}

    private class Node
    {
        public Tile[,] Grid;

        public Tile[,] Room;

        public Node LeftChild;
        public Node RightChild;

        public Vector2Int Size;

        public Node(Tile[,] grid)
        {
            Grid = grid;
            Size = new Vector2Int(grid.GetLength(0), grid.GetLength(1));
        }
    }

    private class RoomGenerator
    {
        private Vector2Int m_minSize;
        private Level m_level;

        public RoomGenerator(Level level, Vector2Int minSize)
        {
            m_minSize = minSize;
            m_level = level;
        }

        public void PlaceRooms(Node node)
        {
            if (node.LeftChild == null && node.RightChild == null)
                GenNewRoom(node);
            else
            {
                PlaceRooms(node.LeftChild);
                PlaceRooms(node.RightChild);
            }
        }

        private void GenNewRoom(Node node)
        {
            int nodeX = node.Size.x;
            int nodeY = node.Size.y;

            for (int i = 0; i < 50; i++)
            {
                int botLeftX = Random.Range(0, nodeX / 2);
                int botLeftY = Random.Range(0, nodeY / 2);
                int topRightX = Random.Range(nodeX / 2, nodeX);
                int topRightY = Random.Range(nodeY / 2, nodeY);

                int width = topRightX - botLeftX;
                int height = topRightY - botLeftY;

                float maxWidth = nodeX;
                float maxHeight = nodeY;

                int varyRoom = Random.Range(0, 3);
                if (varyRoom == 1)
                {
                    maxWidth = Mathf.RoundToInt(maxWidth / 2f);
                    maxHeight = Mathf.RoundToInt(maxHeight / 2f);
                }

                if (varyRoom == 2)
                {
                    maxWidth = Mathf.RoundToInt(maxWidth / 1.5f);
                    maxHeight = Mathf.RoundToInt(maxHeight / 1.5f);
                }

                if ((width > m_minSize.x) && (height > m_minSize.y))
                {
                    if ((width < maxWidth) && (height < maxHeight))
                    {
                        node.Room = Grid.GetSection(node.Grid, botLeftX, topRightX, botLeftY, topRightY);
                        Grid.SetSection(node.Room, Tile.Types.Floor);
                        m_level.Rooms.Add(node.Room);
                        break;
                    }
                }
            }
        }
    }

    private class CorridorGenerator
    {
        private Grid m_grid;
        private CorridorType m_type;

        public CorridorGenerator(Grid grid, CorridorType type)
        {
            m_grid = grid;
            m_type = type;
        }

        public void PlaceCorridors(Node n)
        {
            if (n.LeftChild != null && n.RightChild != null)
            {
                GenNewCorridor(GetRoom(n.LeftChild), GetRoom(n.RightChild));
                PlaceCorridors(n.LeftChild);
                PlaceCorridors(n.RightChild);
            }
        }

        private Tile[,] GetRoom(Node node)
        {
            if (node.Room != null)
                return node.Room;
            else
            {
                Tile[,] lRoom = null;
                Tile[,] rRoom = null;

                if (node.LeftChild != null)
                    lRoom = GetRoom(node.LeftChild);
                if (node.RightChild != null)
                    rRoom = GetRoom(node.RightChild);

                if (lRoom == null && rRoom == null)
                    return null;
                else if (lRoom == null)
                    return rRoom;
                else if (rRoom == null)
                    return lRoom;
                else if (Random.Range(0, 2) == 0)
                    return lRoom;
                else
                    return rRoom;
            }
        }

        private void GenNewCorridor(Tile[,] roomOne, Tile[,] roomTwo)
        {
            if (roomOne == null || roomTwo == null)
                return;

            int[] lengths = GetRoomLengths(roomOne, roomTwo);

            switch (m_type)
            {
                case CorridorType.Straight:
                    PlaceStraightCorridor(roomOne, roomTwo, lengths);
                    break;
                case CorridorType.Bent:
                    PlaceBentCorridor(roomOne, roomTwo, lengths);
                    break;
                case CorridorType.ZigZag:
                    PlaceZCorridor(roomOne, roomTwo, lengths);
                    break;
            }
        }

        private void PlaceStraightCorridor(Tile[,] roomOne, Tile[,] roomTwo, int[] lengths)
        {
            Vector2 botLeftOne = roomOne[0, 0].GridPosition;
            Vector2 topRightOne = roomOne[roomOne.GetLength(0) - 1, roomOne.GetLength(1) - 1].GridPosition;
            Vector2 botLeftTwo = roomTwo[0, 0].GridPosition;
            Vector2 topRightTwo = roomTwo[roomTwo.GetLength(0) - 1, roomTwo.GetLength(1) - 1].GridPosition;

            bool xAligned = false;
            bool yAligned = false;

            if (CheckFacing(botLeftOne.x, topRightOne.x, botLeftTwo.x, topRightTwo.x))
                yAligned = true;
            else if (CheckFacing(botLeftOne.y, topRightOne.y, botLeftTwo.y, topRightTwo.y))
                xAligned = true;

            if (xAligned)
            {
                if (roomOne[0, 0].GridPosition.x > roomTwo[0, 0].GridPosition.x)
                {
                    Tile[,] temp = roomOne;
                    roomOne = roomTwo;
                    roomTwo = temp;
                }

                PlaceStraightXCorridor(roomOne, roomTwo, lengths);

            }

            else if (yAligned)
            {
                if (roomOne[0, 0].GridPosition.y > roomTwo[0, 0].GridPosition.y)
                {
                    Tile[,] temp = roomOne;
                    roomOne = roomTwo;
                    roomTwo = temp;
                }

                PlaceStraightYCorridor(roomOne, roomTwo, lengths);
            }
            else
            {
                PlaceBentCorridor(roomOne, roomTwo, lengths);
            }
        }

        private void PlaceStraightXCorridor(Tile[,] roomOne, Tile[,] roomTwo, int[] roomLengths)
        {
            List<int> possiblePositions = new List<int>();

            for (int i = 0; i < roomLengths[1]; i++)
            {
                for (int j = 0; j < roomLengths[3]; j++)
                {
                    int rOneY = roomOne[roomLengths[0] - 1, i].GridPosition.y;
                    int rTwoY = roomTwo[roomLengths[2] - 1, j].GridPosition.y;

                    if (rOneY == rTwoY)
                        possiblePositions.Add(rOneY);
                }
            }

            int rOneX = roomOne[roomLengths[0] - 1, 0].GridPosition.x;
            int rTwoX = roomTwo[0, 0].GridPosition.x;
            int chosenY = possiblePositions[Random.Range(0, possiblePositions.Count)];
            PlaceCorridor(rOneX + 1, rTwoX, chosenY, chosenY + 1);
        }

        private void PlaceStraightYCorridor(Tile[,] roomOne, Tile[,] roomTwo, int[] roomLengths)
        {
            List<int> possiblePositions = new List<int>();

            for (int i = 0; i < roomLengths[0]; i++)
            {
                for (int j = 0; j < roomLengths[2]; j++)
                {
                    int rOneX = roomOne[i, roomLengths[1] - 1].GridPosition.x;
                    int rTwoX = roomTwo[j, roomLengths[3] - 1].GridPosition.x;

                    if (rOneX == rTwoX)
                        possiblePositions.Add(rOneX);
                }
            }

            int rOneY = roomOne[0, roomLengths[1] - 1].GridPosition.y;
            int rTwoY = roomTwo[0, 0].GridPosition.y;
            int chosenX = possiblePositions[Random.Range(0, possiblePositions.Count)];
            PlaceCorridor(chosenX, chosenX + 1, rOneY + 1, rTwoY);
        }

        private void PlaceBentCorridor(Tile[,] roomOne, Tile[,] roomTwo, int[] roomLengths)
        {
            Tile randTileOne = roomOne[Random.Range(0, roomLengths[0] - 1), Random.Range(0, roomLengths[1] - 1)];
            Tile randTileTwo = roomTwo[Random.Range(0, roomLengths[2] - 1), Random.Range(0, roomLengths[3] - 1)];

            int rOneX = randTileOne.GridPosition.x;
            int rOneY = randTileOne.GridPosition.y;
            int rTwoX = randTileTwo.GridPosition.x;
            int rTwoY = randTileTwo.GridPosition.y;

            if (rOneX < rTwoX)
            {
                PlaceCorridor(rOneX, rTwoX + 1, rOneY, rOneY + 1);

                if (rOneY < rTwoY)
                    PlaceCorridor(rTwoX, rTwoX + 1, rOneY, rTwoY);
                else
                    PlaceCorridor(rTwoX, rTwoX + 1, rTwoY, rOneY);
            }

            else
            {
                PlaceCorridor(rTwoX, rOneX + 1, rOneY, rOneY + 1);

                if (rOneY < rTwoY)
                    PlaceCorridor(rTwoX, rTwoX + 1, rOneY, rTwoY);
                else
                    PlaceCorridor(rTwoX, rTwoX + 1, rTwoY, rOneY);
            }
        }

        private void PlaceZCorridor(Tile[,] roomOne, Tile[,] roomTwo, int[] roomLengths)
        {
            Tile randTileOne = roomOne[Random.Range(0, roomLengths[0] - 1), Random.Range(0, roomLengths[1] - 1)];
            Tile randTileTwo = roomTwo[Random.Range(0, roomLengths[2] - 1), Random.Range(0, roomLengths[3] - 1)];

            int rOneX = randTileOne.GridPosition.x;
            int rOneY = randTileOne.GridPosition.y;
            int rTwoX = randTileTwo.GridPosition.x;
            int rTwoY = randTileTwo.GridPosition.y;

            if (rOneX < rTwoX)
            {
                int rNewX = GetHalfway(rTwoX, rOneX);
                PlaceCorridor(rOneX, rNewX + 1, rOneY, rOneY + 1);
                PlaceCorridor(rNewX, rTwoX, rTwoY, rTwoY + 1);

                if (rOneY < rTwoY)
                    PlaceCorridor(rNewX, rNewX + 1, rOneY, rTwoY);
                else
                    PlaceCorridor(rNewX, rNewX + 1, rTwoY, rOneY);
            }

            else
            {
                int rNewX = GetHalfway(rOneX, rTwoX);
                PlaceCorridor(rTwoX, rNewX + 1, rTwoY, rTwoY + 1);
                PlaceCorridor(rNewX, rOneX, rOneY, rOneY + 1);

                if (rOneY < rTwoY)
                    PlaceCorridor(rNewX, rNewX + 1, rOneY, rTwoY);
                else
                    PlaceCorridor(rNewX, rNewX + 1, rTwoY, rOneY);
            }
        }

        private void PlaceCorridor(int minX, int maxX, int minY, int maxY)
        {
            var corridor = Grid.GetSection(m_grid.Tiles, minX, maxX, minY, maxY);
            Grid.SetSection(corridor, Tile.Types.Floor);
        }

        private int GetHalfway(int i, int j)
        {
            float k = (float)i + (float)j;
            k = k / 2.0f;
            return (int)k;
        }

        private int[] GetRoomLengths(Tile[,] roomOne, Tile[,] roomTwo)
        {
            int[] lengths = new int[4];
            lengths[0] = roomOne.GetLength(0);
            lengths[1] = roomOne.GetLength(1);
            lengths[2] = roomTwo.GetLength(0);
            lengths[3] = roomTwo.GetLength(1);

            return lengths;
        }

        private bool CheckFacing(float minDimOne, float maxDimOne, float minDimTwo, float maxDimTwo)
        {
            if (CheckIfBetween(minDimOne, minDimTwo, maxDimTwo) || CheckIfBetween(minDimTwo, minDimOne, maxDimOne))
                return true;
            else
                return false;
        }

        private bool CheckIfBetween(float check, float min, float max)
        {
            if (check > min && check < max)
                return true;
            else
                return false;
        }
    }
}