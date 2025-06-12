using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using NUnit.Framework.Constraints;
using System.Linq;

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
        iterations = 0;
        GenerateWithBacktracking(occupied, doors, 1);
    }


    bool GenerateWithBacktracking(List<Vector2Int> occupied, List<Door> doors, int depth)
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
            LogDoors(rooms[room]);
            RoomFits(rooms[room], newRoomPos, occupied);
            var doorList = rooms[room].GetDoors(); // gets all the doors in the room
            for (var door = 0; door < doorList.Count; door++) { //loops through all the doors
                if (doorList[door].IsMatching(currentDoor.GetMatching())) //if the door matches our currentDoor in that it pairs with it (east-west)
                {
                    available.Add(rooms[room]); // add the room to the available array
                    break;
                }
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


        // If you run out of doors and rooms to try, return false

        return false;
    }

    bool RoomFits(Room room, Vector2Int location, List<Vector2Int> occupied)
    {
        //Debug.Log("Checking for fit");
        List<Door> doors = room.GetDoors();
        foreach (Door d in doors) 
        {
            Vector2Int adj = d.GetMatching().GetGridCoordinates() + location;
            //Debug.Log(adj);
            if (occupied.Contains(adj)) 
            {
                Debug.Log("There is a room to the " + d.GetDirection().ToString());
            }
            else
            {
                Debug.Log("There is NO room to the " + d.GetDirection().ToString());
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
