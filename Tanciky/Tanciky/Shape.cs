using GameLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanciky {
    [Serializable]
    public class Shape {
        [NonSerialized]
        Game game;
        public float HPMax;
        public float HP;
        public Vec pos;
        public Vec vel;
        public float size;
        public string type;
        [NonSerialized]
        public float BodyDmg;
        [NonSerialized]
        public float DamageReduction;
        [NonSerialized]
        public Player LastHit = null;
        [NonSerialized]
        public float KnockbackResist;

        public Shape(string t, Vec p) {
            if (t == "alpha pentagon") {
                HPMax = 3000;
                size = 120;
                BodyDmg = 20;
                DamageReduction = 0.6f;
                KnockbackResist = 0;
            } else if(t == "pentagon") {
                HPMax = 100;
                size = 32;
                BodyDmg = 12;
                DamageReduction = 2 / 3f;
                KnockbackResist = 1;
            } else if (t == "triangle") {
                HPMax = 30;
                size = 20;
                BodyDmg = 8;
                DamageReduction = 1;
                KnockbackResist = 1;
            } else {
                HPMax = 10;
                size = 16;
                BodyDmg = 8;
                DamageReduction = 1;
                KnockbackResist = 1;
            }
            type = t;
            HP = HPMax;
            pos = p;
            vel = new Vec(
                Game.rnd.NextDouble() * 2 - 1, 
                Game.rnd.NextDouble() * 2 - 1) * 0.1f;
        }

        public void Init(Game gam) {
            game = gam;
        }

        public void Update() {
            pos += vel;
            vel *= 0.9f;
            if (pos.X > 2000)
                pos.X = 2000;
            if (pos.X < -2000)
                pos.X = -2000;
            if (pos.Y > 2000)
                pos.Y = 2000;
            if (pos.Y < -2000)
                pos.Y = -2000;
            foreach (Bullet b in game.bullets) {
                if (b.pos.SqDist(pos) < (size + b.size) * (size + b.size)) {
                    b.penetration -= BodyDmg;
                    HP -= b.damage * DamageReduction;
                    b.vel += (b.pos - pos) * 0.03f;
                    vel -= (b.pos - pos) * 0.02f * KnockbackResist;
                    LastHit = b.owner;
                }
            }
            foreach (Shape s in game.shapes) {
                if (s.pos == pos)
                    continue;
                if (s.pos.SqDist(pos) < (size + s.size) * (size + s.size)) {
                    s.vel += (s.pos - pos) * 0.02f;
                    vel -= (s.pos - pos) * 0.02f;
                }
            }
        }

        public void OnDestroy() {
            if (LastHit != null) {
                if (type == "alpha pentagon")
                    LastHit.GainExp(3000);
                if (type == "pentagon")
                    LastHit.GainExp(130);
                if (type == "triangle")
                    LastHit.GainExp(25);
                if (type == "square") {
                    LastHit.GainExp(10);
                    if (LastHit.type == "necromancer" && 
                        LastHit.drones.Count < LastHit.brst * 2 + 22) {
                        LastHit.drones.Add(new Drone(
                            LastHit.BulletDmg,
                            LastHit.BulletHP * 8,
                            new Vec(),
                            pos.Copy(),
                            size * 1.5f,
                            LastHit.cannons[0] as Spawner));
                        game.bullets.Add(LastHit.drones[LastHit.drones.Count - 1]);
                        LastHit.drones.Last().Init(game, LastHit);
                    }
                }
            }
        }

        public static Shape NRandS() {
            Vec pos = new Vec(Game.rnd.Next(4000) - 2000, 
                Game.rnd.Next(4000) - 2000);
            if (Game.rnd.Next(500) < 1) {
                return new Shape("alpha pentagon", pos);
            }
            if (Game.rnd.Next(80) < 1) {
                return new Shape("pentagon", pos);
            }
            if (Game.rnd.Next(16) < 1) {
                return new Shape("triangle", pos);
            }
            return new Shape("square", pos);
        }
    }
}
