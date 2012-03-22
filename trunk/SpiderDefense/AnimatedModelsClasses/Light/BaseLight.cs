
using Microsoft.Xna.Framework;

namespace SpiderDefense
{
    public abstract class BaseLight
    {
        // Light difusse and specular color
         Vector3 color;

        #region Properties
        public Vector3 Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }
        #endregion

        public BaseLight()
        {
        }

        public BaseLight(Vector3 color)
        {
            this.color = color;
        }
    }
}
