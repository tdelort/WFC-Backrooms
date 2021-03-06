using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WFC
{
    public class PrefabsBaker : MonoBehaviour
    {
        static readonly string jsonPath = "Assets/Resources/Prototypes.json";

        [SerializeField] private GameObject displayModulePrefab;

        [SerializeField] bool debugDisplayModules = false;
        [SerializeField] Transform modulesParent;


        bool _prevDebugDisplayModules = false;
        void Update()
        {
            if (debugDisplayModules != _prevDebugDisplayModules)
            {
                _prevDebugDisplayModules = debugDisplayModules;
                OnDebugDisplayModulesChanged();
            }
        }

        void OnDebugDisplayModulesChanged()
        {
            Debug.Log("DebugDisplayModules changed to " + debugDisplayModules);

            // Parse Prototypes.json
            string json = File.ReadAllText(jsonPath);
            Prototypes prototypes = JsonUtility.FromJson<Prototypes>(json);
            
            // For each prototype, instantiate a module using the prefab and give it the prototype's data
            int i = 0;
            foreach(Prototype p in prototypes.list)
            {
                GameObject module = Instantiate(displayModulePrefab, modulesParent);
                module.transform.position = new Vector3((i % 10) * 4, 0, (i / 10) * 4);
                DebugDisplayModule ddm = module.GetComponent<DebugDisplayModule>();
                ddm.Init(p);
                i++;
            }
        }

        

        // -----------------------------------------------------------------------------------------
        //                                    EDITOR PART                                           
        // -----------------------------------------------------------------------------------------


        [MenuItem("WFC/Bake JSON Modules")]
        static void BakeJSONModules()
        {
            // If not in the unity editor, do nothing
            if (!Application.isEditor)
            {
                return;
            }

            Prototypes prototypes;
            prototypes.list = new List<Prototype>();

            // For each pre bake prototype in Assets/Resources/PreBakePrototypes
            foreach(ModulePreBakePrototype mpbp in Resources.LoadAll<ModulePreBakePrototype>("PreBakePrototypes"))
            {
                // Iterate over the normal version and the flipped version
                for (int flip = 0; flip < (mpbp.mirror ? 2 : 1); flip++)
                {
                    // Iteration over the rotations
                    for (int rotation = 0; rotation < (mpbp.rotate ? 5 : 1); rotation ++)
                    {
                        Prototype proto = CreateProto(mpbp, flip, rotation);
                        prototypes.list.Add(proto);
                    }
                }
            }

            Debug.Log("Baking " + prototypes.list.Count + " prototypes");

            // TODO : 
            // For each Prototype object
            for(int i = 0; i < prototypes.list.Count; i++)
            {
                // For each face
                    // Fill the list of valid neighbours
                
                Prototype p = prototypes.list[i];

                // Init
                p.valid_neighbours.north = new List<int>();
                p.valid_neighbours.east = new List<int>();
                p.valid_neighbours.south = new List<int>();
                p.valid_neighbours.west = new List<int>();

                // Set
                for(int dir = 0; dir < 4; dir++)
                {
                    for(int j = 0; j < prototypes.list.Count; j++)
                    {
                        Prototype other = prototypes.list[j];
                        string currentSocketStr;
                        string otherSocketStr;

                        switch (dir)
                        {
                            case 0 : // current : north / other : south
                                currentSocketStr = p.north;
                                otherSocketStr = other.south;
                                break;
                            case 1 : // current : east / other : west
                                currentSocketStr = p.east;
                                otherSocketStr = other.west;
                                break;
                            case 2 : // current : south / other : north
                                currentSocketStr = p.south;
                                otherSocketStr = other.north;
                                break;
                            case 3 : // current : west / other : east
                                currentSocketStr = p.west;
                                otherSocketStr = other.east;
                                break;
                            default :
                                throw new System.Exception("Invalid direction");
                        }

                        if (ModulePreBakePrototype.IsCompatible(ModulePreBakePrototype.StringToSocket(currentSocketStr), ModulePreBakePrototype.StringToSocket(otherSocketStr)))
                        {
                            switch (dir)
                            {
                                case 0 :
                                    p.valid_neighbours.north.Add(j);
                                    break;
                                case 1 :
                                    p.valid_neighbours.east.Add(j);
                                    break;
                                case 2 :
                                    p.valid_neighbours.south.Add(j);
                                    break;
                                case 3 :
                                    p.valid_neighbours.west.Add(j);
                                    break;
                                default :
                                    throw new System.Exception("Invalid direction");
                            }
                        }
                    }
                }

                // Set
                prototypes.list[i] = p;
            }

            // Save the prototypes in Assets/Resources/Prototypes.json
            string bigJsonString = JsonUtility.ToJson(prototypes, true);
            Debug.Log(bigJsonString.Length);
            File.WriteAllText(jsonPath, bigJsonString);
            UnityEditor.AssetDatabase.Refresh();
        }       

        static Prototype CreateProto(ModulePreBakePrototype mpbp, int flip, int rotation)
        {
            // Create a Prototype object
            Prototype proto = new Prototype();

            // Fill the mesh_name, mesh_rotation, mesh_is_xflipped, probability, north, east, south, west
            proto.mesh_name = mpbp.moduleModel.name;
            proto.mesh_rotation = rotation;
            proto.mesh_is_xflipped = flip != 0;
            proto.probability = mpbp.probability;


            // Gather original sockets
            ModulePreBakePrototype.Socket[] sockets = {mpbp.North, mpbp.East, mpbp.South, mpbp.West};

            // Apply the flip
            if(flip != 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    sockets[i] = ModulePreBakePrototype.FlipSocket(sockets[i]);
                }

                // Flip socket positions along the x axis
                ModulePreBakePrototype.Socket tmp = sockets[1]; // east
                sockets[1] = sockets[3]; // west
                sockets[3] = tmp;
            }

            // Apply the rotation
            ModulePreBakePrototype.Socket[] rotatedSockets = {
                sockets[(0 - rotation + 4) % 4], // north
                sockets[(1 - rotation + 4) % 4], // east
                sockets[(2 - rotation + 4) % 4], // south
                sockets[(3 - rotation + 4) % 4]  // west
            };

            // Fill the north, east, south, west
            proto.north = ModulePreBakePrototype.SocketToString(rotatedSockets[0]);
            proto.east = ModulePreBakePrototype.SocketToString(rotatedSockets[1]);
            proto.south = ModulePreBakePrototype.SocketToString(rotatedSockets[2]);
            proto.west = ModulePreBakePrototype.SocketToString(rotatedSockets[3]);

            return proto;
        }
    }
}