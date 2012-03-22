using Microsoft.Xna.Framework;

namespace SpiderDefense
{
    //a location in the 3D space, indicating a start of a new path section
    class PathCheckpoint
    {
        private Vector3 checkpointPosition;

        public PathCheckpoint(Vector3 checkpointPosition)
        {
            this.checkpointPosition = checkpointPosition;
        }

        public Vector3 Position
        {
            get { return checkpointPosition; }
        }
    }
}
