using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class Path : StaticModel
    {
        private int randomNumber;

        public Path(ContentManager content, Vector3 position, int stoneNumber)
            : base(content.Load<Model>(@"Models\PathStones\stone" + stoneNumber), position)
        {
            this.randomNumber = stoneNumber;
            Init();
        }

        private void Init()
        {
            Rotation = Matrix.CreateRotationY(new Random().Next(randomNumber));
            Scale = Matrix.CreateScale(.003f);
        }
    }
}
