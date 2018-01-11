using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Graphs
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SearchAlgorithms : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        static Texture2D g_floorTexture;
        static Texture2D g_wallTexture;
        static Texture2D g_visitedNodeTexture;
        static Texture2D g_visitingNodeTexture;

        // TODO: set this
        ISearchAlgorithm g_currentSearchAlgorithm;

        const int g_mapDimension = 9;
        int[,] g_map =
            {
                { 0, 1, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 1, 1, 1, 1, 1, 1, 1 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 1, 1, 1, 1, 1, 1, 0 },
                { 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 1, 1, 1, 1, 1, 1, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            };

        Tile[,] g_tiles = new Tile[g_mapDimension, g_mapDimension];

        public SearchAlgorithms()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            for(int i = 0; i < g_mapDimension; i++)
            {
                for(int j = 0; j < g_mapDimension; j++)
                {
                    if(g_map[j,i] == 1)
                    {
                        g_tiles[j,i] = new Tile(TileType.Impassable, i, j);
                    }
                    else
                    {
                        g_tiles[j, i] = new Tile(TileType.Passable, i, j);
                    }
                }
            }

            g_currentSearchAlgorithm = new DepthFirstSearchAlgorithm(g_tiles, 1, 1, 7, 7);
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            g_floorTexture = Content.Load<Texture2D>("grass1");
            g_wallTexture = Content.Load<Texture2D>("wall_4_way");
            g_visitedNodeTexture = Content.Load<Texture2D>("red_dot");
            g_visitingNodeTexture = Content.Load<Texture2D>("yellow_dot");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            if(gameTime.TotalGameTime.Milliseconds % 60 == 0)
            {
                g_currentSearchAlgorithm.Tick();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            int mapWidth = g_mapDimension * 16; // 16px per tile
            int mapHeight = g_mapDimension * 16;

            int startX = (GraphicsDevice.PresentationParameters.BackBufferWidth / 2) - (mapWidth / 2);
            int startY = (GraphicsDevice.PresentationParameters.BackBufferHeight / 2) - (mapHeight / 2);

            VisitedStatus[,] visitedNodes = g_currentSearchAlgorithm.GetVisited();

            Matrix scaleMatrix = Matrix.CreateScale(2f);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, scaleMatrix);
            for(int i = 0; i < g_mapDimension; i++)
            {
                for(int j = 0; j < g_mapDimension; j++)
                {
                    var pos = new Vector2((i * 16), (j * 16));
                    // j,i because our map is column-major not row-major
                    spriteBatch.Draw(g_tiles[j,i].Texture, pos, Color.White);

                    if(visitedNodes[j,i] == VisitedStatus.Visited)
                    {
                        spriteBatch.Draw(g_visitedNodeTexture, pos, Color.White);
                    }

                    if (visitedNodes[j, i] == VisitedStatus.Visiting)
                    {
                        spriteBatch.Draw(g_visitingNodeTexture, pos, Color.White);
                    }
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        enum TileType
        {
            Passable,
            Impassable
        }

        class Tile
        {
            public int X;
            public int Y;
            public Texture2D Texture
            {
                get
                {
                    if(this.m_type == TileType.Impassable)
                    {
                        return g_wallTexture;
                    }
                    else
                    {
                        return g_floorTexture;
                    }
                }
            }

            private TileType m_type;

            public Tile(TileType type, int x, int y)
            {
                this.m_type = type;
                this.X = x;
                this.Y = y;
            }

            public bool IsPassable()
            {
                return m_type == TileType.Passable;
            }
        }

        enum VisitedStatus
        {
            Unvisited,
            Visiting,
            Visited
        }

        interface ISearchAlgorithm
        {
            void Tick();
            VisitedStatus[,] GetVisited();
        }

        class DepthFirstSearchAlgorithm : ISearchAlgorithm
        {
            Stack<Tile> m_openSet = new Stack<Tile>();
            HashSet<Tile> m_closedSet = new HashSet<Tile>();

            Tile[,] m_map;
            VisitedStatus[,] m_visited;

            int m_goalX;
            int m_goalY;

            bool m_done;

            public DepthFirstSearchAlgorithm(Tile[,] map, int startX, int startY, int goalX, int goalY)
            {
                m_map = map;
                // column major so backwards
                m_openSet.Push(m_map[startY, startX]);

                m_visited = new VisitedStatus[g_mapDimension, g_mapDimension];

                m_done = false;
                m_goalX = goalX;
                m_goalY = goalY;
            }

            public void Tick()
            {
                if(m_done)
                {
                    return;
                }

                // Examine our next item off the open set
                Tile nextTile = m_openSet.Pop();
                if(m_closedSet.Contains(nextTile))
                {
                    return;
                }

                if (nextTile.X == m_goalX && nextTile.Y == m_goalY)
                {
                    // we're done!
                    m_done = true;
                    return;
                }
                    
                // Visit our tile
                m_closedSet.Add(nextTile);
                m_visited[nextTile.Y, nextTile.X] = VisitedStatus.Visited;

                // up
                if (IsTileVaild(nextTile.Y - 1, nextTile.X))
                {
                    m_openSet.Push(m_map[nextTile.Y - 1, nextTile.X]);
                    m_visited[nextTile.Y - 1, nextTile.X] = VisitedStatus.Visiting;
                }

                // down
                if (IsTileVaild(nextTile.Y + 1, nextTile.X))
                {
                    m_openSet.Push(m_map[nextTile.Y + 1, nextTile.X]);
                    m_visited[nextTile.Y + 1, nextTile.X] = VisitedStatus.Visiting;
                }

                // left
                if (IsTileVaild(nextTile.Y, nextTile.X - 1))
                {
                    m_openSet.Push(m_map[nextTile.Y, nextTile.X - 1]);
                    m_visited[nextTile.Y, nextTile.X - 1] = VisitedStatus.Visiting;
                }

                // right
                if (IsTileVaild(nextTile.Y, nextTile.X + 1))
                {
                    m_openSet.Push(m_map[nextTile.Y, nextTile.X + 1]);
                    m_visited[nextTile.Y, nextTile.X + 1] = VisitedStatus.Visiting;
                }
            }

            public VisitedStatus[,] GetVisited()
            {
                return m_visited;
            }

            public bool IsTileVaild(int tileY, int tileX)
            {
                if(tileX < 0 || tileX >= g_mapDimension
                    || tileY < 0 || tileY >= g_mapDimension)
                {
                    return false;
                }

                return m_map[tileY, tileX].IsPassable();
            }
        }
    }
}
