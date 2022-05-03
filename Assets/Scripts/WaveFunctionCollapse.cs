using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WFC
{
    public class WaveFunctionCollapse : MonoBehaviour
    {
        [SerializeField] private GameObject displayModulePrefab;
        [SerializeField] Transform modulesParent;
        [SerializeField, Range(0,1)] float timeStep = 0.1f;

        static readonly int xGridSize = 10;
        static readonly int yGridSize = 10;
        
        struct Slot
        {
            public bool collapsed;
            public Prototype prototype;
            public List<Prototype> possibilities;
        }

        Slot[,] slots = new Slot[xGridSize,yGridSize];

        List<Prototype> allPrototypes;

        IEnumerator Start()
        {
            // Parse Prototypes.json
            string json = File.ReadAllText("Assets/Resources/Prototypes.json");
            allPrototypes = JsonUtility.FromJson<Prototypes>(json).list;
            InitializeAllSlots();

            while(!IsCollapsed())
            {
                Iterate();
                if(timeStep != 0)
                    yield return new WaitForSeconds(timeStep);
            }
        }

        void InitializeAllSlots()
        {
            for(int x = 0; x < xGridSize; x++)
            {
                for(int y = 0; y < yGridSize; y++)
                {
                    slots[x,y] = new Slot();
                    slots[x,y].collapsed = false;
                    slots[x,y].possibilities = new List<Prototype>();
                    slots[x,y].possibilities.AddRange(allPrototypes);
                }
            }
        }

        bool IsCollapsed()
        {
            foreach(Slot s in slots)
            {
                if(!s.collapsed)
                    return false;
            }
            return true;
        }

        void Iterate()
        {
            Vector2Int coords = GetMinEntropy();
            CollapseAt(coords);
            Propagate(coords);
        }

        Vector2Int GetMinEntropy()
        {
            // Get the lowest entropy
            int min = int.MaxValue;

            for(int x = 0; x < xGridSize; x++)
            {
                for(int y = 0; y < yGridSize; y++)
                {
                    if(slots[x,y].collapsed)
                        continue;

                    if(slots[x,y].possibilities.Count < min)
                    {
                        min = slots[x,y].possibilities.Count;
                    }
                }
            }

            // Find all slots with the lowest entropy
            List<Vector2Int> minCoordsList = new List<Vector2Int>();
            for(int x = 0; x < xGridSize; x++)
            {
                for(int y = 0; y < yGridSize; y++)
                {
                    if(slots[x,y].collapsed)
                        continue;

                    if(slots[x,y].possibilities.Count == min)
                    {
                        minCoordsList.Add(new Vector2Int(x,y));
                    }
                }
            }

            // Pick a random one
            int index = Random.Range(0, minCoordsList.Count);
            return minCoordsList[index];
        }

        void CollapseAt(Vector2Int coords)
        {
            // Choose a random possibility from the slot possibilities using a weighted random

            List<Prototype> possibilities = slots[coords.x,coords.y].possibilities;

            float sumOfProbabilities = 0;
            foreach(Prototype p in possibilities)
                sumOfProbabilities += p.probability;

            float random = Random.Range(0, sumOfProbabilities);
            float current = 0;
            Prototype chosen = possibilities[0];
            Debug.Log("Chosen : " + chosen.mesh_name);
            foreach(Prototype p in possibilities)
            {
                current += p.probability;
                if(current >= random)
                {
                    chosen = p;
                    break;
                }
            }
            Debug.Log("Chosen after : " + chosen.mesh_name);

            // Apply the collapse
            slots[coords.x, coords.y].possibilities.Clear();
            slots[coords.x, coords.y].possibilities.Add(chosen);
            slots[coords.x, coords.y].collapsed = true;
            slots[coords.x, coords.y].prototype = chosen;

            // TODO : display the prototype in the slot

            GameObject module = Instantiate(displayModulePrefab, modulesParent);
            module.transform.position = new Vector3(coords.x, 0, coords.y);
            DebugDisplayModule ddm = module.GetComponent<DebugDisplayModule>();
            ddm.Init(chosen);
        }

        void Propagate(Vector2Int coords)
        {
            // Thanks to https://www.youtube.com/watch?v=2SuvO4Gi7uY

            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            stack.Push(coords);

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Pop();

                foreach (Vector2Int dir in ValidDirections(current))
                {
                    Vector2Int otherCoords = current + dir;
                    List<Prototype> possibilitiesForOtherCoords = new List<Prototype>(slots[otherCoords.x, otherCoords.y].possibilities); // Deep copy

                    List<Prototype> possibleNeighboursOfCurrent = PossibleNeighboursInDirection(current, dir);

                    if (possibleNeighboursOfCurrent.Count == 0)
                        continue;

                    // For each possibility in the slot right next to the current slot, in the direction dir,
                    // we check if is possible according to the superposition of the current cell
                    // if there is a modification in the superposition of the other cell, we add it to the stack
                    // So that it can be propagated
                    foreach (Prototype possiblePrototype in possibilitiesForOtherCoords)
                    {
                        if (!possibleNeighboursOfCurrent.Contains(possiblePrototype))
                        {
                            bool result = slots[otherCoords.x, otherCoords.y].possibilities.Remove(possiblePrototype);

                            if (!stack.Contains(otherCoords))
                                stack.Push(otherCoords);
                        }
                    }
                }
            }
        }

        List<Prototype> PossibleNeighboursInDirection(Vector2Int coord, Vector2Int dir)
        {
            HashSet<int> validNeighboursIndices = new HashSet<int>();

            Debug.Log("Number of possibilities : " + slots[coord.x, coord.y].possibilities.Count);
            foreach(Prototype p in slots[coord.x, coord.y].possibilities)
            {
                List<int> neighboursIndices;
                if(dir == Vector2Int.up)
                    neighboursIndices = p.valid_neighbours.north;
                else if(dir == Vector2Int.right)
                    neighboursIndices = p.valid_neighbours.east;
                else if(dir == Vector2Int.down)
                    neighboursIndices = p.valid_neighbours.south;
                else if(dir == Vector2Int.left)
                    neighboursIndices = p.valid_neighbours.west;
                else
                    throw new System.Exception("Invalid direction");
                
                Debug.Log("Neighbours indices : " + neighboursIndices.Count);
                validNeighboursIndices.UnionWith(neighboursIndices);
            }

            List<Prototype> validNeighbours = new List<Prototype>();

            foreach(int i in validNeighboursIndices)
            {
                validNeighbours.Add(allPrototypes[i]);
            }

            return validNeighbours;
        }

        List<Vector2Int> ValidDirections(Vector2Int coords)
        {
            List<Vector2Int> validDirections = new List<Vector2Int>();

            if(coords.x > 0)
                validDirections.Add(new Vector2Int(-1,0));
            if(coords.x < xGridSize - 1)
                validDirections.Add(new Vector2Int(1,0));
            if(coords.y > 0)
                validDirections.Add(new Vector2Int(0,-1));
            if(coords.y < yGridSize - 1)
                validDirections.Add(new Vector2Int(0,1));

            return validDirections;
        }
    }
}