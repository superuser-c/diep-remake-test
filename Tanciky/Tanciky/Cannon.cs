using GameLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tanciky {
    [Serializable]
    public class Cannon {
        public float rotpos;
        public float ownersiz;
        private Vec offst;
        public Vec offset {
            get { return new Vec(offst.X, offst.Y * ownersiz); }
            set { offst = value; }
        }
        [JsonIgnore]
        [NonSerialized]
        public Game game;
        [JsonIgnore]
        [NonSerialized]
        public Player owner;
        public float spread;
        private float insz;
        public float insize {
            get { return insz * ownersiz; }
            set { insz = value; }
        }
        private float outsz;
        public float outsize {
            get { return outsz * ownersiz; }
            set { outsz = value; }
        }
        private float censz;
        public float censize {
            get { return censz * ownersiz; }
            set { censz = value; }
        }
        private float clen;
        public float clenght {
            get { return clen * ownersiz; }
            set { clen = value; }
        }
        private float len;
        public float lenght {
            get { return len * ownersiz; }
            set { len = value; }
        }
        public float SpeedMult;
        public float HPMult;
        public float DmgMult;
        public float ReloadMult;
        public float RecoilMult;
        protected float Reload;
        float ReloadAdd;

        public Cannon(float r, Vec o, float s, float si, float so, float l,
            float sm, float hm, float dm, float rm, float rcm, float ra,
            float cs = 0, float cl = 0) {
            rotpos = r;
            offset = o;
            spread = s;
            insize = si;
            outsize = so;
            lenght = l;
            SpeedMult = sm;
            HPMult = hm;
            DmgMult = dm;
            ReloadMult = rm;
            RecoilMult = rcm;
            ReloadAdd = ra;
            censize = cs;
            clenght = cl;
        }
        
        public void Init(Game gam, Player o) {
            game = gam;
            owner = o;
            ownersiz = owner.size;
        }

        public virtual void Shoot() {
            Vec vel = new Vec(0, 1);
            vel.RotateByZero(rotpos - owner.rotation + GSpread());
            Vec p = offset;
            p.X *= owner.size;
            p.RotateByZero(rotpos - owner.rotation);
            p += owner.pos.Copy();
            game.bullets.Add(new Bullet(
                owner.BulletDmg * DmgMult,
                owner.BulletHP * HPMult,
                vel * owner.BulletSpeed,
                p,
                Math.Min(insize, outsize)));
            game.bullets[game.bullets.Count - 1].Init(game, owner);
            owner.vel += RecoilMult * (Vec.Zero - vel) * owner.Recoil;
        }

        public virtual void Update() {
            if (Reload >= 0)
                Reload -= Game.Dt;
            ownersiz = owner.size;
        }

        public virtual void TryShoot() {
            if (Reload < 0) {
                owner.InvisTime = owner.Invis;
                Shoot();
                Reload += ReloadMult * owner.ReloadTime;
            }
        }

        protected float GSpread() {
            return (float)Game.rnd.NextDouble() * spread * 2 - spread;
        }

        public void AddReload() {
            Reload += ReloadAdd * ReloadMult * owner.ReloadTime;
        }
    }

    [Serializable]
    public class SmasherHull {
        [NonSerialized]
        Game game;
        protected float ownersiz;
        [NonSerialized]
        protected Player owner;
        protected float rot;
        float rs;
        public float size;

        public SmasherHull(float size, float rotSpeed) {
            rot = 0;
            rs = rotSpeed;
            this.size = size;
        }

        public void Update() {
            ownersiz = owner.size;
            rot += Game.Dt * rs;
        }

        public void Init(Game gam, Player o) {
            game = gam;
            owner = o;
            ownersiz = owner.size;
        }

        public virtual PointF[] GetPts(Vec pos, float zoom) {
            PointF[] o = new PointF[6];
            for (int i = 0; i < o.Length; i++) {
                Vec v = new Vec(0, 1);
                v.RotateByZero(i * (float)Math.PI / 3 + rot);
                v *= ownersiz * size;
                o[i] = (v + pos) * zoom;
            }
            return o;
        }
    }

    [Serializable]
    public class Spawner : Cannon {
        [NonSerialized]
        protected bool control = false;
        [NonSerialized]
        public bool repel = false;
        [NonSerialized]
        int maxDroneC;

        public Spawner(float r, Vec o, float s, float si, float so, float l, 
            float sm, float hm, float dm, float rm, float rcm, float ra, 
            int droneControl, float cs = 0, float cl = 0) : base(r, o, s, si, 
            so, l, sm, hm, dm, rm, rcm, ra, cs, cl) {
            maxDroneC = droneControl;
        }

        public override void Shoot() {
            Reload += ReloadMult * owner.ReloadTime;
            Vec vel = new Vec(0, 1);
            vel.RotateByZero(rotpos - owner.rotation);
            Vec p = offset;
            p.X *= owner.size;
            p.RotateByZero(rotpos - owner.rotation);
            p += owner.pos.Copy();
            owner.drones.Add(new Drone(
                owner.BulletDmg * DmgMult,
                owner.BulletHP * HPMult,
                vel * 3,
                p,
                outsize,
                this));
            game.bullets.Add(owner.drones[owner.drones.Count - 1]);
            owner.drones.Last().Init(game, owner);
        }

        public override void Update() {
            base.Update();
            repel = Game.rmousedown;
            if (owner is ClientPl clp) {
                repel = clp.cdat.mouseb[1];
            }
            if (owner.type != "necromancer" && 
                owner.drones.Count < maxDroneC && Reload <= 0) {
                Shoot();
            }
            Vec msp = (Game.mousepos) * (owner.size / 20f) + game.scroll;
            if (owner is ClientPl clpl) {
                msp = (clpl.cdat.mousep + new Vec(300, 300)) * (owner.size / 20f);
            }
            if (!(control || repel))
                msp = null;
            if (maxDroneC < 3)
                msp = null;
            foreach (Drone d in owner.drones) {
                d.target = msp;
                d.outFrom = repel;
            }
            control = false;
        }

        public override void TryShoot() {
            control = true;
        }
    }

    [Serializable]
    public class PartisanSpawner : Spawner {
        public PartisanSpawner(float r, Vec o, float s, float si, float so, 
            float l, float sm, float hm, float dm, float rm, float rcm, 
            float ra, bool c, float cs = 0, float cl = 0) : 
            base(r, o, s, si, so, l, sm, hm, dm, rm, rcm, ra, 1000, 
                cs, cl) {
            control = c;
        }

        public override void TryShoot() {
            if (Reload <= 0) {
                Shoot();
            }
        }

        public override void Shoot() {
            Reload += ReloadMult * owner.ReloadTime;
            Vec vel = new Vec(0, 1);
            vel.RotateByZero(rotpos - owner.rotation);
            Vec p = offset;
            p.X *= owner.size;
            p.RotateByZero(rotpos - owner.rotation);
            p += owner.pos.Copy();
            owner.drones.Add(new Partisan(
                owner.BulletDmg * DmgMult,
                owner.BulletHP * HPMult,
                vel * owner.BulletSpeed,
                p,
                outsize,
                this));
            game.bullets.Add(owner.drones[owner.drones.Count - 1]);
            owner.drones.Last().Init(game, owner);
        }

        public override void Update() {
            if (Reload >= 0)
                Reload -= Game.Dt;
            repel = Game.rmousedown;
            if (owner is ClientPl clp) {
                repel = clp.cdat.mouseb[1];
            }
            Vec msp = (Game.mousepos) * (owner.size / 20f) + game.scroll;
            if (owner is ClientPl clpl) {
                msp = (clpl.cdat.mousep + new Vec(300, 300)) * (owner.size / 20f);
            }
            if (!control)
                msp = null;
            foreach (Drone d in owner.drones) {
                if (d.control != this)
                    continue;
                d.target = msp;
                if (control) {
                    d.outFrom = repel;
                }
            }
        }
    }

    [Serializable]
    public class SpikeHull : SmasherHull {
        public SpikeHull(float size, float rotSpeed) : base(size, rotSpeed) {

        }

        public override PointF[] GetPts(Vec pos, float zoom) {
            PointF[] o = new PointF[24];
            for (int i = 0; i < o.Length; i++) {
                Vec v = new Vec(0, 1);
                v.RotateByZero(i * (float)Math.PI / 12 + rot);
                v *= ownersiz * (i % 2 == 0 ? size : 1);
                o[i] = (v + pos) * zoom;
            }
            return o;
        }
    }

    [Serializable]
    public class Launcher : Cannon {
        public Launcher(float r, Vec o, float s, float si, float so, float l,
            float sm, float hm, float dm, float rm, float rcm, float ra, 
            float cs = 0, float cl = 0) : 
            base(r, o, s, si, so, l, sm, hm, dm, rm, rcm, ra, cs, cl) {
        }

        public override void Shoot() {
            Vec vel = new Vec(0, 1);
            vel.RotateByZero(rotpos - owner.rotation + GSpread());
            Vec p = offset;
            p.X *= owner.size;
            p.RotateByZero(rotpos - owner.rotation);
            p += owner.pos.Copy();
            game.bullets.Add(new Trap(
                owner.BulletDmg * DmgMult,
                owner.BulletHP * HPMult,
                vel * owner.BulletSpeed,
                p,
                Math.Min(insize, outsize)));
            game.bullets[game.bullets.Count - 1].Init(game, owner);
            owner.vel += RecoilMult * (Vec.Zero - vel) * owner.Recoil * 0.1f;
        }
    }

    public class OffsetAutoCannon : Cannon {
        public float rotation = 0;
        public bool targetmouse = false;

        public OffsetAutoCannon(float r, Vec o, float s, float si, float so,
            float l, float sm, float hm, float dm, float rm, float rcm) :
            base(r, o, s, si, so, l, sm, hm, dm, rm, rcm, 0, 0, 0) {
            
        }

        protected float Mod(double a, double b) {
            if (b > a) {
                a = a + b;
                b = a - b;
                a = a - b;
            }
            return (float)(a - Math.Floor(a / b) * b);
        }

        public override void Update() {
            base.Update();
            Vec target;
            Vec c = offset;
            c.RotateByZero(rotpos);
            c += owner.pos;
            Vec msp = Game.mousepos * (owner.size / 20f) + game.scroll;
            float da = (float)Math.Atan2(msp.X - c.X, msp.Y - c.Y) - rotpos;
            da += (float)(Mod((da + Math.PI), (Math.PI * 2)) - Math.PI);
            if (targetmouse && Math.Abs(da) < Math.PI / 2) { 
                ShootAt(msp);
            } else if (offset.X == 0 && offset.Y == 0) {
                var q = from s in game.shapes
                        where (s.pos.SqDist(c) < 90000)
                        orderby s.pos.SqDist(owner.pos)
                        select s.pos;
                if (q.Any()) {
                    target = q.First().Copy();
                    ShootAt(target);
                }
            } else {
                var q = from s in game.shapes
                        where (s.pos.SqDist(c) < 90000 && 
                            Math.Abs(Angle(c, s.pos) - rotpos) < Math.PI / 2)
                        orderby s.pos.SqDist(owner.pos)
                        select s.pos;
                if (q.Any()) {
                    target = q.First().Copy();
                    ShootAt(target);
                }
            }
            targetmouse = false;
        }

        protected float Angle(Vec v, Vec target) {
            float a = v.Angle(target);
            if (target.X > v.X)
                a = -a;
            if (target.Y < v.Y)
                a = (float)Math.PI - a;
            return a;
        }

        protected void ShootAt(Vec target) {
            rotation = owner.pos.Angle(target);
            if (target.X > owner.pos.X)
                rotation = -rotation;
            if (target.Y < owner.pos.Y)
                rotation = (float)Math.PI - rotation;
            if (Reload < 0) {
                owner.InvisTime = owner.Invis;
                Shoot();
                Reload += ReloadMult * owner.ReloadTime;
            }
        }

        public override void Shoot() {
            Vec vel = new Vec(0, 1);
            vel.RotateByZero(rotation + GSpread());
            Vec p = offset;
            p.RotateByZero(rotpos);
            p += owner.pos;
            game.bullets.Add(new Bullet(
                owner.BulletDmg * DmgMult,
                owner.BulletHP * HPMult,
                vel * owner.BulletSpeed,
                p,
                Math.Min(insize, outsize)));
            game.bullets[game.bullets.Count - 1].Init(game, owner);
            owner.vel += RecoilMult * (Vec.Zero - vel) * owner.Recoil;
        }

        public override void TryShoot() {
            targetmouse = true;
        }
    }

    [Serializable]
    public class FullAutoCannon : OffsetAutoCannon {
        public FullAutoCannon(float s, float si, float so, float l, float sm,
            float hm, float dm, float rm, float rcm) : 
            base(0, new Vec(), s, si, so, l, sm, hm, dm, rm, rcm) {
        }
    }
}
