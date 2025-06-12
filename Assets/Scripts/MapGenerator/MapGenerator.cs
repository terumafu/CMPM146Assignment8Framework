using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using NUnit.Framework.Constraints;
using System.Linq;
using System.Numerics;
using UnityEditor.Experimental.GraphView;

public class MapGenerator : MonoBehaviour
{
    public List<Room> rooms;
    public Hallway vertical_hallway;
    public Hallway horizontal_hallway;
    public Room start;
    public Room target;

    // Constraint: How big should the dungeon be at most
    // this will limit the run time (~10 is a good value 
    // during development, later you'll want to set it to 
    // something a bit higher, like 25-30)
    public int MAX_SIZE;

    public int MIN_SIZE = 3;

    // set this to a high value when the generator works
    // for debugging it can be helpful to test with few rooms
    // and, say, a threshold of 100 iterations
    public int THRESHOLD;

    private Dictionary<Vector2Int, Room> currentRoomsDict = new Dictionary<Vector2Int, Room>();



    // keep the instantiated rooms and hallways here 
    private List<GameObject> generated_objects;
    
    int iterations;

    public void Generate()
    {
        // dispose of game objects from previous generation process
        foreach (var go in generated_objects)
        {
            Destroy(go);
        }
        generated_objects.Clear();
        
        generated_objects.Add(start.Place(new Vector2Int(0,0)));
        List<Door> doors = start.GetDoors();
        List<Vector2Int> occupied = new List<Vector2Int>();
        occupied.Add(new Vector2Int(0, 0));
        currentRoomsDict[new Vector2Int(0, 0)] = start;
        iterations = 0;
        Debug.Log("Grid before backtracking");
        Debug.Log(occupied.Count);
        GenerateWithBacktracking(occupied, currentRoomsDict, doors, 1);
        Debug.Log("Grid after backtracking");
        Debug.Log(occupied.Count);
    }


    bool GenerateWithBacktracking(List<Vector2Int> occupied, Dictionary<Vector2Int, Room> roomDict, List<Door> doors, int depth)
    {
        if (iterations > THRESHOLD) throw new System.Exception("Iteration limit exceeded");

        //If there are no more doors that need to be connected check if the dungeon has the required minimum size and return true if it does, false otherwise
        if (doors.Count == 0)
        {
            if (depth > MIN_SIZE)
            {
                return true;
            }
            return false;
        }

        // Select one of the doors that still have to be connected XXXXX NOT DONE XXXXXX
        var currentDoor = doors[0];
        Vector2Int newRoomPos = currentDoor.GetMatching().GetGridCoordinates();
        Debug.Log(newRoomPos);
        
        // Determines which of the available rooms are compatible with this door; if there are none, return false
        List<Room> available = new List<Room>();
        for (var room = 0; room < rooms.Count; room++)
        {
            //LogDoors(rooms[room]);
            //RoomFits(rooms[room], newRoomPos, occupied, roomDict);
            var doorList = rooms[room].GetDoors(); // gets all the doors in the room

            if (rooms[room].HasDoorOnSide(currentDoor.GetMatching().GetDirection()) && RoomFits(rooms[room], newRoomPos, occupied, roomDict))
            {
                available.Add(rooms[room]); // add the room to the available array
            }
        }

        // need to check if it "fits" in the current dungeon 


        if (available.Count == 0)
        {
            return false;
        }



        // Tentatively place the room and recursively call GenerateWithBacktracking


        // If the recursive call returns true, you instantiate the room prefab and return true


        // If the recursive call fails, try again with another door and/or compatible room
        List<Vector2Int> occupiedCopy;
        Dictionary<Vector2Int, Room> roomDictCopy; 
        for (var room = 0; room < available.Count; room++)
        {
            occupiedCopy = GridCopy(occupied);
            occupiedCopy.Add(newRoomPos);

            roomDictCopy = new Dictionary<Vector2Int, Room>(roomDict);
            roomDictCopy.Add(newRoomPos, available[room]);

            if (GenerateWithBacktracking(occupiedCopy, roomDictCopy, doors, depth + 1))
            {
                occupied = occupiedCopy;
                roomDict = roomDictCopy;
                generated_objects.Add(available[0].Place(newRoomPos));
                return true;
            }
        }
        
        

        // If you run out of doors and rooms to try, return false

        return false;
    }

    bool RoomFits(Room room, Vector2Int location, List<Vector2Int> occupied, Dictionary<Vector2Int, Room> roomDict)
    {
        //Debug.Log("Checking for fit");
        Debug.Log(location);
        List<Door> doors = room.GetDoors();
        List<Vector2Int> directions = new List<Vector2Int>()
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
           new Vector2Int(0, -1),
        };
        Dictionary<Vector2Int, Door.Direction> flipDict = new Dictionary<Vector2Int, Door.Direction>()
        {
           {new Vector2Int(1, 0), Door.Direction.WEST},
           {new Vector2Int(-1, 0), Door.Direction.EAST},
           {new Vector2Int(0, 1), Door.Direction.NORTH},
           {new Vector2Int(0, -1), Door.Direction.SOUTH},
        };
        Dictionary<Vector2Int, Door.Direction> matchDict = new Dictionary<Vector2Int, Door.Direction>()
        {
           {new Vector2Int(1, 0), Door.Direction.EAST},
           {new Vector2Int(-1, 0), Door.Direction.WEST},
           {new Vector2Int(0, 1), Door.Direction.SOUTH},
           {new Vector2Int(0, -1), Door.Direction.NORTH},
        };

        
        foreach (Vector2Int direction in directions)
        {
            var targetDoor = false;
            var roomDoor = false;
            Vector2Int targetLocation = direction + location;
            if (occupied.Contains(targetLocation))
            {

                Debug.Log("There is a room at " + targetLocation.ToString());
                if (roomDict[targetLocation].HasDoorOnSide(flipDict[direction]))
                {
                    Debug.Log("there is a door on the " + flipDict[direction].ToString());
                    targetDoor = true;

                }
                if (room.HasDoorOnSide(matchDict[direction]))
                {
                    roomDoor = true;
                    Debug.Log("yippee");
                }
                if (roomDoor == targetDoor)
                {

                }
                else
                {
                    return false;
                }
            }
            else
            {
                Debug.Log("There is NO room at " + targetLocation.ToString());
            }
        }
        // if (roomDict[adj].HasDoorOnSide(d.GetDirection()))
        //         {

        //         }
        
        return true;
    }
    void LogDoors(Room room)
    {
        List<Door> doors = room.GetDoors();
        string dString = "Room has doors in the following directions:";
        foreach (Door d in doors)
        {
            dString = dString + " " + d.GetDirection().ToString();
        }
        Debug.Log(dString);
    }

    List<Vector2Int> GridCopy(List<Vector2Int> original)
    {
        List<Vector2Int> cpy = new List<Vector2Int>();
        foreach(Vector2Int v in original)
        {
            cpy.Add(v);
        }
        return cpy;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generated_objects = new List<GameObject>();
        Generate();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
            Generate();
    }
}
