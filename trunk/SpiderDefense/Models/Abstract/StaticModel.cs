using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    //a generic class used for representing all the non movable objects on the map
    abstract class StaticModel : MapModel
    {
        public StaticModel(Model model)
            : base(model)
        {
        }

        public StaticModel(Model model, Vector3 position)
            : base(model, position)
        {
        }
    }
}
