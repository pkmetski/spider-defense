
namespace SpiderDefense
{
    public class SelectionCircleAttributes
    {
        private const float radius = 1.5f;
        private const int pointsCount = 36;
        private const int linesCount = 5;

        private int mapWidth;
        private int mapHeight;
        private float[,] heightValues;

        public SelectionCircleAttributes(int mapWidth, int mapHeight, float[,] heightValues)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.heightValues = heightValues;
        }

        public float Radius
        {
            get { return radius; }
        }

        public int PointsCount
        {
            get { return pointsCount; }
        }

        public int LinesCount
        {
            get { return linesCount; }
        }

        public int MapWidth
        {
            get { return mapWidth; }
        }

        public int MapHeight
        {
            get { return mapHeight; }
        }

        public float[,] HeightValues
        {
            get { return heightValues; }
        }
    }
}
