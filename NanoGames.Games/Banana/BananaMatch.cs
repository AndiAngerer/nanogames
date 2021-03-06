﻿// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Collections.Generic;


/*
ToDo:
    Anzeige: near miss
    verschiedene modi: suddenddeath
    töne
    mappool oder random maps
*/

namespace NanoGames.Games.Banana
{
    class BananaMatch : Match<BananaPlayer>
    {
        public int FramesLeft = 0;
        private int framesMax = 3000;
        public string StateOfGame = "ActivePlayerActing";
        public BananaPlayer ActivePlayer;
        public int StartPlayerIdx = 0;
        private int activePlayerIdx = 0;
        public Map Map = new Map();
        public Landscape Land = new Landscape();
        public Bullet Bullet = new Bullet();
        public Grenade Grenade = new Grenade();
        public Wind Wind = new Wind();
        public AudioSettings MatchAudioSettings = new AudioSettings();

        private int finishedPlayers = 0;
        private bool somethingFlying = false;

        protected override void Initialize()
        {
            // initialize map
            Map.Initialize();

            foreach (var player in Players)
            {
                player.GetBorn(Map.GetRandomBorderPixel(Random.NextDouble()));
            }

            StartPlayerIdx = Convert.ToInt32(Math.Floor(Random.NextDouble() * Players.Count));
            activePlayerIdx = StartPlayerIdx;
            ActivePlayer = Players[activePlayerIdx];

            Wind.SetSpeed(Random);

            FramesLeft = framesMax;
        }

        protected override void Update()
        {
            switch (StateOfGame)
            {
                case "NextPlayer":

                    do
                    {
                        activePlayerIdx++;
                        activePlayerIdx = activePlayerIdx % Players.Count;
                        ActivePlayer = Players[activePlayerIdx];

                    } while (ActivePlayer.HasFinished);

                    FramesLeft = framesMax + 60;
                    Wind.SetSpeed(Random);
                    StateOfGame = "Wait";     // StateOfGame -> Wait 1 s
                    break;

                case "Wait":

                    if (FramesLeft <= framesMax)
                    {
                        StateOfGame = "ActivePlayerActing";     // StateOfGame -> ActivePlayerActing
                        MatchAudioSettings.NextPlayer = true;
                    }

                    break;
                    
                case "ActivePlayerActing":

                    ActivePlayer.Move();
                    ActivePlayer.SetAngle();
                    ActivePlayer.SetWeapon();
                    ActivePlayer.Shoot1();                  // StateOfGame -> Shooting2
                    break;

                case "ActivePlayerShoot2":
                    ActivePlayer.Shoot2();                  // StateOfGame -> Shooting3
                    break;

                case "ActivePlayerShoot3":
                    ActivePlayer.Shoot3();                  // StateOfGame -> SomethingFlying, ...
                    break;

                case "SomethingFlying":

                    somethingFlying = false;

                    // bullet flying
                    if (!Bullet.IsExploded)
                    {
                        Bullet.MoveBullet(Wind);
                        CheckCollisionBulletScreen();
                        CheckCollisionBulletLand(); 
                        if (!Bullet.IsExploded)
                        {
                            //CheckCollisionBulletPlayers();
                        }
                        somethingFlying = true;
                    }

                    // grenade flying
                    if (!Grenade.IsExploded && !Grenade.IsDead)
                    {
                        Grenade.MoveGrenade();
                        CheckCollisionGrenadeLand();
                        CheckCollisionGrenadeScreen();
                        CheckGrenadeExplosion();
                        somethingFlying = true;
                    }

                    if (!somethingFlying)
                    {
                        StateOfGame = "NextPlayer";            // StateOfGame -> NextPlayer
                    }

                    break;
            }

            if ((FramesLeft == 300) ||
                (FramesLeft == 240) ||
                (FramesLeft == 180) ||
                (FramesLeft == 120))
            {
                MatchAudioSettings.TimerFiveSecondsToGo = true;
            }

            if (FramesLeft == 60)
            {
                MatchAudioSettings.TimerOneSecondToGo = true;
            }

            FramesLeft--;
            
            if (FramesLeft <= 0 && !somethingFlying)
            {
                StateOfGame = "NextPlayer";
            }

            foreach (var player in Players)
            {
                if (player.Health <= 0)
                {
                    if (!player.HasFinished)
                    {
                        finishedPlayers++;
                    }
                    player.HasFinished = true;

                    /* Finishing lateer is better. */
                    player.Score = finishedPlayers;
                }
            }

            foreach (var player in Players)
            {
                player.DrawScreen();
                player.PlayAudio();
            }
            MatchAudioSettings.Reset();

            if (Players.Count == 1)
            {
                /* Practice mode. */
                if (finishedPlayers == 1)
                {
                    IsCompleted = true;
                }
            }
            else
            {
                /* Tournament mode. The match ends when the second-to-last player has reached the goal. */
                if (finishedPlayers >= Players.Count - 1)
                {
                    IsCompleted = true;
                }
            }
        }
        
        /*
        private void CheckCollisionBulletPlayers()
        {
            foreach (var player in Players)
            {
                for (int i = 0; i < player.Hitbox.Length - 2; i++)
                {
                    Intersection intersection = new Intersection(Bullet.Position, Bullet.PositionBefore, player.Hitbox[i], player.Hitbox[i + 1]);

                    if (intersection.IsTrue)
                    {
                        MatchAudioSettings.BulletExploded = true;
                        Bullet.IsExploded = true;
                        Land.makeCaldera(intersection.Point, 5);

                        // push player into caldera
                        bool foundSomething = false;
                        Vector p1 = player.Pixel.Position;
                        Vector p2 = player.Pixel.Position + Bullet.Velocity.Normalized;
                        while ((p1 - player.Pixel.Position).Length <= 6 && !foundSomething)
                        {
                            for (int ii = 0; ii < Land.Border.Count; ii++)
                            {
                                for (int jj = 0; jj < Land.Border[ii].Count; jj++)
                                {
                                    Intersection intersection2 = new Intersection(p1, p2, Land.Border[ii][jj], Land.Border[ii][mod(jj + 1, Land.Border[ii].Count)]);

                                    if (intersection2.IsTrue)
                                    {
                                        foundSomething = true;

                                        if ((intersection.Point - Land.Border[ii][jj]).Length < (intersection.Point - Land.Border[ii][mod(jj + 1, Land.Border[ii].Count)]).Length)
                                        {
                                            player.PositionIndex[0] = ii;
                                            player.PositionIndex[1] = jj;
                                        }

                                        else
                                        {
                                            player.PositionIndex[0] = ii;
                                            player.PositionIndex[1] = mod(jj + 1, Land.Border[ii].Count);
                                        }
                                    }
                                }
                            }

                            p1 += Bullet.Velocity.Normalized;
                            p2 += Bullet.Velocity.Normalized;
                        }

                        player.Health -= 50;

                        foreach (var playerB in Players)
                        { 
                            if (player != playerB)
                            {
                                double damage = 0;
                                double dist = (playerB.Position + playerB.Normal - Bullet.Position).Length;

                                if (dist <= 3)
                                {
                                    damage = 30;
                                }

                                if ((dist > 3) && (dist <= 10))
                                {
                                    damage = -6.0 * dist + 38.0;
                                }

                                playerB.Health -= damage;
                            }
                            
                        }

                        return;
                    }
                }
            }              
        }
        */

        private void CheckCollisionBulletLand()
        {
            Intersection intersection = Map.CheckForHit(new Segment(Bullet.PositionBefore, Bullet.Position), 5);
            
            if (intersection.IsTrue)
            {
                MatchAudioSettings.BulletExploded = true;
                Bullet.IsExploded = true;
                
                foreach (var player in Players)
                {
                    double damage = 0;
                    double dist = (player.Pixel.Position + player.Pixel.Normal - Bullet.Position).Length;

                    if (dist <= 4)
                    {
                        damage = 50;
                    }

                    if ((dist > 4) && (dist <= 6))
                    {
                        damage = -22.5 * dist + 140;
                    }

                    player.Health -= damage;
                }
            }
        }
        
        private void CheckCollisionGrenadeLand()
        {
            for (int i = 0; i < Land.Border.Count; i++)
            {                 
                for (int j = 0; j < Land.Border[i].Count; j++)
                {
                    Vector p0 = Land.Border[i][mod(j - 1, Land.Border[i].Count)];
                    Vector p1 = Land.Border[i][j];
                    Vector p2 = Land.Border[i][mod(j + 1, Land.Border[i].Count)];
                    Vector p3 = Land.Border[i][mod(j + 2, Land.Border[i].Count)];

                    Intersection intersection = new Intersection(Grenade.Position, Grenade.PositionBefore, p1, p2);

                    if (intersection.IsTrue)
                    {
                        var n = new Vector();
                        var n0 = new Vector();
                        var n1 = new Vector();
                        var n2 = new Vector();
                        if ((p1 - intersection.Point).Length < (p2 - intersection.Point).Length)
                        {
                            n0 = new Vector((p1 - p0).Y, -(p1 - p0).X).Normalized;
                            n1 = new Vector((p2 - p1).Y, -(p2 - p1).X).Normalized;
                            
                            n = (n0 + n1).Normalized;
                        }
                        else
                        {
                            n1 = new Vector((p2 - p1).Y, -(p2 - p1).X).Normalized;
                            n2 = new Vector((p3 - p2).Y, -(p3 - p2).X).Normalized;

                            n = (n1 + n2).Normalized;
                        }

                        Grenade.Bounce(intersection.Point, n);

                        return;
                    }
                }
            }


        }

        private void CheckCollisionBulletScreen()
        {
            if (Bullet.Position.X < 0 || Bullet.Position.X > 320 || Bullet.Position.Y > 200)
            {
                Bullet.IsExploded = true;
            }
            
        }

        private void CheckCollisionGrenadeScreen()
        {
            if (Grenade.Position.X < 0 || Grenade.Position.X > 320 || Grenade.Position.Y > 200)
            {
                Grenade.IsDead = true;
            }

        }

        private void CheckGrenadeExplosion()
        {
            if (Grenade.IsExploded)
            {
                Land.makeCaldera(Grenade.Position, 10);
                MatchAudioSettings.GrenadeExploded = true;

                foreach (var player in Players)
                {
                    double damage = 0;
                    double dist = (player.Pixel.Position - Grenade.Position).Length;

                    if (dist <= 5)
                    {
                        damage = 75;
                    } 

                    if ((dist > 5) && (dist <= 17))
                    {
                        damage = -6 * dist + 105;
                    }
                    
                    player.Health -= damage;
                }
            }
        }

        private int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

    }
}
