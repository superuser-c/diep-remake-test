using GameLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tanciky {
    [Serializable]
    public class Player : PlayerBase {
        [JsonIgnore]
        [NonSerialized]
        protected Game game;
        private float RegenSpeed;
        public float MaxHP { get; set; }
        public float HP { get; set; }
        [JsonIgnore]
        [NonSerialized]
        public float BodyDmg;
        [JsonIgnore]
        [NonSerialized]
        public float BodyDmgPl;
        [JsonIgnore]
        [NonSerialized]
        public float BulletSpeed;
        [JsonIgnore]
        [NonSerialized]
        public float BulletHP;
        [JsonIgnore]
        [NonSerialized]
        public float BulletDmg;
        [JsonIgnore]
        [NonSerialized]
        public float ReloadTime;
        [JsonIgnore]
        [NonSerialized]
        public float Movespeed;
        [JsonIgnore]
        [NonSerialized]
        public float DamageReduction;
        [JsonIgnore]
        [NonSerialized]
        public float Recoil = 0.2f;
        [JsonIgnore]
        [NonSerialized]
        public float KnockbackResistance = 0;
        public int level { get; set; }
        public int score { get; set; }
        public int skillPts = 0;
        public int regst = 0;
        public int mhpst = 0;
        public int rdst = 0;
        public int bsst = 0;
        public int bpst = 0;
        public int bdst = 0;
        public int brst = 0;
        public int msst = 0;
        [JsonIgnore]
        [NonSerialized]
        public int regb = 0;
        [JsonIgnore]
        [NonSerialized]
        public int mhpb = 0;
        [JsonIgnore]
        [NonSerialized]
        public int rdb = 0;
        [JsonIgnore]
        [NonSerialized]
        public int bsb = 0;
        [JsonIgnore]
        [NonSerialized]
        public int bpb = 0;
        [JsonIgnore]
        [NonSerialized]
        public int bdb = 0;
        [JsonIgnore]
        [NonSerialized]
        public int brb = 0;
        [JsonIgnore]
        [NonSerialized]
        public int msb = 0;
        [JsonIgnore]
        [NonSerialized]
        public float SizeBonus = 1;
        public float size { get; set; } = 0;
        public float rotation { get; set; } = 0;
        public List<Cannon> cannons { get; set; } = new List<Cannon>();
        public List<SmasherHull> hulls { get; set; } = new List<SmasherHull>();
        public Color col;
        [JsonIgnore]
        [NonSerialized]
        public bool LastShoot = false;
        protected bool AutoFire = false;
        protected bool AutoSpin = false;
        public string type;
        [JsonIgnore]
        [NonSerialized]
        public int tier;
        protected bool EPress = false;
        protected bool CPress = false;
        public float Invis { get; set; } = 0;
        public float InvisTime { get; set; } = 0;
        [JsonIgnore]
        [NonSerialized]
        public List<Drone> drones = new List<Drone>();
        float timeIdle = 0;
        [JsonIgnore]
        [NonSerialized]
        public int ID;
        [JsonIgnore]
        public Keys upblock;
        [JsonIgnore]
        public Keys upblock2;

        public Player(int lvl, Color c, int id) {
            ID = id;
            level = 1;
            for (int i = 0; i < lvl - 1; i++) {
                LevelUp();
            }
            score = Scoring.ScoreByLvl(lvl);
            size = 20 * (float)Math.Pow(1.01f, (level - 1));
            cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.05f, 0.4f, 0.4f, 1.5f,
                1, 1, 1, 1, 1, 0));
            col = c;
            type = "tank";
            tier = 0;
        }

        public void Init(Game gam) {
            game = gam;
            foreach (Cannon c in cannons)
                c.Init(gam, this);
            regst = 0;
            mhpst = 0;
            rdst = 0;
            bsst = 0;
            bpst = 0;
            bdst = 0;
            brst = 0;
            msst = 0;
            OnUpgrade();
            HP = MaxHP;
        }

        public override void Update() {
            TestDie();
            Controls();
            timeIdle += Game.Dt;
            pos += vel;
            if (pos.X > 2000)
                pos.X = 2000;
            if (pos.X < -2000)
                pos.X = -2000;
            if (pos.Y > 2000)
                pos.Y = 2000;
            if (pos.Y < -2000)
                pos.Y = -2000;
            vel *= 0.9f;
            if (Invis > 0) {
                if (InvisTime > 0)
                    InvisTime -= Game.Dt;
            }
            if (HP < MaxHP) {
                HP += RegenSpeed * Game.Dt;
                if (timeIdle > 30)
                    HP += RegenSpeed * 100 * Game.Dt;
            } else if (HP > MaxHP)
                HP = MaxHP;
            foreach (Cannon c in cannons) {
                c.Update();
            }
            foreach (SmasherHull h in hulls) {
                h.Update();
            }
            foreach (Bullet b in game.bullets) {
                if (b.owner.col == col)
                    continue;
                if (b.pos.SqDist(pos) < (size + b.size) * (size + b.size)) {
                    b.penetration -= BodyDmg;
                    HP -= b.damage * DamageReduction;
                    b.vel += (b.pos - pos) * 0.03f;
                    vel -= (b.pos - pos) * 0.02f * (1 - KnockbackResistance);
                    InvisTime = Invis;
                    if (HP <= 0) {
                        b.owner.GainExp(score > 23536 ? 23537 : score);
                    }
                    timeIdle = 0;
                }
            }
            foreach (Shape s in game.shapes) {
                if (s.pos.SqDist(pos) < (size + s.size) * (size + s.size)) {
                    s.HP -= BodyDmg * s.DamageReduction;
                    HP -= s.BodyDmg * DamageReduction;
                    s.vel += (s.pos - pos) * 0.02f * s.KnockbackResist;
                    vel -= (s.pos - pos) * 0.02f * (1 - KnockbackResistance);
                    s.LastHit = this;
                    InvisTime = Invis;
                    timeIdle = 0;
                }
            }
            foreach (Player pl in game.players) {
                if (pl.col != col &&
                    pl.pos.SqDist(pos) < (size + pl.size) * (size + pl.size)) {
                    pl.HP -= BodyDmg * pl.DamageReduction;
                    HP -= pl.BodyDmg * DamageReduction;
                    pl.vel += (pl.pos - pos) * 0.02f * (1 - pl.KnockbackResistance);
                    vel -= (pl.pos - pos) * 0.02f * (1 - KnockbackResistance);
                    InvisTime = Invis;
                    if (HP <= 0) {
                        pl.GainExp(score > 23536 ? 23537 : score);
                    }
                    timeIdle = 0;
                }
            }
        }
        
        public void Reset() {
            cannons.Clear();
            hulls.Clear();
            Invis = 0;
            for (int i = 0; i < drones.Count; i++) {
                game.bullets.Remove(drones[i]);
                drones.RemoveAt(i);
                i--;
            }
        }

        public void GainExp(int exp) {
            score += exp;
            while (Scoring.NextLvl(level, score))
                LevelUp();
        }

        public void OnUpgrade() {
            MaxHP = 50 + (level - 1) * 2 + 20 * (mhpst + mhpb);
            RegenSpeed = 1 / 30f * MaxHP * (0.03f + 0.12f * (regst + regb));
            BodyDmg = (rdst + rdb) * 4 + 20;
            BodyDmgPl = (rdst + rdb) * 6 + 30;
            BulletSpeed = 14 + (bsst + bsb) * 1.5f;
            BulletHP = (float)Math.Pow(2, (bpst + bpb - 1)) + 4;
            BulletDmg = 7 + 3 * (bdst + bdb);
            ReloadTime = 0.6f - (brst + brb) * 0.04f;
            DamageReduction = 1 - 4f / (10f + 2f * (rdst + rdb));
            size = 20 * (float)Math.Pow(1.01f, (level - 1)) * SizeBonus;
            Movespeed = ((msst + msb) * 60 + 100) / size + 5;
        }

        protected override void TestDie() {
            if (HP < 0)
                died++;
            if (died > 0)
                Respawn();
        }

        protected void LevelUp() {
            level++;
            if (level < 29 || level % 3 == 0)
                skillPts++;
            size = 20 * (float)Math.Pow(1.01f, (level - 1)) * SizeBonus;
            OnUpgrade();
        }

        protected override void Respawn() {
            
        }

        public void Respawn2(int lvl, Color c) {
            Reset();
            died = 0;
            skillPts = 0;
            regst = 0;
            mhpst = 0;
            rdst = 0;
            bsst = 0;
            bpst = 0;
            bdst = 0;
            brst = 0;
            msst = 0;
            regb = 0;
            mhpb = 0;
            rdb = 0;
            bsb = 0;
            bpb = 0;
            bdb = 0;
            brb = 0;
            msb = 0;
            size = 20 * (float)Math.Pow(1.01f, (level - 1));
            cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.05f, 0.4f, 0.4f, 1.5f,
                1, 1, 1, 1, 1, 0));
            col = c;
            type = "tank";
            tier = 0;
            Recoil = 0.2f;
            KnockbackResistance = 0;
            SizeBonus = 1;
            level = 1;
            score = Scoring.ScoreByLvl(lvl);
            for (int i = 0; i < lvl - 1; i++) {
                LevelUp();
            }
        }

        protected override void Controls() {
            if (Form1.press.Contains(Keys.W)) {
                vel.Y -= Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (Form1.press.Contains(Keys.S)) {
                vel.Y += Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (Form1.press.Contains(Keys.A)) {
                vel.X -= Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (Form1.press.Contains(Keys.D)) {
                vel.X += Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (Form1.press.Contains(Keys.E) && !EPress) {
                AutoFire = !AutoFire;
                EPress = true;
            }
            if (EPress && !Form1.press.Contains(Keys.E))
                EPress = false;
            if (Form1.press.Contains(Keys.C) && !CPress) {
                AutoSpin = !AutoSpin;
                CPress = true;
            }
            if (CPress && !Form1.press.Contains(Keys.C))
                CPress = false;
            Vec scrolledpos = new Vec(300, 300) * zoom;
            Vec msp = Game.mousepos * (size / 20f);
            if (AutoSpin) {
                msp = new Vec(0, 100);
                msp.RotateByZero(-rotation + Game.Dt);
                msp += scrolledpos;
            }
            rotation = scrolledpos.Angle(msp);
            if (msp.X < scrolledpos.X)
                rotation = -rotation;
            if (msp.Y < scrolledpos.Y)
                rotation = (float)Math.PI - rotation;
            if (Game.mousedown || AutoFire) {
                if (!LastShoot)
                    foreach (Cannon c in cannons)
                        c.AddReload();
                LastShoot = true;
                foreach (Cannon c in cannons) {
                    c.TryShoot();
                }
            } else {
                LastShoot = false;
            }
        }

        public Vec Forward() => new Vec(Math.Sin(rotation), Math.Cos(rotation));

        public float zoom {
            get {
                return size / 20f;
            }
        }

        public float drawZoom {
            get {
                return 20f / size;
            }
        }
    }

    public class AiPlayer : Player {
        public AiPlayer(int lvl, Color c, int id) : base(lvl, c, id) {
        }

        protected override void Controls() {
            /*if (Form1.press.Contains(Keys.W)) {
                vel.Y -= Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (Form1.press.Contains(Keys.S)) {
                vel.Y += Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (Form1.press.Contains(Keys.A)) {
                vel.X -= Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (Form1.press.Contains(Keys.D)) {
                vel.X += Movespeed * 0.1f;
                InvisTime = Invis;
            }*/
            var q = from pl in game.players
                    where (pl.pos.SqDist(pos) < 160000)
                    orderby pl.pos.SqDist(pos)
                    select pl.pos;
            Vec msp = new Vec();
            if (q.Any()) {
                msp = q.First();
            } else {
                var r = from s in game.shapes
                        where (s.pos.SqDist(pos) < 160000)
                        orderby s.pos.SqDist(pos)
                        select s.pos;
                if (r.Any())
                    msp = r.First();
            }
            rotation = (float)Math.Atan2(msp.X, msp.Y);
            if (Game.mousedown || true) {
                if (!LastShoot)
                    foreach (Cannon c in cannons)
                        c.AddReload();
                LastShoot = true;
                foreach (Cannon c in cannons) {
                    c.TryShoot();
                }
            } else {
                LastShoot = false;
            }
        }
    }

    public class ClientPl : Player {
        public ControlData cdat = new ControlData();

        public ClientPl(int lvl, Color c, int id) : base(lvl, c, id) {

        }

        protected override void Controls() {
            if (cdat.keys[0]) {
                vel.Y -= Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (cdat.keys[3]) {
                vel.Y += Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (cdat.keys[1]) {
                vel.X -= Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (cdat.keys[2]) {
                vel.X += Movespeed * 0.1f;
                InvisTime = Invis;
            }
            if (cdat.keys[4] && !EPress) {
                AutoFire = !AutoFire;
                EPress = true;
            }
            if (EPress && !cdat.keys[4])
                EPress = false;
            if (cdat.keys[5] && !CPress) {
                AutoSpin = !AutoSpin;
                CPress = true;
            }
            if (CPress && !cdat.keys[5])
                CPress = false;
            Vec scrolledpos = new Vec(300, 300);
            Vec msp = cdat.mousep??new Vec() * (size / 20f);
            if (AutoSpin) {
                msp = new Vec(0, 100);
                msp.RotateByZero(-rotation + Game.Dt);
                msp += scrolledpos;
            }
            rotation = scrolledpos.Angle(msp);
            if (msp.X < scrolledpos.X)
                rotation = -rotation;
            if (msp.Y < scrolledpos.Y)
                rotation = (float)Math.PI - rotation;
            if (cdat.mouseb[0] || AutoFire) {
                if (!LastShoot)
                    foreach (Cannon c in cannons)
                        c.AddReload();
                LastShoot = true;
                foreach (Cannon c in cannons) {
                    c.TryShoot();
                }
            } else {
                LastShoot = false;
            }
        }
    }
}
