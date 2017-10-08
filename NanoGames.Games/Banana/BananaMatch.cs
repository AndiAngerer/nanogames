// Copyright (c) the authors of nanoGames. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
        private int framesMax = 1800;
        public string StateOfGame = "ActivePlayerActing";
        public BananaPlayer ActivePlayer;
        public int StartPlayerIdx = 0;
        private int activePlayerIdx = 0;
        public Landscape Land = new Landscape();
        public Bullet Bullet = new Bullet();
        public Grenade Grenade = new Grenade();
        public Wind Wind = new Wind();
        public AudioSettings MatchAudioSettings = new AudioSettings();

        private int finishedPlayers = 0;
        private bool somethingFlying = false;

        protected override void Initialize()
        {
            Land.BuildLandscape("Blocks");

            foreach (var player in Players)
            {
                player.GetBorn();
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
                    
                    // player falling/flying
                    somethingFlying = false;

                    foreach (var player in Players)
                    {
                        if (player.IsFalling)
                        {
                            player.Fall();
                            CheckCollisionPlayerLand(player);
                            CheckCollisionPlayerScreen(player);
                            somethingFlying = true;
                        }
                    }

                    // bullet flying
                    if (!Bullet.IsExploded)
                    {
                        Bullet.MoveBullet(Wind);
                        CheckCollisionBulletLand();                  
                        CheckCollisionBulletPlayers();              
                        CheckCollisionBulletScreen();                
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
                        Land.makeCaldera(intersection.Point, 8);
                        player.Health -= 50;

                        foreach (var playerB in Players)
                        {
                            CheckIfPlayerIsFalling(playerB);
                            RecalculatePlayerPositionIndex(playerB);
                        }

                        return;
                    }
                }
            }              
        }

        private void CheckCollisionBulletLand()
        {
            foreach (List<Vector> block in Land.Border)
            {
                for (int j = 0; j < block.Count; j++)
                {
                    Intersection intersection = new Intersection(Bullet.Position, Bullet.PositionBefore, block[j], block[mod(j + 1, block.Count)]);

                    if (intersection.IsTrue)
                    {
                        MatchAudioSettings.BulletExploded = true;
                        Bullet.IsExploded = true;
                        Land.makeCaldera(intersection.Point, 8);

                        foreach (var player in Players)
                        {
                            CheckIfPlayerIsFalling(player);
                            RecalculatePlayerPositionIndex(player);
                        }
                        
                        return;
                    }
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
                Land.makeCaldera(Grenade.Position, 15);
                MatchAudioSettings.GrenadeExploded = true;

                foreach (var player in Players)
                {
                    double damage = 0;
                    double dist = (player.Position - Grenade.Position).Length;

                    if (dist <= 5)
                    {
                        damage = 75;
                    } 

                    if ((dist > 5) && (dist <= 17))
                    {
                        damage = -6 * dist + 105;
                    }
                    
                    player.Health -= damage;

                    CheckIfPlayerIsFalling(player);
                    RecalculatePlayerPositionIndex(player);
                }
            }
        }

        private void CheckCollisionPlayerLand(BananaPlayer player)
        {
            for (int i = 0; i < Land.Border.Count; i++)
            {
                for (int j = 0; j < Land.Border[i].Count; j++)
                {
                    Intersection intersection = new Intersection(player.Position, player.PositionBefore, Land.Border[i][j], Land.Border[i][mod(j + 1, Land.Border[i].Count)]);

                    if (intersection.IsTrue)
                    {
                        if ((intersection.Point - Land.Border[i][j]).Length < (intersection.Point - Land.Border[i][mod(j + 1, Land.Border[i].Count)]).Length)
                        {
                            player.Position = Land.Border[i][j];
                            player.PositionIndex[0] = i;
                            player.PositionIndex[1] = j;
                        }

                        else
                        {
                            player.Position = Land.Border[i][mod(j + 1, Land.Border[i].Count)];
                            player.PositionIndex[0] = i;
                            player.PositionIndex[1] = j + 1;
                        }

                        player.UpdateHitbox();
                        player.IsFalling = false;
                        player.Health -= player.Velocity.Length * 10;
                        player.Velocity = new Vector(0, 0);
                        MatchAudioSettings.PlayerHitGround = true;

                        return;
                    }
                }
            }
        }

        private void CheckCollisionPlayerScreen(BananaPlayer player)
        {
            if (player.Position.X < 0 || player.Position.X > 320 || player.Position.Y > 200)
            {
                player.Health = 0;
                player.IsFalling = false;
            }

        }

        private void CheckIfPlayerIsFalling(BananaPlayer player)
        {
            for (int i = 0; i < Land.Border.Count; i++)
            {
                for (int j = 0; j < Land.Border[i].Count; j++)
                {
                    if (player.Position == Land.Border[i][j])
                    {
                        return;
                    }
                }
            }

            // player is hovering in air
            player.IsFalling = true;
        }

        public void RecalculatePlayerPositionIndex(BananaPlayer player)
        {
            for (int i = 0; i < Land.Border.Count; i++)
            {
                for (int j = 0; j < Land.Border[i].Count; j++)
                {
                    Vector p = Land.Border[i][j];

                    double epsilon = 0.000001;
                    if ((p-player.Position).Length < epsilon)
                    {
                        player.PositionIndex[0] = i;
                        player.PositionIndex[1] = j;
                        player.Position = p;

                        return;
                    }
                }
            }

        }

        private int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

    }
}
