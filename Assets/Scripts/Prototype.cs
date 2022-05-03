using System.Collections.Generic;

namespace WFC
{
    [System.Serializable]
    struct Prototypes 
    {
        public List<Prototype> list;
    }

    [System.Serializable]
    public struct Prototype
    {
        // Name of the mesh in the Assets/Resources/Meshes folder
        public string mesh_name;

        // Rotation of the mesh in 90 degree increments around the Y axis
        public int mesh_rotation;

        // If the mesh is flipped along X (scaleX * -1)
        public bool mesh_is_xflipped;

        public float probability;

        // The socket id of all 4 faces
        public string north;
        public string east;
        public string south;
        public string west;

        [System.Serializable]
        public struct ValidNeighbours
        {
            public List<int> north;
            public List<int> east;
            public List<int> south;
            public List<int> west;
        }

        // List of valid neighbours for each face
        public ValidNeighbours valid_neighbours;
    }
}