using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
     abstract class Building : StaticModel
    {

        public Building(Model model, Vector3 position)
            : base(model, position)
        {
        }
    }
}
