using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using NUnit.Framework.Constraints;
using System.Linq;
using System.Numerics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;

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

    private Dictionary<Vector2Int, GameObject> currentRoomsDict = new Dictionary<Vector2Int, GameObject>();



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
        currentRoomsDict[new Vector2Int(0, 0)] = generated_objects[0];
        iterations = 0;
        Debug.Log("Grid before backtracking");
        Debug.Log(occupied.Count);
        GenerateWithBacktracking(occupied, doors, 1);
        Debug.Log("Grid after backtracking");
        Debug.Log(occupied.Count);
    }


    bool GenerateWithBacktracking(List<Vector2Int> occupied, List<Door> doors, int depth)
    {
        //print(generated_objects[0].GetComponent<Room>().GetDoors());

        Debug.Log("depth: " + depth);
        iterations++;
        if (iterations > THRESHOLD) throw new System.Exception("Iteration limit exceeded");
        if (occupied.Count > MAX_SIZE)
        {
            //return false;
        }
        //If there are no more doors that need to be connected check if the dungeon has the required minimum size and return true if it does, false otherwise
        if (doors.Count == 0)
        {
            if (depth > MIN_SIZE)
            {
                Debug.Log("depth exceeded max size");
                return true;
            }
            return false;
        }
        
        // Select one of the doors that still have to be connected XXXXX NOT DONE XXXXXX
        var currentDoor = doors[0];
        Vector2Int newRoomPos = currentDoor.GetMatching().GetGridCoordinates();
        //Debug.Log(newRoomPos);
        print(newRoomPos);
        // Determines which of the available rooms are compatible with this door; if there are none, return false
        List<Room> available = new List<Room>();
        for (var room = 0; room < rooms.Count; room++)
        {
            //rooms[room].HasDoorOnSide(currentDoor.GetMatching().GetDirection()) &&
            if (RoomFits(rooms[room], newRoomPos, occupied, currentRoomsDict))
            {
                available.Add(rooms[room]); // add the room to the available array
            }
        }

        // need to check if it "fits" in the current dungeon 
        Debug.Log("available count: " + available.Count);

        if (available.Count == 0)
        {
            return false;
        }



        // Tentatively place the room and recursively call GenerateWithBacktracking


        // If the recursive call returns true, you instantiate the room prefab and return true


        // If the recursive call fails, try again with another door and/or compatible room

        //instantiate room before comparing doors
        //keep track of room
        //add room to roomlist in coordinate room pair
        //get rid of room if needed
        
        Shuffle(available);
        for (var room = 0; room < available.Count; room++)
        {
            occupied.Add(newRoomPos);

            var newObject = available[room].Place(newRoomPos);//instantiate new object
            foreach (Door d in newObject.GetComponent<Room>().GetDoors(newRoomPos))
            {
                print("grid coordinates: " + d.GetGridCoordinates());
            }
            generated_objects.Add(newObject);
            currentRoomsDict[newRoomPos] = newObject;//add new object to list and dictionary

            List<Door> tempdoors = new List<Door>();
            foreach (Door d in newObject.GetComponent<Room>().GetDoors(newRoomPos))//iterate through new object's doors
            {
                print("coordinates: " + d.GetMatching().GetGridCoordinates());
                var matches = false;
                for (var door = doors.Count-1; door >= 0; door --)
                {
                    if (doors[door].IsMatching(d))
                    {
                        doors.Remove(doors[door]);
                        print("matching is true");
                        matches = true;
                    }
                }

                if (matches == false)
                {
                    tempdoors.Add(d);
                }
            }
            doors.AddRange(tempdoors);
            print(doors.Count);
            
            Debug.Log("added doors: " + tempdoors.Count);

            //doors.RemoveAt(0);
            print("doors: " + doors.Count);
            if (GenerateWithBacktracking(occupied, doors, depth + 1))
            {
                if (currentDoor.IsHorizontal())
                {
                    generated_objects.Add(horizontal_hallway.Place(currentDoor));
                }
                else {
                    generated_objects.Add(vertical_hallway.Place(currentDoor));
                }
                

                return true;
            }
            else
            {
                doors.Add(currentDoor);
                generated_objects.Remove(currentRoomsDict[newRoomPos]);
                occupied.Remove(newRoomPos);
                currentRoomsDict.Remove(newRoomPos);
                Destroy(newObject);

                foreach (Door door in tempdoors)
                {
                    doors.Remove(door);
                }

            }
        }
        
        

        // If you run out of doors and rooms to try, return false

        return false;
    }

    bool RoomFits(Room room, Vector2Int location, List<Vector2Int> occupied, Dictionary<Vector2Int, GameObject> roomDict)
    {
        //Debug.Log("Checking for fit");
        //Debug.Log(location);
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
           {new Vector2Int(0, -1), Door.Direction.NORTH},
           {new Vector2Int(0, 1), Door.Direction.SOUTH},
        };
        Dictionary<Vector2Int, Door.Direction> matchDict = new Dictionary<Vector2Int, Door.Direction>()
        {
           {new Vector2Int(1, 0), Door.Direction.EAST},
           {new Vector2Int(-1, 0), Door.Direction.WEST},
           {new Vector2Int(0, -1), Door.Direction.SOUTH},
           {new Vector2Int(0, 1), Door.Direction.NORTH},
        };
        print("fitting room");
        foreach (Vector2Int direction in directions)
        {
            var targetDoor = false;
            var roomDoor = false;
            Vector2Int targetLocation = direction + location;

            if (occupied.Contains(targetLocation))
            {

                //Debug.Log("There is a room at " + targetLocation.ToString());
                if (roomDict[targetLocation].GetComponent<Room>().HasDoorOnSide(flipDict[direction]))
                {
                    Debug.Log("there is a matching door on the " + flipDict[direction].ToString());
                    targetDoor = true;

                }
                if (room.HasDoorOnSide(matchDict[direction]))
                {
                    roomDoor = true;
                    Debug.Log("we have a door on " + matchDict[direction].ToString());
                }
                if (roomDoor != targetDoor)
                {
                    return false;
                }
                
            }
            else
            {
                //Debug.Log("There is NO room at " + targetLocation.ToString());
            }
        }
        
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

    public void Shuffle(List<Room> rooms)
    {
        var count = rooms.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = rooms[i];
            rooms[i] = rooms[r];
            rooms[r] = tmp;
        }
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
