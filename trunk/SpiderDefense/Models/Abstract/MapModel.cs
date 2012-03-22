using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    abstract class MapModel
    {
        private Model model;
        private Vector3 position;
        private Matrix scale = Matrix.Identity;
        private Matrix rotation = Matrix.Identity;
        private float distanceFromCamera;
        private bool selected;
        private bool selectable;
        private bool drawSelectionCircle = true;
        private Matrix[] absoluteBoneTransforms = null;
        private SelectionCircleAttributes selCircle;
        private float selectionCircleRadiusVariable = 1.0f;
        private SpriteFont font;
        private Vector3 topInfoOffset;
        private Vector3 twodProjection;
        private string[] description;

        //a jagged array that could contain any number of circles, allowing for creating different circle thickness
        //or decoration effect
        private VertexPositionColor[][] circlePoints;
        //a jagged array, containing the indices for each circle
        private short[][] circleIndices;

        #region Properties
        public VertexPositionColor[][] CirclePoints
        {
            get { return circlePoints; }
            set { circlePoints = value; }
        }

        public short[][] CircleIndices
        {
            get { return circleIndices; }
            set { circleIndices = value; }
        }

        public Model Model
        {
            get { return model; }
            set { model = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Matrix Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public Matrix Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public float DistanceFromCamera
        {
            get { return distanceFromCamera; }
            set { distanceFromCamera = value; }
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (value)
                    CreateSelectionCircle(selCircle);
                selected = value;
            }
        }

        public bool Selectable
        {
            get { return selectable; }
            set { selectable = value; }
        }

        public bool DrawSelectionCircle
        {
            get { return drawSelectionCircle; }
            set { drawSelectionCircle = value; }
        }

        public Matrix[] AbsoluteBoneTransforms
        {
            get { return absoluteBoneTransforms; }
            set { absoluteBoneTransforms = value; }
        }

        public SelectionCircleAttributes SelectionCircleAttributes
        {
            get { return selCircle; }
            set { selCircle = value; }
        }

        public float SelectionCircleRadiusVariable
        {
            get { return selectionCircleRadiusVariable; }
            set { selectionCircleRadiusVariable = value; }
        }

        public Matrix World
        {
            get { return scale * rotation * Matrix.CreateTranslation(position); }
        }

        public SpriteFont Font
        {
            get { return font; }
            set { font = value; }
        }

        public Vector3 TopInfoOffset
        {
            get { return topInfoOffset; }
            set { topInfoOffset = value; }
        }

        //a dummy property, which is to be overriden by the child classes
        public virtual string[] OnScreenInfo
        {
            get { return new string[1] { " " }; }
        }

        public virtual string ModelTopInfo
        {
            get { return ""; }
        }

        public Vector3 TwoDProjection
        {
            get { return twodProjection; }
            set { twodProjection = value; }
        }

        public string[] Description
        {
            get { return description; }
            set { description = value; }
        }

        #endregion

        public MapModel(Model model, Vector3 position)
        {
            this.model = model;
            this.position = position;
        }

        public MapModel(Model model)
        {
            this.model = model;
        }

        public void CreateSelectionCircle(SelectionCircleAttributes selCircle)
        {
            if (selCircle != null)
            {
                this.selCircle = selCircle;
                float mapWidth = selCircle.MapWidth;
                float mapHeight = selCircle.MapHeight;
                float[,] heightValues = selCircle.HeightValues;
                int linesCount = selCircle.LinesCount;
                int pointsCount = selCircle.PointsCount;
                float radius = selCircle.Radius * selectionCircleRadiusVariable;
                for (int t = 0; t < linesCount; t++)
                {
                    VertexPositionColor[] circlePoints = new VertexPositionColor[pointsCount];
                    double angle = MathHelper.ToRadians(360 / pointsCount);
                    for (int i = 0; i < pointsCount; i++)
                    {
                        float x = (float)(radius * (Math.Cos(i * angle))) + position.X;
                        float z = (float)(radius * (Math.Sin(i * angle))) + position.Z;

                        //in case the model is on map's boundaries
                        x = MathHelper.Clamp(x, 0, mapWidth - 1);
                        z = MathHelper.Clamp(z, -(mapHeight - 1), 0);

                        float y = heightValues[(int)x, (int)-z];
                        circlePoints[i].Position = new Vector3(x, y+.3f, z);
                        circlePoints[i].Color = Color.White;
                    }

                    short[] circleIndices = new short[pointsCount + 1];

                    for (int i = 0; i < pointsCount + 1; i++)
                    {
                        if (i == pointsCount)
                            circleIndices[i] = circleIndices[0];
                        else
                            circleIndices[i] = (short)i;
                    }

                    if (t == 0)
                    {
                        CirclePoints = new VertexPositionColor[linesCount][];
                        CircleIndices = new short[linesCount][];
                    }

                    CirclePoints[t] = circlePoints;
                    CircleIndices[t] = circleIndices;

                    //the distance between the lines
                    radius += .1f;
                }
            }
        }

        #region Selecting Unit With Single Click

        public bool RayIntersectsModel(Ray ray, Matrix worldTransform, Matrix[] absoluteBoneTransforms)
        {
            if (absoluteBoneTransforms != null)
            {
                // Each ModelMesh in a Model has a bounding sphere, so to check for an
                // intersection in the Model, we have to check every mesh.
                foreach (ModelMesh mesh in Model.Meshes)
                {
                    // the mesh's BoundingSphere is stored relative to the mesh itself.
                    // (Mesh space). We want to get this BoundingSphere in terms of world
                    // coordinates. To do this, we calculate a matrix that will transform
                    // from coordinates from mesh space into world space....
                    Matrix world =
                        absoluteBoneTransforms[mesh.ParentBone.Index] * Scale * Rotation * worldTransform;

                    // ... and then transform the BoundingSphere using that matrix.
                    BoundingSphere sphere = TransformBoundingSphere(mesh.BoundingSphere, world);

                    // now that the we have a sphere in world coordinates, we can just use
                    // the BoundingSphere class's Intersects function. Intersects returns a
                    // nullable float (float?). This value is the distance at which the ray
                    // intersects the BoundingSphere, or null if there is no intersection.
                    // so, if the value is not null, we have a collision.

                    if (sphere.Intersects(ray) != null)
                    {
                        return true;
                    }
                }
            }

            // if we've gotten this far, we've made it through every BoundingSphere, and
            // none of them intersected the ray. This means that there was no collision,
            // and we should return false.
            return false;
        }

        private static BoundingSphere TransformBoundingSphere(BoundingSphere sphere,
            Matrix transform)
        {
            BoundingSphere transformedSphere;

            // the transform can contain different scales on the x, y, and z components.
            // this has the effect of stretching and squishing our bounding sphere along
            // different axes. Obviously, this is no good: a bounding sphere has to be a
            // SPHERE. so, the transformed sphere's radius must be the maximum of the 
            // scaled x, y, and z radii.

            // to calculate how the transform matrix will affect the x, y, and z
            // components of the sphere, we'll create a vector3 with x y and z equal
            // to the sphere's radius...
            Vector3 scale3 = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);

            // then transform that vector using the transform matrix. we use
            // TransformNormal because we don't want to take translation into account.
            scale3 = Vector3.TransformNormal(scale3, transform);

            // scale3 contains the x, y, and z radii of a squished and stretched sphere.
            // we'll set the finished sphere's radius to the maximum of the x y and z
            // radii, creating a sphere that is large enough to contain the original 
            // squished sphere.
            transformedSphere.Radius = Math.Max(scale3.X, Math.Max(scale3.Y, scale3.Z));

            // transforming the center of the sphere is much easier. we can just use 
            // Vector3.Transform to transform the center vector. notice that we're using
            // Transform instead of TransformNormal because in this case we DO want to 
            // take translation into account.
            transformedSphere.Center = Vector3.Transform(sphere.Center, transform);


            return transformedSphere;
        }

        public bool ToBeSelected(MouseClass myMouse, Camera camera, GraphicsDeviceManager graphics)
        {
            Ray ray = myMouse.CalculateCursorRay(camera.Projection, camera.View, graphics);
            if (RayIntersectsModel(
                ray,
               Matrix.CreateTranslation(Position),
                AbsoluteBoneTransforms) && Selectable)
            {
                return true;
            }

            else
                return false;
        }
        #endregion

        //checks if the current model intersects with the passed model
        public bool IntersectsWithModel(MapModel mapModelPassed)
        {
            Matrix worldTransformPassed = Matrix.CreateTranslation(mapModelPassed.Position);
            Matrix worldTransformCurrent = Matrix.CreateTranslation(Position);

            //iterate through the passed model meshes
            foreach (ModelMesh meshPassed in mapModelPassed.Model.Meshes)
            {
                if (mapModelPassed.AbsoluteBoneTransforms != null && absoluteBoneTransforms != null)
                {
                    Matrix world =
                        mapModelPassed.AbsoluteBoneTransforms[meshPassed.ParentBone.Index] *
                        mapModelPassed.Scale *
                        mapModelPassed.Rotation *
                        worldTransformPassed;

                    BoundingSphere spherePassed = TransformBoundingSphere(meshPassed.BoundingSphere, world);

                    //iterate through the current model meshes
                    foreach (ModelMesh meshCurrent in Model.Meshes)
                    {
                        Matrix worldCurrent =
                            absoluteBoneTransforms[meshCurrent.ParentBone.Index] *
                            Scale *
                            Rotation *
                            worldTransformCurrent;

                        BoundingSphere sphereCurrent = TransformBoundingSphere(meshCurrent.BoundingSphere, worldCurrent);
                        if (sphereCurrent.Intersects(spherePassed))
                            return true;
                    }
                }
            }
            return false;
        }


    }
}
