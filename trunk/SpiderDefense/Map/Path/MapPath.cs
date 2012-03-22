using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SpiderDefense
{
    //a collection of PathCheckpoints, combined, all the checkpoints form a complete path
    class MapPath
    {
        private List<PathCheckpoint> path = new List<PathCheckpoint>();

        public MapPath(float[,] heightValues)
        {
            int width = heightValues.GetUpperBound(0) + 1;
            int height = heightValues.GetUpperBound(1) + 1;
            path.Add(new PathCheckpoint(
        new Vector3(width / 3, heightValues[(int)width / 3, 0], 0.0f)));

            path.Add(new PathCheckpoint(
                new Vector3(width / 3, heightValues[(int)width / 3, (int)height / 2], -height / 2)));

            path.Add(new PathCheckpoint(
                new Vector3(2 * (width / 3), heightValues[(int)2 * (width / 3), (int)height / 2], -height / 2)));

            path.Add(new PathCheckpoint(
                new Vector3(2 * (width / 3), heightValues[(int)2 * (width / 3), (int)2 * (height / 3)], -2 * (height / 3))));

            path.Add(new PathCheckpoint(
                new Vector3(width / 6, heightValues[(int)width / 6, (int)2 * (height / 3)], -2 * (height / 3))));

            path.Add(new PathCheckpoint(
    new Vector3(width / 6, heightValues[(int)width / 6, (int)5 * (height / 6)], -5 * (height / 6))));

            path.Add(new PathCheckpoint(
    new Vector3(5 * (width / 6), heightValues[(int)5 * (width / 6), (int)5 * (height / 6)], -5 * (height / 6))));

            path.Add(new PathCheckpoint(
    new Vector3(5 * (width / 6), heightValues[(int)5 * (width / 6), (int)height - 1], -(height - 1))));


        }

        public void AddCheckpoint(PathCheckpoint checkpoint)
        {
            path.Add(checkpoint);
        }

        public PathCheckpoint GetNextCheckpoint(PathCheckpoint currentCheckpoint)
        {
            PathCheckpoint nextCheckpoint = null;
            int index = path.IndexOf(currentCheckpoint);

            if (index < path.Count - 1)
            {
                nextCheckpoint = path[index + 1];
            }

            return nextCheckpoint;
        }

        public List<PathCheckpoint> Checkpoints
        {
            get { return path; }
        }
    }
}
