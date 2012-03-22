using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class Fence : StaticModel
    {
        public Fence(ContentManager content, Vector3 position)
            : base(content.Load<Model>(@"Models\fence"), position)
        {
            Initialize();
        }

        private void Initialize()
        {

            Scale = Matrix.CreateScale(3.0f);
        }
    }
}
