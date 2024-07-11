using System;
using System.Collections.Generic;
using System.Text;

namespace Planes3D
{
    public sealed class Game
    {
        public static Game Current;

        public PlayerAirplane Player;
        public Terrain Terrain;

        public Enemy enemy;

        public Scene Scene;

        public void Start()
        {
            Player = new PlayerAirplane();

            Terrain = new Terrain();
            Terrain.Build("data/heightmap.bmp");

            Enemy enemy = new Enemy();
            enemy.Position = new Vector3(25, 15, 35);

            Scene = new Scene();
            Scene.Add(enemy);
            Scene.Add(Player);
        }

        public void Update()
        {
            Scene.Update();
        }

        public void Draw()
        {
            Scene.Draw();
            Terrain.Draw(new Vector3(0, 0, 0));
        }
    }
}
