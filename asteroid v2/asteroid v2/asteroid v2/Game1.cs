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

namespace asteroid_v2
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        BasicEffect basicEffect;
        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        SpriteFont font;
        SpriteFont fontBig;
        Ship ship = new Ship();
        List<Asteroid> asteroids = new List<Asteroid>();
        Random r = new Random();
        List<Bullet> bullet = new List<Bullet>();
        //Bullet[] bullet;
        bool bOldSpace = false;
        int score = 0;
        bool gameOver = false; 

        class Ship
        {
            VertexPositionColor[] vertexShip;
            Vector3 posShip;
            float size;
            float rotation;
            float speed;
            float speedLimit;
            int life;
            bool immortal;
            public Ship()
            {
                immortal = true;
                life = 3;
                size = 1;
                rotation = 0;
                posShip = new Vector3(0, 0, 0);
                speed = 0;
                speedLimit = 0.5f;
                vertexShip = new VertexPositionColor[3];
                vertexShip[0] = new VertexPositionColor(new Vector3(posShip.X + size, posShip.Y, posShip.Z), Color.Orange);
                vertexShip[1] = new VertexPositionColor(new Vector3(posShip.X - size, posShip.Y - size, posShip.Z), Color.Red);
                vertexShip[2] = new VertexPositionColor(new Vector3(posShip.X - size, posShip.Y + size, posShip.Z), Color.Red);          
            }

            #region Getters
            public VertexPositionColor[] GetVertex()
            {
                return this.vertexShip;
            }

            public Vector3 GetPosShip()
            {
                return posShip;
            }

            public float GetRotation()
            {
                return rotation;
            }

            public float GetSpeed()
            {
                return speed;
            }

            public int GetLife()
            {
                return life;
            }
            #endregion

            #region Setters
            public void SetSpeed(float s)
            {
                speed = s;
            }

            public void SetImmortal(bool i)
            {
                immortal = i;
            }
            #endregion


            public void Turn(float r)
            {
                for (int i = 0; i < this.vertexShip.Length; i++)
                { 
                    vertexShip[i].Position = Vector3.Transform(vertexShip[i].Position, 
                        Matrix.CreateTranslation(new Vector3(-posShip.X, -posShip.Y, -posShip.Z)) 
                        * Matrix.CreateRotationZ(MathHelper.ToRadians(-r))
                        * Matrix.CreateTranslation(new Vector3(posShip.X , posShip.Y, posShip.Z))); 
                }
                rotation += r;
                if (rotation > 360)
                    rotation -= 360;
                else if (rotation < 0)
                    rotation += 360;
            }

            public void MoveShip()
            { 
                Vector3 v = new Vector3(speed, 0, 0);

                v = Vector3.Transform(v, Matrix.CreateRotationZ(MathHelper.ToRadians(-rotation)));

                posShip = Vector3.Transform(posShip, Matrix.CreateTranslation(v));
               
                for (int i = 0; i < vertexShip.Length; i++)
                {

                    vertexShip[i].Position = Vector3.Transform(vertexShip[i].Position,
                        Matrix.CreateTranslation(v));
                }
                //funkcja ograniczajaca pozycje statku
                Edges();
            }

            public void Edges()
            {
                int szerokoscOkna = 28, wyskoscOkna = 18;
                //ograniczenie okna szerokosc
                if (posShip.X > szerokoscOkna)
                {
                    posShip = Vector3.Transform(posShip, Matrix.CreateTranslation(new Vector3(-szerokoscOkna * 2, 0, 0)));
                    for (int i = 0; i < vertexShip.Length; i++)
                    {
                        vertexShip[i].Position = Vector3.Transform(vertexShip[i].Position,
                            Matrix.CreateTranslation(new Vector3(-szerokoscOkna * 2, 0, 0)));
                    }
                }
                else if (posShip.X < -szerokoscOkna)
                {
                    posShip = Vector3.Transform(posShip, Matrix.CreateTranslation(new Vector3(szerokoscOkna * 2, 0, 0)));
                    for (int i = 0; i < vertexShip.Length; i++)
                    {
                        vertexShip[i].Position = Vector3.Transform(vertexShip[i].Position,
                            Matrix.CreateTranslation(new Vector3(szerokoscOkna * 2, 0, 0)));
                    }
                }
                //ograniczenie okna wyskokosc
                if (posShip.Y > wyskoscOkna)
                {
                    posShip = Vector3.Transform(posShip, Matrix.CreateTranslation(new Vector3(0, -wyskoscOkna * 2, 0)));
                    for (int i = 0; i < vertexShip.Length; i++)
                    {
                        vertexShip[i].Position = Vector3.Transform(vertexShip[i].Position,
                            Matrix.CreateTranslation(new Vector3(0, -wyskoscOkna * 2, 0)));
                    }
                }
                else if (posShip.Y < -wyskoscOkna)
                {
                    posShip = Vector3.Transform(posShip, Matrix.CreateTranslation(new Vector3(0, wyskoscOkna * 2, 0)));
                    for (int i = 0; i < vertexShip.Length; i++)
                    {
                        vertexShip[i].Position = Vector3.Transform(vertexShip[i].Position,
                            Matrix.CreateTranslation(new Vector3(0, wyskoscOkna * 2, 0)));
                    }
                }
            }

            public void Thrust(float s)
            {
                speed += s;
                if (speed < 0)
                    speed = 0;
                if (speed > speedLimit)
                    speed = speedLimit;
                    
            }

            public Bullet Shoot()
            {
                Bullet bullet = new Bullet(posShip, rotation, speed);
                return bullet;
            }

            public void Collision(Asteroid asteroid)
            {
                if (immortal == false)
                {
                    float dist;
                    dist = Vector3.Distance(posShip, asteroid.GetPos());

                    if (dist < asteroid.GetSize() || dist < size)
                    {
                        rotation = 0;
                        posShip = new Vector3(0, 0, 0);
                        speed = 0;
                        vertexShip = new VertexPositionColor[3];
                        vertexShip[0] = new VertexPositionColor(new Vector3(posShip.X + size, posShip.Y, posShip.Z), Color.Orange);
                        vertexShip[1] = new VertexPositionColor(new Vector3(posShip.X - size, posShip.Y - size, posShip.Z), Color.Red);
                        vertexShip[2] = new VertexPositionColor(new Vector3(posShip.X - size, posShip.Y + size, posShip.Z), Color.Red);
                        life--;
                        immortal = true;
                    }
                }
                

            }
        }

        class Asteroid
        {
            Vector3 pos;
            VertexPositionColor[] vertex;
            float speed;
            float rotation;
            float size;
            bool exist;
            public Asteroid(Random r)
            {
                exist = true;
                rotation = r.Next(0, 359);
                pos = new Vector3(r.Next(-28, 28), r.Next(-18, 18), 0);              
                speed = (float) r.NextDouble() / 3;
                //speed = 0;
                size = 2;
                vertex = new VertexPositionColor[9];               
                for (int i = 0; i < vertex.Length - 1; i++)
                {
                    float a = (float) r.NextDouble();
                    Vector3 v = new Vector3(size - a, 0, 0);
                    v = Vector3.Transform(v, Matrix.CreateRotationZ(MathHelper.ToRadians(i*45)));                   
                    vertex[i] = new VertexPositionColor(pos, Color.Black);
                    vertex[i].Position = Vector3.Transform(pos, Matrix.CreateTranslation(v));
                }
                vertex[8] = vertex[0];
            }

            public Asteroid(Random r, Vector3 position, float sizeAsteroid, float speedAsteroid)
            {
                exist = true;
                rotation = r.Next(0, 359);
                pos = position;
                speed = speedAsteroid;
                size = sizeAsteroid / 2;
                vertex = new VertexPositionColor[9];
                for (int i = 0; i < vertex.Length - 1; i++)
                {
                    float a = (float)r.NextDouble();
                    Vector3 v = new Vector3(size - (a *( size / 2)), 0, 0);
                    v = Vector3.Transform(v, Matrix.CreateRotationZ(MathHelper.ToRadians(i * 45)));
                    vertex[i] = new VertexPositionColor(pos, Color.Black);
                    vertex[i].Position = Vector3.Transform(pos, Matrix.CreateTranslation(v));
                }
                vertex[8] = vertex[0];
            }

            public VertexPositionColor[] GetVertex()
            {
                return vertex;
            }

            public void MoveAsteroid()
            {
                Vector3 v = new Vector3(speed, 0, 0);

                v = Vector3.Transform(v, Matrix.CreateRotationZ(MathHelper.ToRadians(-rotation)));

                pos = Vector3.Transform(pos, Matrix.CreateTranslation(v));

                for (int i = 0; i < vertex.Length; i++)
                {

                    vertex[i].Position = Vector3.Transform(vertex[i].Position,
                        Matrix.CreateTranslation(v));
                }
                //funkcja ograniczajaca pozycje statku
                Edges();
            }

            public void Edges()
            {
                int szerokoscOkna = 28, wyskoscOkna = 18;
                //ograniczenie okna szerokosc
                if (pos.X > szerokoscOkna)
                {
                    pos = Vector3.Transform(pos, Matrix.CreateTranslation(new Vector3(-szerokoscOkna * 2, 0, 0)));
                    for (int i = 0; i < vertex.Length; i++)
                    {
                        vertex[i].Position = Vector3.Transform(vertex[i].Position,
                            Matrix.CreateTranslation(new Vector3(-szerokoscOkna * 2, 0, 0)));
                    }
                }
                else if (pos.X < -szerokoscOkna)
                {
                    pos = Vector3.Transform(pos, Matrix.CreateTranslation(new Vector3(szerokoscOkna * 2, 0, 0)));
                    for (int i = 0; i < vertex.Length; i++)
                    {
                        vertex[i].Position = Vector3.Transform(vertex[i].Position,
                            Matrix.CreateTranslation(new Vector3(szerokoscOkna * 2, 0, 0)));
                    }
                }
                //ograniczenie okna wyskokosc
                if (pos.Y > wyskoscOkna)
                {
                    pos = Vector3.Transform(pos, Matrix.CreateTranslation(new Vector3(0, -wyskoscOkna * 2, 0)));
                    for (int i = 0; i < vertex.Length; i++)
                    {
                        vertex[i].Position = Vector3.Transform(vertex[i].Position,
                            Matrix.CreateTranslation(new Vector3(0, -wyskoscOkna * 2, 0)));
                    }
                }
                else if (pos.Y < -wyskoscOkna)
                {
                    pos= Vector3.Transform(pos, Matrix.CreateTranslation(new Vector3(0, wyskoscOkna * 2, 0)));
                    for (int i = 0; i < vertex.Length; i++)
                    {
                        vertex[i].Position = Vector3.Transform(vertex[i].Position,
                            Matrix.CreateTranslation(new Vector3(0, wyskoscOkna * 2, 0)));
                    }
                }
            }

            public float GetSize()
            {
                return size;
            }

            public Vector3 GetPos()
            {
                return pos;
            }

            public bool GetExist()
            {
                return exist;
            }

            public float GetSpeed()
            {
                return speed;
            }

            public void SetExist(bool ex)
            {
                exist = ex;
            }
        }

        class Bullet
        {
            Vector3 pos;
            float speed;
            float rotation;
            VertexPositionColor[] vertex;
            float size;
            float range;
            bool exist;

            public Bullet()
            {
                pos = new Vector3();
                rotation = 0;
                speed = 0;

                size = 2;
                vertex = new VertexPositionColor[9];
                for (int i = 0; i < vertex.Length - 1; i++)
                {
                    Vector3 v = new Vector3(size, 0, 0);
                    v = Vector3.Transform(v, Matrix.CreateRotationZ(MathHelper.ToRadians(i * 45)));
                    vertex[i] = new VertexPositionColor(pos, Color.Black);
                    vertex[i].Position = Vector3.Transform(pos, Matrix.CreateTranslation(v));
                }
                vertex[8] = vertex[0];
            }

            public Bullet(Vector3 posShip, float rotationShip, float speedShip)
            {
                exist = true;
                range = 50;
                pos = posShip;
                rotation = rotationShip;
                size = 0.2f;
                speed = speedShip + 0.5f;
                vertex = new VertexPositionColor[9];
                for (int i = 0; i < vertex.Length - 1; i++)
                {
                    Vector3 v = new Vector3(size, 0, 0);
                    v = Vector3.Transform(v, Matrix.CreateRotationZ(MathHelper.ToRadians(i * 45)));
                    vertex[i] = new VertexPositionColor(pos, Color.Black);
                    vertex[i].Position = Vector3.Transform(pos, Matrix.CreateTranslation(v));
                }
                vertex[8] = vertex[0];
            } 

            public VertexPositionColor[] GetVertex()
            {
                return vertex;
            }

            public void MoveBullet()
            {
                Vector3 v = new Vector3(speed, 0, 0);

                v = Vector3.Transform(v, Matrix.CreateRotationZ(MathHelper.ToRadians(-rotation)));

                pos = Vector3.Transform(pos, Matrix.CreateTranslation(v));

                for (int i = 0; i < vertex.Length; i++)
                {
                    vertex[i].Position = Vector3.Transform(vertex[i].Position,
                        Matrix.CreateTranslation(v));
                }
                Edges();
                range--;
            }

            public void Edges()
            {
                int szerokoscOkna = 28, wyskoscOkna = 18;
                //ograniczenie okna szerokosc
                if (pos.X > szerokoscOkna)
                {
                    pos = Vector3.Transform(pos, Matrix.CreateTranslation(new Vector3(-szerokoscOkna * 2, 0, 0)));
                    for (int i = 0; i < vertex.Length; i++)
                    {
                        vertex[i].Position = Vector3.Transform(vertex[i].Position,
                            Matrix.CreateTranslation(new Vector3(-szerokoscOkna * 2, 0, 0)));
                    }
                }
                else if (pos.X < -szerokoscOkna)
                {
                    pos = Vector3.Transform(pos, Matrix.CreateTranslation(new Vector3(szerokoscOkna * 2, 0, 0)));
                    for (int i = 0; i < vertex.Length; i++)
                    {
                        vertex[i].Position = Vector3.Transform(vertex[i].Position,
                            Matrix.CreateTranslation(new Vector3(szerokoscOkna * 2, 0, 0)));
                    }
                }
                //ograniczenie okna wyskokosc
                if (pos.Y > wyskoscOkna)
                {
                    pos = Vector3.Transform(pos, Matrix.CreateTranslation(new Vector3(0, -wyskoscOkna * 2, 0)));
                    for (int i = 0; i < vertex.Length; i++)
                    {
                        vertex[i].Position = Vector3.Transform(vertex[i].Position,
                            Matrix.CreateTranslation(new Vector3(0, -wyskoscOkna * 2, 0)));
                    }
                }
                else if (pos.Y < -wyskoscOkna)
                {
                    pos = Vector3.Transform(pos, Matrix.CreateTranslation(new Vector3(0, wyskoscOkna * 2, 0)));
                    for (int i = 0; i < vertex.Length; i++)
                    {
                        vertex[i].Position = Vector3.Transform(vertex[i].Position,
                            Matrix.CreateTranslation(new Vector3(0, wyskoscOkna * 2, 0)));
                    }
                }
            }
            
            public bool ExistBullet()
            {
                if (range == 0)
                {
                    exist = false;
                }
                return exist;
            }

            public bool Hit(Asteroid asteroid)
            {
                float dist;
                bool asteroidExist = true;
                dist = Vector3.Distance(pos, asteroid.GetPos());
                if (dist < asteroid.GetSize())
                {
                    range = 0;
                    asteroidExist = false;
                }

                return asteroidExist;
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            for (int i = 0; i < 10; i++)
            {
                asteroids.Add(new Asteroid(r));
            }
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

            base.Initialize();
            this.graphics.PreferredBackBufferWidth = 1280;
            this.graphics.PreferredBackBufferHeight = 800;
            this.graphics.ApplyChanges();
            this.IsMouseVisible = true;
            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.VertexColorEnabled = true;

            font = Content.Load<SpriteFont>("SpriteFont1");
            fontBig = Content.Load<SpriteFont>("SpriteFont2");
            
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
            KeyboardState keybord = Keyboard.GetState();
            
           if (keybord.IsKeyDown(Keys.Escape))
                    this.Exit();
           if (gameOver == false)
           {
                if (keybord.IsKeyDown(Keys.Right))
                    ship.Turn(4f);
                else if (keybord.IsKeyDown(Keys.Left))
                    ship.Turn(-4f);

                if (keybord.IsKeyDown(Keys.Up))
                {
                    ship.SetImmortal(false);
                    ship.Thrust(0.01f);
                }

                if (keybord.IsKeyDown(Keys.Space) && !bOldSpace)
                {
                    ship.SetImmortal(false);
                    bullet.Add(ship.Shoot());
                    bOldSpace = true;
                }
                bOldSpace = keybord.IsKeyDown(Keys.Space);

                //obsługa pocisków przemieszczanie oraz czyszczenie lisy
                bool b = false;
                do
                {
                    b = false;
                    foreach (Bullet bullet1 in bullet)
                    {
                        bullet1.MoveBullet();
                        b = false;
                        foreach (Asteroid asteroid in asteroids)
                        {
                            asteroid.SetExist(bullet1.Hit(asteroid));
                            if (!asteroid.GetExist())
                            {
                                if (asteroid.GetSize() >= 1)
                                {
                                    asteroids.Add(new Asteroid(r, asteroid.GetPos(), asteroid.GetSize(), asteroid.GetSpeed()));
                                    asteroids.Add(new Asteroid(r, asteroid.GetPos(), asteroid.GetSize(), asteroid.GetSpeed()));
                                }
                                asteroids.Remove(asteroid);
                                score += 100;
                                break;
                            }
                        }
                        if (!bullet1.ExistBullet())
                        {
                            bullet.Remove(bullet1);
                            b = true;
                            break;
                        }

                    }
                } while (b);

                foreach (Asteroid asteroid in asteroids)
                {
                    ship.Collision(asteroid);

                }

                //ruch statku
                ship.Thrust(-0.005f);
                ship.MoveShip();

                //ruch asteroid
                foreach (Asteroid asteroids1 in asteroids)
                {
                    asteroids1.MoveAsteroid();
                }
            }

            worldMatrix = Matrix.Identity;
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 40), Vector3.Zero, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 
                GraphicsDevice.Viewport.AspectRatio, 0.01f, 1000);
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;




            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            basicEffect.CurrentTechnique.Passes[0].Apply();

            if (ship.GetLife() > 0)
            {
                foreach (Asteroid asteroids1 in asteroids)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, asteroids1.GetVertex(), 0, 8);
                }

                foreach (Bullet bullet1 in bullet)
                {
                    if (bullet1.ExistBullet())
                        GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, bullet1.GetVertex(), 0, 8);
                }
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, ship.GetVertex(), 0, 1);

                spriteBatch.Begin();

                spriteBatch.DrawString(font, "SCOORE " + score, new Vector2(400, 0), Color.White);
                spriteBatch.DrawString(font, "LIFES " + ship.GetLife(), new Vector2(600, 0), Color.White);

                /*
                //debug
                spriteBatch.DrawString(font, "pos=("+ship.GetPosShip().X+", "+ship.GetPosShip().Y+", "+ship.GetPosShip().Z+")"  , Vector2.Zero, Color.White);
                spriteBatch.DrawString(font, "rotation= " + ship.GetRotation(), new Vector2(0, 20), Color.White);
                VertexPositionColor[] a = ship.GetVertex();
                spriteBatch.DrawString(font, "pos+1=(" + a[0].Position.X + ", " + a[0].Position.Y + ", " + a[0].Position.Z + ")", new Vector2(0, 40), Color.White);
                spriteBatch.DrawString(font, "Speed= " + ship.GetSpeed(), new Vector2(0, 60), Color.White);
                spriteBatch.DrawString(font, "Zycia " + ship.GetLife(), new Vector2(0, 80), Color.White);
                */
                spriteBatch.End();
            }
            else
            {
                gameOver = true;
                spriteBatch.Begin();
                spriteBatch.DrawString(fontBig, "GAME OVER", new Vector2(400, 300), Color.White);
                spriteBatch.DrawString(font, "SCOORE " + score, new Vector2(600, 450), Color.White);
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}
