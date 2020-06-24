using GameLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanciky {
    [Serializable]
    public class Bullet {
        [JsonIgnore]
        [NonSerialized]
        public Player owner;
        [JsonIgnore]
        [NonSerialized]
        public Game game;
        public string type = "bullet";
        public Color col;
        public Vec pos;
        public Vec vel { get; set; }
        [JsonIgnore]
        [NonSerialized]
        public float lifetime;
        [JsonIgnore]
        [NonSerialized]
        public float damage;
        [JsonIgnore]
        [NonSerialized]
        public float penetration;
        public float size;
        [JsonIgnore]
        [NonSerialized]
        public bool destroyed;

        public Bullet(float dmg, float pntr, Vec v, Vec p, float s) {
            lifetime = 5;
            damage = dmg;
            penetration = pntr;
            destroyed = false;
            if (v == null)
                v = new Vec();
            vel = v * 1.3f;
            pos = p;
            size = s;
        }

        public void Init(Game gam, Player pl) {
            game = gam;
            owner = pl;
        }

        public virtual void Update() {
            pos += vel;
            vel *= 0.98f;
            col = owner.col;
            if (this is Drone) {
                if (penetration <= 0)
                    destroyed = true;
            } else {
                lifetime -= Game.Dt;
                if (lifetime < 0 || penetration <= 0)
                    destroyed = true;
            }
            foreach (Bullet b in game.bullets) {
                if (b.pos == pos)
                    continue;
                if (b.pos.SqDist(pos) < (size + b.size) * (size + b.size)) {
                    if (b.col != col) {
                        b.penetration -= damage;
                        penetration -= b.damage;
                    }
                    if (b is Drone ||
                        (b is Trap &&
                            ((b.lifetime > 28 && lifetime > 28) ||
                            b.owner.col != owner.col))) {
                        b.vel += (b.pos - pos) * 0.05f;
                        b.pos += (b.pos - pos) * 0.1f;
                    }
                    if (this is Drone ||
                        (this is Trap &&
                            ((lifetime > 28 && b.lifetime > 28) ||
                            b.owner.col != owner.col))) {
                        vel -= (b.pos - pos) * 0.05f;
                        pos -= (b.pos - pos) * 0.1f;
                    }
                }
            }
        }
    }

    [Serializable]
    public class Drone : Bullet {
        [JsonIgnore]
        [NonSerialized]
        public Vec target;
        [JsonIgnore]
        [NonSerialized]
        public bool outFrom;
        [JsonIgnore]
        [NonSerialized]
        public Spawner control;

        public Drone(float dmg, float pntr, Vec vel, Vec pos, float size,
            Spawner c) :
            base(dmg, pntr, vel, pos, size) {
            control = c;
            if (c != null) {
                type = "drone";
                if (c.owner.type == "necromancer")
                    type = "necrodrone";
            }
        }

        public override void Update() {
            base.Update();
            if (pos.X > 2000)
                pos.X = 2000;
            if (pos.X < -2000)
                pos.X = -2000;
            if (pos.Y > 2000)
                pos.Y = 2000;
            if (pos.Y < -2000)
                pos.Y = -2000;
            vel *= 0.7f;
            if (target == null) {
                var q = from s in game.shapes
                        where (s.pos.SqDist(owner.pos) < 90000)
                        orderby s.pos.SqDist(pos)
                        select s.pos;
                if (q.Any()) {
                    target = q.First().Copy();
                } else if (owner.pos.SqDist(pos) > 5000) {
                    target = owner.pos.Copy();
                } else {
                    Vec orbit = pos - owner.pos;
                    orbit.RotateByZero(Game.Dt);
                    target = orbit + owner.pos;
                }
            }
            vel += (target - pos).Normalized() * (outFrom ? -1 : 1) *
                   owner.BulletSpeed;
        }
    }

    [Serializable]
    public class Partisan : Drone {
        public Partisan(float dmg, float pntr, Vec vel, Vec pos, 
            float size, Spawner c) : base(dmg, pntr, vel, pos, size, c) {
            lifetime = 4;
        }

        public override void Update() {
            base.Update();
            lifetime -= Game.Dt;
            if (lifetime < 0 || penetration <= 0)
                destroyed = true;
        }
    }

    [Serializable]
    public class Trap : Bullet {
        public Trap(float dmg, float pntr, Vec v, Vec p, float s) 
            : base(dmg, pntr, v, p, s) {
            lifetime = 30;
            type = "trap";
        }

        public override void Update() {
            base.Update();
            vel *= 0.9f;
        }
    }

    [Serializable]
    public class TankDrone : Drone {
        public TankDrone(float dmg, float pntr, Vec vel, Vec pos, float size, Spawner c) : base(dmg, pntr, vel, pos, size, c) {
            type = "tankdrone";
        }


    }
}
