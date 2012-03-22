using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework.Content;

namespace SpiderDefense
{
    class SpiderDispatcher
    {
        private ContentManager content;
        private Map map;
        Camera camera;
        private bool running = true;
        private int level = 1;
        private int spiderCount;

        private TimeSpan timeBetweenSwarms = new TimeSpan(0, 0, 10);
        private TimeSpan currentTime = new TimeSpan(0, 0, 0);
        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private BackgroundWorker bckg = new BackgroundWorker();
        private AutoResetEvent pauseEvent;

        public SpiderDispatcher(ContentManager content, Map map, Camera camera, AutoResetEvent pauseEvent)
        {
            this.content = content;
            this.map = map;
            this.camera = camera;
            this.pauseEvent = pauseEvent;

            bckg.DoWork += new DoWorkEventHandler(bckg_DoWork);
            bckg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bckg_RunWorkerCompleted);
        }

        #region Properties
        public bool Running
        {
            get { return running; }
            set { running = value; }
        }

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public int SpiderCount
        {
            get { return spiderCount; }
            set { spiderCount = value; }
        }

        public TimeSpan TimeToNextSwarm
        {
            get
            {
                if (currentTime != TimeSpan.Zero)
                    return timeBetweenSwarms - currentTime;
                else
                    return TimeSpan.Zero;
            }
        }

        #endregion

        //starts the spider dispatcher
        //while running it will dispatch athe specified number of spiders and advance to the next level
        public void DispatchSpider()
        {
            while (running)
            {
                Random rnd = new Random();

                float speed = 0.0f;
                float health = 0.0f;
                float shield = 0.0f;

                float factor = level / 3.0f;

                speed = .14f * factor;
                health = 32 * factor;
                if (level < 5)
                    shield = 0;
                else
                    shield = .01f * factor;

                for (int i = 0; i < spiderCount; i++)
                {
                    int pause = rnd.Next(1, 4);
                    Spider spider = new Spider(content, map.Path, camera);
                    spider.Speed = speed;
                    spider.TotalHP = health;
                    spider.CurrentHP = health;
                    spider.Shield = shield;
                    spider.Level = level;
                    spider.SetMoving(spider.NextCheckpoint.Position);

                    //pause this thread in case the game has been paused
                    pauseEvent.WaitOne();

                    //pause between each spider
                    if (i != 0)
                        Thread.Sleep(2000 * pause);
                    map.AddModel(spider);
                }
                while (map.AllMapModels.Count(n => n is Spider) != 0) { }

                //starting the timer between the levels
                bckg.RunWorkerAsync();

                //the dispatcher thread waits until the timer has finished
                autoResetEvent.WaitOne();
                level++;
            }
        }

        void bckg_DoWork(object sender, DoWorkEventArgs e)
        {
            TimeSpan interval = new TimeSpan(0, 0, 1);
            currentTime = new TimeSpan(0, 0, 0);
            while (currentTime < timeBetweenSwarms)
            {
                pauseEvent.WaitOne();
                currentTime = currentTime.Add(interval);
                Thread.Sleep(interval);
            }
        }

        void bckg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //allow the dispatcher thread to continue
            autoResetEvent.Set();
        }
    }
}
