using System;
using System.IO;

namespace Labs.Utility
{
    /// <summary>
    /// Model Utility reads a very simple, inefficient file format that uses 
    /// Triangles only. This is not good practice, but it does the job :)
    /// </summary>
    public class ModelUtility
    {
        public float[] Vertices { get; private set; }
        public int[] Indices { get; private set; }

        private ModelUtility() { }

        private static ModelUtility LoadFromBIN(string pModelFile)
        {
            ModelUtility model = new ModelUtility();
            BinaryReader reader = new BinaryReader(new FileStream(pModelFile, FileMode.Open));

            int numberOfVertices = reader.ReadInt32();
            int floatsPerVertex = 6;

            model.Vertices = new float[numberOfVertices * floatsPerVertex];

            byte[]  byteArray = new byte[model.Vertices.Length * sizeof(float)];
            byteArray = reader.ReadBytes(byteArray.Length);

            Buffer.BlockCopy(byteArray, 0, model.Vertices, 0, byteArray.Length);

            int numberOfTriangles = reader.ReadInt32();

            model.Indices = new int[numberOfTriangles * 3];
            
            byteArray = new byte[model.Indices.Length * sizeof(int)];
            byteArray = reader.ReadBytes(model.Indices.Length * sizeof(int));
            Buffer.BlockCopy(byteArray, 0, model.Indices, 0, byteArray.Length);

            reader.Close();
            return model;
        }     
 
        private static ModelUtility LoadFromSJG(string pModelFile)
        {
            ModelUtility model = new ModelUtility();
            StreamReader reader;
            reader = new StreamReader(pModelFile);
            string line = reader.ReadLine(); // vertex format
            int numberOfVertices = 0;
            int floatsPerVertex = 8;
            if (!int.TryParse(reader.ReadLine(), out numberOfVertices))
            {
                throw new Exception("Error when reading number of vertices in model file " + pModelFile);
            }

            model.Vertices = new float[numberOfVertices * floatsPerVertex];

            string[] values;
            for (int i = 0; i < model.Vertices.Length; )
            {
                line = reader.ReadLine();
                values = line.Split(',');
                foreach(string s in values)
                {
                    if (!float.TryParse(s, out model.Vertices[i]))
                    {
                        throw new Exception("Error when reading vertices in model file " + pModelFile + " " + s + " is not a valid number");
                    }
                    ++i;
                }
            }

            reader.ReadLine();
            int numberOfTriangles = 0;
            line = reader.ReadLine();
            if (!int.TryParse(line, out numberOfTriangles))
            {
                throw new Exception("Error when reading number of triangles in model file " + pModelFile);
            }

            model.Indices = new int[numberOfTriangles * 3];

            for(int i = 0; i < numberOfTriangles * 3;)
            {
                line = reader.ReadLine();
                values = line.Split(',');
                foreach(string s in values)
                {
                    if (!int.TryParse(s, out model.Indices[i]))
                    {
                        throw new Exception("Error when reading indices in model file " + pModelFile + " " + s + " is not a valid index");
                    }
                    ++i;
                }
            }

            reader.Close();
            return model;
        }

        public static ModelUtility LoadModel(string pModelFile)
        {
            string extension = pModelFile.Substring(pModelFile.IndexOf('.'));

            if (extension == ".sjg")
            {
                return LoadFromSJG(pModelFile);
            }
            else if (extension == ".bin")
            {
                return LoadFromBIN(pModelFile);
            }
            else
            {
                throw new Exception("Unknown file extension " + extension);
            }
        }

    }
}
