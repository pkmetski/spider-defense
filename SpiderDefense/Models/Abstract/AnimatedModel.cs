using System;
using System.Collections.Generic;
using AnimatedModelContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    abstract class AnimatedModel : DynamicModel
    {
        AnimatedModelData animatedModelData;

        AnimatedModelEffect animatedModelEffect;
        LightMaterial lightMaterial;

        bool enableAnimationLoop;
        float animationSpeed;
        int activeAnimationKeyframe;
        TimeSpan activeAnimationTime;
        AnimationData activeAnimation;

        Matrix[] bones;
        Matrix[] bonesAbsolute;
        Matrix[] bonesAnimation;
        Matrix[] bonesTransform;

        BoundingBox modelBoundingBox;
        BoundingSphere modelBoundingSphere;

        Camera camera;
        LightManager lightManager;

        public AnimatedModel(Model model, Vector3 position, Camera camera)
            : base(model, position)
        {
            this.camera = camera;
            Init();
        }

        public AnimatedModel(Model model, MapPath path, Camera camera)
            : base(model, path)
        {
            this.camera = camera;
            Init();
        }

        #region Properties
        public LightMaterial LightMaterial
        {
            get { return lightMaterial; }
            set { lightMaterial = value; }
        }

        public TimeSpan ActiveAnimationTime
        {
            get { return activeAnimationTime; }
            set { activeAnimationTime = value; }
        }

        public virtual AnimationData ActiveAnimation
        {
            get { return activeAnimation; }
            set
            {
                activeAnimation = value;
                // Reset animation
                ResetAnimation();
            }
        }

        public bool IsAnimationFinished
        {
            get { return (activeAnimationTime >= activeAnimation.Duration); }
        }

        public AnimationData[] Animations
        {
            get { return animatedModelData.Animations; }
        }

        public bool EnableAnimationLoop
        {
            get { return enableAnimationLoop; }
            set { enableAnimationLoop = value; }
        }

        public float AnimationSpeed
        {
            get { return animationSpeed; }
            set { animationSpeed = value; }
        }

        public Matrix[] BonesTransform
        {
            get { return bonesTransform; }
            set { bonesTransform = value; }
        }

        public Matrix[] BonesAbsolute
        {
            get { return bonesAbsolute; }
            set { bonesAbsolute = value; }
        }

        public Matrix[] BonesAnimation
        {
            get { return bonesAnimation; }
            set { bonesAnimation = value; }
        }

        public BoundingBox BoundingBox
        {
            get { return modelBoundingBox; }
        }

        public BoundingSphere BoundingSphere
        {
            get { return modelBoundingSphere; }
        }
        #endregion

        private void Init()
        {
            enableAnimationLoop = true;
            animationSpeed = 1.0f;
            activeAnimationKeyframe = 0;
            activeAnimationTime = TimeSpan.Zero;

            lightManager = new LightManager();
            lightManager.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
            // Light 1
            lightManager.Add("Light1", new PointLight(new Vector3(500.0f, -400.0f, -300.0f), Vector3.One));

            // Light 2
            lightManager.Add("Light2", new PointLight(new Vector3(-500.0f, 400.0f, 300.0f), Vector3.One));

            LoadAnimationData();
        }

        public void LoadAnimationData()
        {
            // Get animated model data
            Dictionary<string, object> modelTag = (Dictionary<string, object>)Model.Tag;
            if (modelTag == null)
                throw new InvalidOperationException("This is not a valid animated model.");

            // Read tag data
            animatedModelData = (AnimatedModelData)modelTag["AnimatedModelData"];
            modelBoundingBox = (BoundingBox)modelTag["ModelBoudingBox"];
            modelBoundingSphere = (BoundingSphere)modelTag["ModelBoudingSphere"];


            if (animatedModelData.Animations.Length > 0)
                activeAnimation = animatedModelData.Animations[0];

            bones = new Matrix[animatedModelData.BonesBindPose.Length];
            bonesAbsolute = new Matrix[animatedModelData.BonesBindPose.Length];
            bonesAnimation = new Matrix[animatedModelData.BonesBindPose.Length];
            bonesTransform = new Matrix[animatedModelData.BonesBindPose.Length];

            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = animatedModelData.BonesBindPose[i];
                bonesTransform[i] = Matrix.Identity;
            }

            // Get the animated model effect - Shared by all meshes
            animatedModelEffect = new AnimatedModelEffect(Model.Meshes[0].Effects[0]);

            // Create a default material
            lightMaterial = new LightMaterial();

        }

        private void ResetAnimation()
        {
            // Reset animation
            activeAnimationTime = TimeSpan.Zero;
            activeAnimationKeyframe = 0;

            for (int i = 0; i < bones.Length; i++)
                bones[i] = animatedModelData.BonesBindPose[i];
        }

        private void UpdateAnimation(GameTime time, Matrix parent)
        {
            activeAnimationTime += new TimeSpan((long)(time.ElapsedGameTime.Ticks * animationSpeed));

            if (activeAnimation != null)
            {
                // Loop the animation
                if (activeAnimationTime > activeAnimation.Duration && enableAnimationLoop)
                {
                    long elapsedTicks = activeAnimationTime.Ticks % activeAnimation.Duration.Ticks;
                    activeAnimationTime = new TimeSpan(elapsedTicks);
                    activeAnimationKeyframe = 0;

                    ResetAnimation();
                }

                // Read all animation keyframes until the current time
                // That's possible because we have sorted the keyframes by time in the model processor
                int index = 0;
                Keyframe[] keyframes = activeAnimation.Keyframes;

                while (index < keyframes.Length && keyframes[index].Time <= activeAnimationTime)
                {
                    int boneIndex = keyframes[index].Bone;
                    bones[boneIndex] = keyframes[index].Transform;
                    index++;
                }
                activeAnimationKeyframe = index - 1;
            }

            // Apply the custom transformation over all bones
            for (int i = 0; i < bones.Length; i++)
            {
                bonesAbsolute[i] = bones[i] * bonesTransform[i];
            }

            // Fill the bones with their absolute coordinate (Not relative to the parent bones)
            // Note that we don't need to worry about the hierarchy because our bone list 
            // was made using a depth-first search
            bonesAbsolute[0] = bonesAbsolute[0] * parent;
            for (int i = 1; i < bonesAnimation.Length; i++)
            {
                int boneParent = animatedModelData.BonesParent[i];

                // Here we are transforming the child bone by it's father position and orientation
                bonesAbsolute[i] = bonesAbsolute[i] * bonesAbsolute[boneParent];
            }

            // Before we could transform the vertices using the calculated bone matriz we
            // need to put the vertices in the coordinate system of the skeleton bind pose
            for (int i = 0; i < bonesAnimation.Length; i++)
            {
                bonesAnimation[i] = animatedModelData.BonesInverseBindPose[i] *
                    bonesAbsolute[i];
            }
        }

        public void Update(GameTime time)
        {
            Update(time, Matrix.Identity);
        }

        public void Update(GameTime time, Matrix world)
        {
            float elapsedTimeSeconds = (float)time.ElapsedGameTime.TotalSeconds;

            UpdateAnimation(time, world);
        }

        private void SetEffectMaterial()
        {
            // Get the first two lights from the light manager
            PointLight light0 = PointLight.NoLight;
            PointLight light1 = PointLight.NoLight;
            if (lightManager.Count > 0)
            {
                light0 = lightManager[0] as PointLight;
                if (lightManager.Count > 1)
                    light1 = lightManager[1] as PointLight;
            }

            // Lights
            animatedModelEffect.AmbientLightColor = lightManager.AmbientLightColor;
            animatedModelEffect.Light1Position = light0.Position;
            animatedModelEffect.Light1Color = light0.Color;
            animatedModelEffect.Light2Position = light1.Position;
            animatedModelEffect.Light2Color = light1.Color;

            // Configure material
            animatedModelEffect.DiffuseColor = lightMaterial.DiffuseColor;
            animatedModelEffect.SpecularColor = lightMaterial.SpecularColor;
            animatedModelEffect.SpecularPower = lightMaterial.SpecularPower;

            // Camera and world transformations
            animatedModelEffect.World = World;
            animatedModelEffect.View = camera.View;
            animatedModelEffect.Projection = camera.Projection;
            animatedModelEffect.Bones = bonesAnimation;
        }

        public void Draw(GameTime gameTime)
        {
            SetEffectMaterial();

            for (int i = 0; i < Model.Meshes.Count; i++)
            {
                Model.Meshes[i].Draw();
            }
        }

    }
}
