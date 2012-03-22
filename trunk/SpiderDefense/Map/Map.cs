using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class Map
    {
        //a value indicating how high/low the hills/valleys should be - the lower the value, the higher the hills
        private float heightFactor;
        //a value, used when tiling the texture, the lower value, the smaller the texture tiles will be
        private float textureDivisionFactor;
        private Texture2D texture;
        private Texture2D heightMap;
        private List<MapModel> allMapModels = new List<MapModel>();
        private List<MapModel> pathModels = new List<MapModel>();
        private List<MapModel> fenceModels = new List<MapModel>();
        private List<Tower> towers = new List<Tower>();
        private List<MapModel> selectedModels = new List<MapModel>();
        private int mapWidth;
        private int mapHeight;
        private float[,] heightValues;
        private int[] indices;
        private VertexPositionNormalTexture[] terrainVertices;
        private Texture2D cloudsTexture;
        private Model skyDome;
        private SelectionCircleAttributes selCircle;
        //the path that will be drawn on the terrain and followed by the units
        private MapPath path;
        private ContentManager content;
        private Random rnd = new Random();

        public Map(ContentManager content, float heightFactor, float textureDivisionFactor)
        {
            this.content = content;
            this.texture = content.Load<Texture2D>(@"Textures\grass");
            this.heightMap = content.Load<Texture2D>(@"Heightmaps\height3");
            this.heightFactor = heightFactor;
            this.textureDivisionFactor = textureDivisionFactor;
            InitializeMap();
        }

        #region Properties

        public float HeightFactor
        {
            get { return heightFactor; }
            set { heightFactor = value; }
        }

        public float TextureDivisionFactor
        {
            get { return textureDivisionFactor; }
            set { textureDivisionFactor = value; }
        }

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        public Texture2D HeightMap
        {
            get { return heightMap; }
            set { heightMap = value; }
        }

        public List<MapModel> AllMapModels
        {
            //arrange the models according to their distance to the camera
            get
            {
                try { return allMapModels.OrderByDescending((m) => (m.DistanceFromCamera)).ToList(); }
                catch { }
                return allMapModels;
            }
            set { allMapModels = value; }
        }

        public List<MapModel> FenceModels
        {
            get { return fenceModels; }
        }

        public List<MapModel> SelectedModels
        {
            get { return selectedModels; }
            set { selectedModels = value; }
        }

        public int MapWidth
        {
            get { return mapWidth; }
            set { mapWidth = value; }
        }

        public int MapHeight
        {
            get { return mapHeight; }
            set { mapHeight = value; }
        }

        public float[,] HeightValues
        {
            get { return heightValues; }
            set { heightValues = value; }
        }

        public MapPath Path
        {
            get { return path; }
            set { path = value; }
        }

        public float HighestPoint
        {
            get
            {
                float max = float.MinValue;
                foreach (float val in heightValues)
                    if (val > max)
                        max = val;
                return max;
            }
        }

        public float LowestPoint
        {
            get
            {
                float min = float.MaxValue;
                foreach (float val in heightValues)
                    if (val < min)
                        min = val;
                return min;
            }
        }

        public int[] Indices
        {
            get { return indices; }
            set { indices = value; }
        }

        public VertexPositionNormalTexture[] TerrainVertices
        {
            get { return terrainVertices; }
            set { terrainVertices = value; }
        }

        public Model SkyDome
        {
            get { return skyDome; }
            set { skyDome = value; }
        }

        public Texture2D CloudsTexture
        {
            get { return cloudsTexture; }
            set { cloudsTexture = value; }
        }

        public List<MapModel> PathModels
        {
            get { return pathModels; }
            set { pathModels = value; }
        }

        public SelectionCircleAttributes SelCircleAttr
        {
            get { return selCircle; }
        }

        public List<Tower> Towers
        {
            get { return towers; }
        }

        #endregion

        public void AddModel(MapModel mapModel)
        {
            //setting the selection circle attributes
            mapModel.SelectionCircleAttributes = selCircle;

            if (mapModel is Tower)
            {
                towers.Add((Tower)mapModel);
                foreach (Arrow arrow in ((Tower)mapModel).Arrows)
                    AddModel(arrow);
            }

            allMapModels.Add(mapModel);
        }

        public void InitializeMap()
        {
            LoadHeightData(HeightMap);
            SetUpTerrain();
            SetUpIndices();
            CalculateNormals();
            selCircle = new SelectionCircleAttributes(mapWidth, mapHeight, heightValues);
            CreatePath();
            //CreateFence();
            CreateTowers();
            CreateTrees();
        }

        private void LoadHeightData(Texture2D heightMap)
        {
            MapWidth = heightMap.Width;
            MapHeight = heightMap.Height;
            HeightValues = new float[MapWidth, MapHeight];
            Color[] heightMapColors = new Color[MapWidth * MapHeight];
            heightMap.GetData(heightMapColors);

            for (int x = 0; x < MapWidth; x++)
            {
                for (int z = 0; z < MapHeight; z++)
                {
                    HeightValues[x, z] = heightMapColors[x + z * MapWidth].R / HeightFactor;
                }
            }
        }

        private void SetUpTerrain()
        {
            TerrainVertices = new VertexPositionNormalTexture[MapWidth * MapHeight];

            for (int x = 0; x < MapWidth; x++)
            {
                for (int z = 0; z < MapHeight; z++)
                {
                    float height = HeightValues[x, z];
                    TerrainVertices[x + z * MapWidth].Position = new Vector3(x, height, -z);
                    TerrainVertices[x + z * MapWidth].TextureCoordinate.X = (float)x / TextureDivisionFactor;
                    TerrainVertices[x + z * MapWidth].TextureCoordinate.Y = (float)z / TextureDivisionFactor;
                }
            }
        }

        private void SetUpIndices()
        {
            Indices = new int[(MapWidth - 1) * (MapHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < MapHeight - 1; y++)
            {
                for (int x = 0; x < MapWidth - 1; x++)
                {
                    int lowerLeft = x + y * MapWidth;
                    int lowerRight = (x + 1) + y * MapWidth;
                    int topLeft = x + (y + 1) * MapWidth;
                    int topRight = (x + 1) + (y + 1) * MapWidth;

                    Indices[counter++] = topLeft;
                    Indices[counter++] = lowerRight;
                    Indices[counter++] = lowerLeft;

                    Indices[counter++] = topLeft;
                    Indices[counter++] = topRight;
                    Indices[counter++] = lowerRight;
                }
            }
        }

        private void CalculateNormals()
        {
            for (int i = 0; i < TerrainVertices.Length; i++)
                TerrainVertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < Indices.Length / 3; i++)
            {
                int index1 = Indices[i * 3];
                int index2 = Indices[i * 3 + 1];
                int index3 = Indices[i * 3 + 2];

                Vector3 side1 = TerrainVertices[index1].Position - TerrainVertices[index3].Position;
                Vector3 side2 = TerrainVertices[index1].Position - TerrainVertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                TerrainVertices[index1].Normal += normal;
                TerrainVertices[index2].Normal += normal;
                TerrainVertices[index3].Normal += normal;
            }
            for (int i = 0; i < TerrainVertices.Length; i++)
                TerrainVertices[i].Normal.Normalize();
        }

        private void CreateFence()
        {
            for (int i = 0; i < mapWidth; i++)
            {
                if (i % 5 == 0)
                {

                    Fence fence1 = new Fence(content,
                                   new Vector3(i + 2, heightValues[i + 2, 0], 0.0f));

                    Fence fence2 = new Fence(content,
                                    new Vector3(i + 2, heightValues[i, -(-mapHeight + 1)], -mapHeight + 1));

                    fenceModels.Add(fence1);
                    fenceModels.Add(fence2);
                }
            }
            for (int i = 0; i < mapHeight; i++)
            {
                if (i % 5 == 0)
                {
                    Fence fence1 = new Fence(content,
                                   new Vector3(0.0f, heightValues[0, -(-i - 2)], -i - 2));
                    fence1.Rotation = Matrix.CreateRotationY(MathHelper.PiOver2);

                    Fence fence2 = new Fence(content,
                                   new Vector3(mapWidth - 1, heightValues[mapWidth - 1, -(-i - 2)], -i - 2));
                    fence2.Rotation = Matrix.CreateRotationY(MathHelper.PiOver2);

                    fenceModels.Add(fence1);
                    fenceModels.Add(fence2);
                }
            }
        }

        private void CreatePath()
        {
            path = new MapPath(heightValues);
            GeneratePathModels();
        }

        private void GeneratePathModels()
        {
            float distanceBetweenTiles = 12;
            bool movingOnX = false;
            bool movingOnZ = false;
            for (int i = 0; i < path.Checkpoints.Count; i++)
            {
                PathCheckpoint currentCheckpoint = path.Checkpoints[i];
                PathCheckpoint nextCheckpoint = null;
                if (i + 1 < path.Checkpoints.Count)
                    nextCheckpoint = path.Checkpoints[i + 1];

                Vector3 pos = currentCheckpoint.Position;
                if (i == 0)
                    pos.Z -= distanceBetweenTiles / 2;
                if (i + 1 == path.Checkpoints.Count)
                    pos.Z += distanceBetweenTiles / 2;

                AddTile(pos);

                if (nextCheckpoint == null)
                    return;
                else if (nextCheckpoint != null)
                {
                    float difference1 = nextCheckpoint.Position.X - currentCheckpoint.Position.X;
                    if (difference1 != 0)
                        movingOnX = true;
                    else
                        movingOnX = false;
                    float difference2 = nextCheckpoint.Position.Z - currentCheckpoint.Position.Z;
                    if (difference2 != 0)
                        movingOnZ = true;
                    else
                        movingOnZ = false;

                    if (movingOnX)
                    {
                        Matrix rot = Matrix.CreateRotationY(90);
                        if (difference1 > 0)
                        {
                            pos.X += distanceBetweenTiles;
                            while (pos.X < nextCheckpoint.Position.X)
                            {
                                AddTile(new Vector3(pos.X, heightValues[(int)pos.X, -(int)pos.Z], pos.Z));
                                pos.X += distanceBetweenTiles;
                            }
                        }
                        else if (difference1 < 0)
                        {
                            pos.X -= distanceBetweenTiles;
                            while (pos.X > nextCheckpoint.Position.X)
                            {
                                AddTile(new Vector3(pos.X, heightValues[(int)pos.X, -(int)pos.Z], pos.Z));
                                pos.X -= distanceBetweenTiles;
                            }
                        }

                    }
                    if (movingOnZ)
                    {
                        if (difference2 < 0)
                        {
                            pos.Z -= distanceBetweenTiles;
                            while (pos.Z > nextCheckpoint.Position.Z)
                            {
                                AddTile(new Vector3(pos.X, heightValues[(int)pos.X, -(int)pos.Z], pos.Z));
                                pos.Z -= distanceBetweenTiles;
                            }
                        }
                        else if (difference2 > 0)
                        {
                            pos.Z += distanceBetweenTiles;
                            while (pos.Z < nextCheckpoint.Position.Z)
                            {
                                AddTile(new Vector3(pos.X, heightValues[(int)pos.X, -(int)pos.Z], pos.Z));
                                pos.Z += distanceBetweenTiles;
                            }
                        }
                    }
                }
            }
        }

        private void AddTile(Vector3 position)
        {
            position = new Vector3(position.X, position.Y + .15f, position.Z);
            pathModels.Add(new Path(content, position, rnd.Next(1, 6)));
        }

        private void CreateTowers()
        {
            //the road offset - at what distance from the road the towers will be placed
            int offset = 15;

            Vector3 tower1Position = new Vector3(mapWidth / 3 + offset,
                heightValues[(int)(mapWidth / 3 + offset), -(int)(Path.Checkpoints[0].Position.Z - Vector3.Distance(Path.Checkpoints[0].Position, Path.Checkpoints[1].Position) / 3)],
                Path.Checkpoints[0].Position.Z - Vector3.Distance(Path.Checkpoints[1].Position, Path.Checkpoints[0].Position) / 3);

            Vector3 tower2Position = new Vector3(Path.Checkpoints[1].Position.X - offset,
                heightValues[(int)(Path.Checkpoints[1].Position.X - offset), -(int)(Path.Checkpoints[1].Position.Z - offset)],
                Path.Checkpoints[1].Position.Z - offset);

            Vector3 tower3Position = new Vector3(Path.Checkpoints[3].Position.X - offset,
                heightValues[(int)(Path.Checkpoints[3].Position.X - offset), -(int)(Path.Checkpoints[3].Position.Z + offset)],
                Path.Checkpoints[3].Position.Z + offset);

            Vector3 tower4Position = new Vector3(Path.Checkpoints[4].Position.X + offset,
               heightValues[(int)(Path.Checkpoints[4].Position.X + offset), -(int)(Path.Checkpoints[4].Position.Z - offset)],
               Path.Checkpoints[4].Position.Z - offset);

            Vector3 tower5Position = new Vector3(Path.Checkpoints[6].Position.X - offset,
                heightValues[(int)(Path.Checkpoints[6].Position.X - offset), -(int)(Path.Checkpoints[6].Position.Z - offset)],
                Path.Checkpoints[6].Position.Z - offset);

            Tower tower1 = new Tower(content, tower1Position, 1);
            Tower tower2 = new Tower(content, tower2Position, 0);
            Tower tower3 = new Tower(content, tower3Position, 2);
            Tower tower4 = new Tower(content, tower4Position, 3);
            Tower tower5 = new Tower(content, tower5Position, 4);

            AddModel(tower1);
            //AddModel(tower2);
            AddModel(tower3);
            AddModel(tower4);
            AddModel(tower5);
        }

        private void CreateTrees()
        {
            int treeCount = 20;
            Random rnd = new Random();
            for (int i = 0; i < treeCount; i++)
            {
                float x;
                float z;
                Vector3 position = Vector3.Zero;
                do
                {
                    x = rnd.Next(MapWidth);
                    z = rnd.Next(MapHeight);
                    x = MathHelper.Clamp(x, 0, MapWidth);
                    z = MathHelper.Clamp(z, 0, MapHeight);
                    position = new Vector3(x, HeightValues[(int)x, (int)z], -z);
                }
                while (ToBeOnTopOf(position));

                OldTree oldTree = new OldTree(content, position);
                oldTree.Rotation = Matrix.CreateRotationY(rnd.Next(90));
                oldTree.Scale = Matrix.CreateScale(rnd.Next(5) / 2 + 2);
                AddModel(oldTree);
            }
        }

        private bool ToBeOnTopOf(Vector3 position)
        {
            bool intersects = false;
            float maxDistance = 3;

            //test for any model intersection
            for (int i = 0; i < AllMapModels.Count; i++)
            {
                if (Vector3.Distance(position, AllMapModels[i].Position) < maxDistance)
                    intersects = true;
            }

            //test for path intersection
            for (int i = 0; i < PathModels.Count; i++)
            {
                if (Vector3.Distance(position, PathModels[i].Position) < maxDistance + 10)
                    intersects = true;
            }
            return intersects;
        }

        public Vector3 TerrainCollision(Ray ray)
        {
            foreach (VertexPositionNormalTexture vertex in terrainVertices)
            {
                BoundingSphere sphere = new BoundingSphere(vertex.Position, 0.6f);
                if (ray.Intersects(sphere) != null)
                    return vertex.Position;
            }
            return Vector3.Zero;
        }

        internal void RemoveDead()
        {
            for (int i = 0; i < allMapModels.Count; i++)
            {
                if (allMapModels[i] is DynamicModel)
                    if (!((DynamicModel)allMapModels[i]).IsAlive)
                    {
                        if (selectedModels.Contains(allMapModels[i]))
                            selectedModels.Remove(allMapModels[i]);
                        allMapModels.RemoveAt(i);
                    }
            }
        }
    }
}