using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class GreenTree : StaticModel
    {
        public GreenTree(ContentManager content, Vector3 position)
            : base(content.Load<Model>(@"Models\GreenTree"), position)
        {
            Initialize();
        }

        private void Initialize()
        {
            Scale = Matrix.CreateScale(2.5f);
            Selectable = true;
            SelectionCircleRadiusVariable = 4;
            TopInfoOffset = new Vector3(0, 10, 0);
        }
        public override string[] OnScreenInfo
        {
            get
            {
                string[] info = new string[1];
                info[0] = "A Green Tree";
                return info;
            }
        }

        public override string ModelTopInfo
        {
            get
            {
                return "Nature Object";
            }
        }
    }
}
