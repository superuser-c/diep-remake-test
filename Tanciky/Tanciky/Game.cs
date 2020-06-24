using GameLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tanciky {
    public class Game : IGame {
        public static Random rnd = new Random();

        public static Vec mousepos = new Vec();
        public static bool mousedown = false;
        public static bool rmousedown = false;

        public static float Dt = 0;
        
        public List<Player> players = new List<Player>();
        public List<Bullet> bullets;
        public List<Shape> shapes;

        public int plTrackIndex;

        public Vec scroll = new Vec();
        
        public static Pen stroke = new Pen(Color.Black, 3);
        
        static bool inConsole;

        static readonly float hc = (float)Math.PI;

        public Game() {
            players.Add(new Player(45, Color.DeepSkyBlue, 0));
            players.Last().Init(this);
            //players.Add(new AiPlayer(45, Color.Lime));
            //players.Last().Init(this);
            bullets = new List<Bullet>();
            shapes = new List<Shape>();
        }

        public void SetControlData(int id, ControlData cd) {
            var q = from pl in players
                    where (pl.ID == id)
                    select pl;
            if (q.Any()) {
                Player pl = q.First();
                if (pl is ClientPl cpl) {
                    cpl.cdat = cd;
                }
            }
        }

        public void Update(string scene) {
            Player pltr = players[plTrackIndex];
            foreach (Player pl in players) {
                pl.Update();
                PlUps(pl);
                if (pl.died > 0) {
                    pl.Respawn2(Scoring.RespawnLvl(pl.level), pl.col);
                    pl.Init(this);
                }
            }
            scroll += (pltr.pos - new Vec(300, 300) * pltr.zoom - scroll) * 0.5f;
            for (int i = 0; i < bullets.Count; i++) {
                bullets[i].Update();
                if (bullets[i].destroyed) {
                    if (bullets[i] is Drone) {
                        Drone d = bullets[i] as Drone;
                        d.owner.drones.Remove(d);
                    }
                    bullets.RemoveAt(i);
                    i--;
                }
            }
            if (shapes.Count < 400) {
                shapes.Add(Shape.NRandS());
                shapes[shapes.Count - 1].Init(this);
            }
            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].Update();
                if (shapes[i].HP <= 0) {
                    shapes[i].OnDestroy();
                    shapes.RemoveAt(i);
                    i--;
                }
            }
        }

        public void PlUps(Player p) {
            if (p.skillPts > 0) {
                int psp = p.skillPts;
                TryUp(Keys.D1, p.regst, out p.regst, p, 6);
                TryUp(Keys.D2, p.mhpst, out p.mhpst, p, 7);
                TryUp(Keys.D3, p.rdst, out p.rdst, p, 8);
                TryUp(Keys.D4, p.bsst, out p.bsst, p, 9);
                TryUp(Keys.D5, p.bpst, out p.bpst, p, 10);
                TryUp(Keys.D6, p.bdst, out p.bdst, p, 11);
                TryUp(Keys.D7, p.brst, out p.brst, p, 12);
                TryUp(Keys.D8, p.msst, out p.msst, p, 13);
                if (psp > p.skillPts)
                    p.OnUpgrade();
            }
            if (p.level >= (p.tier + 1) * 15) { // vbnfgj
                if (p.type == "tank") {
                    UpgradePlayer("twin", p, Keys.V, 14);
                    UpgradePlayer("sniper", p, Keys.B, 15);
                    UpgradePlayer("machinegun", p, Keys.N, 16);
                    UpgradePlayer("flank guard", p, Keys.F, 17);
                    if (p.level >= 30)
                        UpgradePlayer("smasher", p, Keys.G, 18);
                } else if (p.type == "twin") {
                    UpgradePlayer("triple shoot", p, Keys.V, 14);
                    UpgradePlayer("quad tank", p, Keys.B, 15);
                    UpgradePlayer("twin flank", p, Keys.N, 16);
                } else if (p.type == "triple shoot") {
                    UpgradePlayer("triplet", p, Keys.V, 14);
                    UpgradePlayer("penta shoot", p, Keys.B, 15);
                    UpgradePlayer("spread shoot", p, Keys.N, 16);
                } else if (p.type == "quad tank") {
                    UpgradePlayer("octo tank", p, Keys.V, 14);
                    UpgradePlayer("auto 5", p, Keys.B, 15);
                } else if (p.type == "twin flank") {
                    UpgradePlayer("tripple twin", p, Keys.V, 14);
                    UpgradePlayer("battleship", p, Keys.B, 15);
                } else if (p.type == "sniper") {
                    UpgradePlayer("assasin", p, Keys.V, 14);
                    UpgradePlayer("overseer", p, Keys.B, 15);
                    UpgradePlayer("hunter", p, Keys.N, 16);
                    UpgradePlayer("trapper", p, Keys.F, 17);
                } else if (p.type == "assasin") {
                    UpgradePlayer("ranger", p, Keys.V, 14);
                    UpgradePlayer("stalker", p, Keys.B, 15);
                } else if (p.type == "overseer") {
                    UpgradePlayer("overlord", p, Keys.V, 14);
                    UpgradePlayer("necromancer", p, Keys.B, 15);
                    UpgradePlayer("manager", p, Keys.N, 16);
                    UpgradePlayer("overtrapper", p, Keys.F, 17);
                    UpgradePlayer("battleship", p, Keys.G, 18);
                    UpgradePlayer("factory", p, Keys.J, 19); //
                } else if (p.type == "hunter") {
                    UpgradePlayer("predator", p, Keys.V, 14); // napul
                    UpgradePlayer("streamliner", p, Keys.B, 15);
                    UpgradePlayer("xhunter", p, Keys.N, 16);
                } else if (p.type == "trapper") {
                    UpgradePlayer("tri-trapper", p, Keys.V, 14);
                    UpgradePlayer("gunner trapper", p, Keys.B, 15);
                    UpgradePlayer("overtrapper", p, Keys.N, 16);
                    UpgradePlayer("mega trapper", p, Keys.F, 17);
                    UpgradePlayer("auto trapper", p, Keys.G, 18);
                } else if (p.type == "machinegun") {
                    UpgradePlayer("destroyer", p, Keys.V, 14);
                    UpgradePlayer("gunner", p, Keys.B, 15);
                    if (p.level >= 30)
                        UpgradePlayer("sprayer", p, Keys.N, 16);
                } else if (p.type == "destroyer") {
                    UpgradePlayer("hybrid", p, Keys.V, 14);
                    UpgradePlayer("anihiliator", p, Keys.B, 15);
                    UpgradePlayer("skimmer", p, Keys.N, 16); //  !!!!!!!!!!!!!!!
                    UpgradePlayer("rocketer", p, Keys.F, 17); // !!!!!!!!!!!!!!!
                } else if (p.type == "gunner") {
                    UpgradePlayer("auto gunner", p, Keys.V, 14);
                    UpgradePlayer("gunner trapper", p, Keys.B, 15);
                    UpgradePlayer("streamliner", p, Keys.N, 16);
                } else if (p.type == "flank guard") {
                    UpgradePlayer("tri-angle", p, Keys.V, 14);
                    UpgradePlayer("quad tank", p, Keys.B, 15);
                    UpgradePlayer("twin flank", p, Keys.N, 16);
                    UpgradePlayer("auto 3", p, Keys.F, 17); // napul
                } else if (p.type == "tri-angle") {
                    UpgradePlayer("booster", p, Keys.V, 14);
                    UpgradePlayer("fighter", p, Keys.B, 15);
                } else if (p.type == "auto 3") {
                    UpgradePlayer("auto 5", p, Keys.V, 14); // napul
                    UpgradePlayer("auto gunner", p, Keys.B, 15);
                } else if (p.type == "smasher") {
                    UpgradePlayer("landmine", p, Keys.V, 14);
                    UpgradePlayer("spike", p, Keys.B, 15);
                }
            }
        }
        
        public void UpgradePlayer(string upto, Player p, Keys k, int index) {
            bool press = Form1.press.Contains(k);
            if (p is ClientPl cl) {
                press = cl.cdat.keys[index];
            }
            if (p.upblock2 != k && press) {
                p.type = upto;
                p.Reset();
                switch (p.type) {
                    case "twin":
                        p.cannons.Add(new Cannon(0, new Vec(0.55f, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(-0.55f, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0.5f));
                        p.cannons.Last().Init(this, p);
                        p.tier = 1;
                        p.bdb -= 2;
                        p.bpb--;
                        break;
                    case "triple shoot":
                        p.cannons.Add(new Cannon(1.04719755f,
                            new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(-1.04719755f,
                            new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 2;
                        p.Recoil = 0.3f;
                        p.bdb++;
                        p.bpb++;
                        break;
                    case "triplet":
                        p.cannons.Add(new Cannon(0, new Vec(-0.55f, 0), 0.04f,
                            0.45f, 0.45f, 1.2f, 1, 1, 1, 1, 1, 0.5f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0.55f, 0), 0.04f,
                            0.45f, 0.45f, 1.2f, 1, 1, 1, 1, 1, 0.5f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 3;
                        p.bdb--;
                        p.bpb--;
                        break;
                    case "penta shoot":
                        p.cannons.Add(new Cannon(1.04719755f,
                            new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(-1.04719755f,
                            new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0.523598f,
                            new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 2f, 1, 1, 1, 1, 1, 0.5f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(-0.523598f,
                            new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 2f, 1, 1, 1, 1, 1, 0.5f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 2.3f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.Recoil = 0.6f;
                        p.tier = 3;
                        p.bdb--;
                        break;
                    case "spread shoot":
                        for (int i = 5; i > 0; i--) {
                            p.cannons.Add(new Cannon(i * hc / 10f,
                                new Vec(0, 0), 0.04f,
                                0.25f, 0.25f, 1.4f - 0.05f * i,
                                1, 1, 0.6f, 1, 1, 0.2f * i));
                            p.cannons.Last().Init(this, p);
                            p.cannons.Add(new Cannon(-i * hc / 10f,
                                new Vec(0, 0), 0.04f,
                                0.25f, 0.25f, 1.4f - 0.05f * i,
                                1, 1, 0.6f, 1, 1, 0.2f * i));
                            p.cannons.Last().Init(this, p);
                        }
                        p.cannons.Add(new Cannon(0, new Vec(0, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 3;
                        p.Recoil = 0.2f;
                        p.brb -= 3;
                        break;
                    case "quad tank":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1.2f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(hc, new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1.2f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(-hc / 2, new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1.2f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(hc / 2, new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1.2f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 2;
                        p.Recoil = 0.02f;
                        p.bpb = p.bdb = 0;
                        break;
                    case "octo tank":
                        for (int i = 0; i < 8; i++) {
                            p.cannons.Add(new Cannon(i * hc / 4, new Vec(0, 0.5f),
                                0.05f, 0.4f, 0.4f, 1.4f, 1, 1, 1, 1, 1,
                                (i % 2) * 0.5f));
                            p.cannons.Last().Init(this, p);
                        }
                        p.tier = 3;
                        p.bdb--;
                        break;
                    case "twin flank":
                        p.cannons.Add(new Cannon(0, new Vec(0.55f, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(-0.55f, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0.5f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(hc, new Vec(0.55f, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(hc, new Vec(-0.55f, 0), 0.04f,
                            0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0.5f));
                        p.cannons.Last().Init(this, p);
                        p.tier = 2;
                        p.bdb = -2;
                        p.bpb = 0;
                        break;
                    case "tripple twin":
                        for (int i = 0; i < 3; i++){
                            p.cannons.Add(new Cannon(i * hc / 3f * 2f, 
                                new Vec(0.55f, 0), 0.04f,
                                0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0));
                            p.cannons.Last().Init(this, p);
                            p.cannons.Add(new Cannon(i * hc / 3f * 2f,
                                new Vec(-0.55f, 0), 0.04f,
                                0.45f, 0.45f, 1.5f, 1, 1, 1, 1, 1, 0.5f));
                            p.cannons.Last().Init(this, p);
                        }
                        break;
                    case "battleship":
                        for (int i = 0; i < 2; i++) {
                            p.cannons.Add(new PartisanSpawner(i * hc - hc / 2,
                                new Vec(0.55f, 0), 0.04f,
                                0.45f, 0.3f, 1.5f, 1, 1, 1, 1, 1, 0, i % 2 == 0));
                            p.cannons.Last().Init(this, p);
                            p.cannons.Add(new PartisanSpawner(i * hc - hc / 2,
                                new Vec(-0.55f, 0), 0.04f,
                                0.45f, 0.3f, 1.5f, 1, 1, 1, 1, 1, 0, i % 2 == 1));
                            p.cannons.Last().Init(this, p);
                        }
                        p.bsb = 1;
                        p.SizeBonus = 1.111f;
                        p.bdb = -2;
                        p.bpb = 0;
                        p.tier = 3;
                        break;
                    case "sniper":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.4f, 2f, 1.8f, 1, 1, 2f, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.111f;
                        p.tier = 1;
                        break;
                    case "assasin":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.01f,
                            0.4f, 0.4f, 2.3f, 2f, 1, 1, 3f, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.25f;
                        p.tier = 2;
                        break;
                    case "ranger":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.01f,
                            0.7f, 0.4f, 2.3f, 2.5f, 1, 1, 3f, 1, 0, 0.4f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.428f;
                        p.tier = 3;
                        break;
                    case "stalker":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0f), 0.01f,
                            1f, 0.4f, 2.8f, 2f, 1, 1, 3f, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.25f;
                        p.tier = 3;
                        p.Invis = 2.2f;
                        break;
                    case "overseer":
                        p.cannons.Add(new Spawner(hc / 2, new Vec(0, 0.7f), 0.01f,
                            0.4f, 0.7f, 0.5f, 0.01f, 8, 1, 5f, 0, 0, 8));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Spawner(-hc / 2, new Vec(0, 0.7f), 0.01f,
                            0.4f, 0.7f, 0.5f, 0.01f, 8, 1, 5f, 0, 0, 8));
                        p.cannons.Last().Init(this, p);
                        p.tier = 2;
                        p.bdb--;
                        p.bpb += 5;
                        p.bsb -= 5;
                        break;
                    case "overlord":
                        p.cannons.Add(new Spawner(hc / 2, new Vec(0, 0.7f), 0.01f,
                            0.4f, 0.7f, 0.5f, 0.01f, 8, 1, 5f, 0, 0, 8));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Spawner(-hc / 2, new Vec(0, 0.7f), 0.01f,
                            0.4f, 0.7f, 0.5f, 0.01f, 8, 1, 5f, 0, 0, 8));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Spawner(0, new Vec(0, 0.7f), 0.01f,
                            0.4f, 0.7f, 0.5f, 0.01f, 8, 1, 5f, 0, 0, 8));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Spawner(hc, new Vec(0, 0.7f), 0.01f,
                            0.4f, 0.7f, 0.5f, 0.01f, 8, 1, 5f, 0, 0, 8));
                        p.cannons.Last().Init(this, p);
                        p.tier = 3;
                        break;
                    case "necromancer":
                        p.rdb = 0;
                        p.cannons.Add(new Spawner(hc / 2, new Vec(0, 0.7f), 0.01f,
                            0.4f, 0.7f, 0.5f, 0.01f, 8, 1, 5f, 0, 0, 
                            22 + 2 * p.rdst));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Spawner(-hc / 2, new Vec(0, 0.7f), 0.01f,
                            0.4f, 0.7f, 0.5f, 0.01f, 8, 1, 5f, 0, 0,
                            22 + 2 * p.rdst));
                        p.cannons.Last().Init(this, p);
                        p.tier = 3;
                        p.bdb--;
                        p.bsb--;
                        p.bpb--;
                        break;
                    case "manager":
                        p.cannons.Add(new Spawner(0, new Vec(0, 0.7f), 0.01f,
                            0.4f, 0.7f, 0.5f, 0.01f, 8, 1, 5f, 0, 0, 8));
                        p.cannons.Last().Init(this, p);
                        p.tier = 3;
                        p.Invis = 2.35f;
                        break;
                    case "hunter":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.4f, 2f, 1.8f, 1, 1.3f, 2f, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.02f,
                            0.6f, 0.6f, 1.6f, 1.8f, 1, 1.3f, 2f, 1, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.176f;
                        p.brb--;
                        p.bsb--;
                        p.tier = 2;
                        break;
                    case "predator":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.4f, 2f, 1.8f, 1, 1, 2f, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.02f,
                            0.8f, 0.6f, 1.6f, 1.8f, 1, 1.3f, 2f, 1, 0.3f, 
                            0.6f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.hulls.Add(new SmasherHull(1.3f, 1));
                        p.hulls.Last().Init(this, p);
                        p.SizeBonus = 1.176f;
                        p.bdb += 2;
                        p.mhpb++;
                        p.brb--;
                        p.tier = 3;
                        break;
                    case "streamliner":
                        for (int i = 0; i < 5; i++) {
                            p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0f,
                                0.4f, 0.4f, 2f - 0.2f * i, 1.8f, 1, 1, 
                                1f, 1, 0.2f * i));
                            p.cannons.Last().Init(this, p);
                        }
                        p.SizeBonus = 1.176f;
                        p.tier = 3;
                        p.bsb = 0;
                        p.bdb = -1;
                        p.brb = 0;
                        break;
                    case "xhunter":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.4f, 2f, 1.8f, 1, 1, 2f, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.02f,
                            0.6f, 0.6f, 1.6f, 1.8f, 1, 1.3f, 2f, 1, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.02f,
                            0.8f, 0.8f, 1.2f, 1.8f, 1, 1.4f, 2f, 1, 0.6f));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.125f;
                        p.tier = 3;
                        p.brb++;
                        p.bdb += 2;
                        break;
                    case "trapper":
                        p.cannons.Add(new Launcher(0, new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.7f, 1.2f, 2.3f, 12, 1, 2f, 1, 0, 0.4f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.111f;
                        p.tier = 2;
                        break;
                    case "tri-trapper":
                        p.cannons.Add(new Launcher(0, new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.7f, 1.2f, 2.3f, 12, 1, 2f, 1, 0, 0.4f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Launcher(-hc / 3 * 2, 
                            new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.7f, 1.2f, 2.3f, 12, 1, 2f, 1, 0, 0.4f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Launcher(hc / 3 * 2, 
                            new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.7f, 1.2f, 2.3f, 12, 1, 2f, 1, 0, 0.4f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.111f;
                        p.tier = 2;
                        break;
                    case "gunner trapper":
                        p.cannons.Add(new Launcher(hc, new Vec(0, 0.5f), 0.02f,
                            0.5f, 0.8f, 1.2f, 2.3f, 12, 1, 2f, 1, 0, 0.5f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0.45f, 0), 0.03f,
                            0.2f, 0.2f, 1.8f, 1.5f, .6f, .7f, .6f, 0.1f, 0.5f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(-0.45f, 0), 0.03f,
                            0.2f, 0.2f, 1.8f, 1.5f, .6f, .7f, .6f, 0.1f, 0));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.111f;
                        p.tier = 2;
                        break;
                    case "overtrapper":
                        p.cannons.Add(new Launcher(0, new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.7f, 1.2f, 2.3f, 12, 1, 2f, 1, 0, 0.4f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Spawner(hc / 3 * 2, 
                            new Vec(0, 0), 0.06f,
                            0.4f, 0.7f, 1.2f, 0.03f, 8, 9, 3, 0, 0, 2));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Spawner(hc / 3 * -2, 
                            new Vec(0, 0), 0.06f,
                            0.4f, 0.7f, 1.2f, 0.03f, 8, 9, 3, 0, 0, 2));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.111f;
                        p.tier = 2;
                        break;
                    case "mega trapper":
                        p.cannons.Add(new Launcher(0, new Vec(0, 0.5f), 0.02f,
                            0.8f, 1.2f, 1.2f, 2.3f, 16, 2, 2f, 1, 0, 0.8f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.111f;
                        p.tier = 2;
                        break;
                    case "auto trapper":
                        p.cannons.Add(new Launcher(0, new Vec(0, 0.5f), 0.02f,
                            0.4f, 0.7f, 1.2f, 2.3f, 12, 1, 2f, 1, 0, 0.4f, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new FullAutoCannon(0.03f,
                            0.25f, 0.25f, 0.8f, 1f, 1f, 1f, 1, 0f));
                        p.cannons.Last().Init(this, p);
                        p.SizeBonus = 1.111f;
                        p.tier = 2;
                        break;
                    case "machinegun":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.3f,
                            0.4f, 0.7f, 1.2f, 1, 1, 0.5f, 0.5f, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 1;
                        break;
                    case "destroyer":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0), 0.06f,
                            0.7f, 0.7f, 1.5f, 0.3f, 16, 8, 3, 50, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 2;
                        break;
                    case "hybrid":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0), 0.06f,
                            0.7f, 0.7f, 1.5f, 0.3f, 16, 8, 3, 50, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Spawner(hc, new Vec(0, 0), 0.06f,
                            0.4f, 0.7f, 1.2f, 0.03f, 8, 9, 3, 0, 0, 2));
                        p.cannons.Last().Init(this, p);
                        p.tier = 3;
                        break;
                    case "anihiliator":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0), 0.06f,
                            1f, 1f, 1.5f, 0.3f, 20, 11, 3, 70, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 3;
                        break;
                    case "gunner":
                        p.cannons.Add(new Cannon(0, new Vec(0.8f, 0), 0.03f,
                            0.2f, 0.2f, 1.4f, 1f, .6f, .7f, 1, 0.1f, 0.6f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(-0.8f, 0), 0.03f,
                            0.2f, 0.2f, 1.4f, 1f, .6f, .7f, 1, 0.1f, 0.4f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0.5f, 0), 0.03f,
                            0.2f, 0.2f, 1.8f, 1f, .6f, .7f, 1, 0.1f, 0.2f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(-0.5f, 0), 0.03f,
                            0.2f, 0.2f, 1.8f, 1f, .6f, .7f, 1, 0.1f, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 2;
                        break;
                    case "auto gunner":
                        p.cannons.Add(new Cannon(0, new Vec(0.8f, 0), 0.03f,
                            0.2f, 0.2f, 1.4f, 1f, .6f, .7f, 1, 0.1f, 0.6f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(-0.8f, 0), 0.03f,
                            0.2f, 0.2f, 1.4f, 1f, .6f, .7f, 1, 0.1f, 0.4f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0.5f, 0), 0.03f,
                            0.2f, 0.2f, 1.8f, 1f, .6f, .7f, 1, 0.1f, 0.2f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(-0.5f, 0), 0.03f,
                            0.2f, 0.2f, 1.8f, 1f, .6f, .7f, 1, 0.1f, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new FullAutoCannon(0.03f,
                            0.25f, 0.25f, 0.8f, 1f, 1f, 1f, 1, 0f));
                        p.cannons.Last().Init(this, p);
                        p.tier = 2;
                        break;
                    case "sprayer":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.3f,
                            0.25f, 0.6f, 1.8f, 1, 1, 1, 0.3f, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.3f,
                            0.45f, 0.8f, 1.5f, 1, 1, 1, 0.3f, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 3;
                        break;
                    case "flank guard":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.05f, 
                            0.4f, 0.4f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(hc, new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.tier = 1;
                        break;
                    case "tri-angle":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(2.61799f,
                            new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1f, 1, 1, 1, 1, 5, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(-2.61799f,
                             new Vec(0, 0.5f), 0.05f,
                             0.4f, 0.4f, 1f, 1, 1, 1, 1, 5, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.tier = 1;
                        break;
                    case "booster":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(2.3561944901922747f,
                            new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1f, 1, 0.25f, 0.5f, 1, 5, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(-2.3561944901922747f,
                             new Vec(0, 0.5f), 0.05f,
                             0.4f, 0.4f, 1f, 1, 0.25f, 0.5f, 1, 5, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(2.6179938779914163f,
                            new Vec(-0.1f, 0.3f), 0.05f,
                            0.4f, 0.4f, 1.4f, 1, 0.25f, 0.5f, 1, 5, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(-2.6179938779914163f,
                             new Vec(0.1f, 0.3f), 0.05f,
                             0.4f, 0.4f, 1.4f, 1, 0.25f, 0.5f, 1, 5, 0.8f));
                        p.cannons.Last().Init(this, p);
                        p.tier = 1;
                        break;
                    case "fighter":
                        p.cannons.Add(new Cannon(0, new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1.5f, 1, 1, 1, 1, 1, 0));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(2.61799f,
                            new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1f, 1, 1, 1, 1, 5, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(-2.61799f,
                             new Vec(0, 0.5f), 0.05f,
                             0.4f, 0.4f, 1f, 1, 1, 1, 1, 5, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(hc / 2,
                            new Vec(0, 0.5f), 0.05f,
                            0.4f, 0.4f, 1f, 1, 1, 1, 1, 5, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.cannons.Add(new Cannon(-hc / 2,
                             new Vec(0, 0.5f), 0.05f,
                             0.4f, 0.4f, 1f, 1, 1, 1, 1, 5, 0.3f));
                        p.cannons.Last().Init(this, p);
                        p.tier = 1;
                        break;
                    case "auto 3":
                        for (int i = 0; i < 3; i++) {
                            p.cannons.Add(new OffsetAutoCannon(hc / 3 * 2 * i, 
                                new Vec(0, 1),
                                0.03f, 0.25f, 0.25f, 0.8f, 1f, 1f, 1f, 1, 0f));
                            p.cannons.Last().Init(this, p);
                        }
                        p.tier = 2;
                        break;
                    case "auto 5":
                        for (int i = 0; i < 5; i++) {
                            p.cannons.Add(new OffsetAutoCannon(hc * 0.4f * i,
                                new Vec(0, 1),
                                0.03f, 0.25f, 0.25f, 0.8f, 1f, 1f, 1f, 1, 0f));
                            p.cannons.Last().Init(this, p);
                        }
                        p.tier = 2;
                        break;
                    case "smasher":
                        p.tier = 2;
                        p.SizeBonus = 1.111f;
                        p.rdb++;
                        p.KnockbackResistance = 0.1f;
                        p.skillPts += p.bsst + p.bpst + p.bdst + p.brst;
                        p.bsst = p.bpst = p.bdst = p.brst = 0;
                        p.hulls.Add(new SmasherHull(1.3f, 1));
                        p.hulls.Last().Init(this, p);
                        break;
                    case "landmine":
                        p.tier = 3;
                        p.KnockbackResistance = 0.15f;
                        p.hulls.Add(new SmasherHull(1.3f, 1));
                        p.hulls.Last().Init(this, p);
                        p.hulls.Add(new SmasherHull(1.3f, -1));
                        p.hulls.Last().Init(this, p);
                        p.msb++;
                        p.Invis = 13.05f;
                        break;
                    case "spike":
                        p.tier = 3;
                        p.hulls.Add(new SpikeHull(1.3f, 3));
                        p.hulls.Last().Init(this, p);
                        p.rdb += 2;
                        break;
                    default:
                        break;
                }
                p.OnUpgrade();
                p.upblock2 = k;
            }
            if (p.upblock2 == k && !Form1.press.Contains(k)) {
                p.upblock2 = Keys.D0;
            }
        }

        public void TryUp(Keys k, int statval, out int stat, Player p, int index) {
            stat = statval;
            bool press = Form1.press.Contains(k);
            if (p is ClientPl cl) {
                press = cl.cdat.keys[index];
            }
            int statcap = p.type == "smasher" ||
                p.type == "spike" ||
                p.type == "landmine" ? 10 : 7;
            if (p.skillPts > 0 && statval < statcap && p.upblock != k && press) {
                stat = statval + 1;
                p.skillPts--;
                p.upblock = k;
            }
            if (p.upblock == k && !Form1.press.Contains(k)) {
                p.upblock = Keys.D0;
            }
        }

        public Bitmap Render(string scene) {
            Bitmap b = new Bitmap(600, 600);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.FromArgb(0xee, 0xee, 0xee));
            Player pt = players[plTrackIndex];
            float zoom = pt.drawZoom;
            foreach (Bullet t in bullets) {
                g.DrawBullet(t, scroll, zoom);
            }
            foreach (Shape s in shapes)
                g.DrawShape(s, (s.pos - scroll) * zoom, zoom);
            foreach (Player pl in players)
                g.DrawPlayer(pl.col, pl, scroll, zoom);
            #region Upgrady
            g.ContrastText(new Vec(270, 560), "score: " + pt.score, Color.White);
            g.ContrastText(new Vec(270, 580), "level " + pt.level + " " + pt.type,
                Color.White);
            g.ContrastText(new Vec(10, 420), pt.skillPts + "x", Color.White);
            g.ContrastText(new Vec(10, 440), pt.regst + "x regeneration",
                Color.SandyBrown);
            g.ContrastText(new Vec(10, 460), pt.mhpst + "x max HP",
                Color.Magenta);
            g.ContrastText(new Vec(10, 480), pt.rdst + "x body dmg",
                Color.Purple);
            if (pt.type == "smasher" || pt.type == "landmine" ||
                pt.type == "spike") {
                g.ContrastText(new Vec(10, 500), pt.bsst + "x",
                    Color.DodgerBlue);
                g.ContrastText(new Vec(10, 520), pt.bpst + "x Update to",
                    Color.LemonChiffon);
                g.ContrastText(new Vec(10, 540), pt.bdst + "x Windows 10",
                    Color.LightCoral);
                g.ContrastText(new Vec(10, 560), pt.brst + "x",
                    Color.Lime);
            } else if (pt.type == "overseer" || pt.type == "overlord" ||
                pt.type == "manager" || pt.type == "factory") {
                g.ContrastText(new Vec(10, 500), pt.bsst + "x drone speed",
                    Color.DodgerBlue);
                g.ContrastText(new Vec(10, 520), pt.bpst + "x drone HP",
                    Color.LemonChiffon);
                g.ContrastText(new Vec(10, 540), pt.bdst + "x drone dmg",
                    Color.LightCoral);
                g.ContrastText(new Vec(10, 560), pt.brst + "x reload",
                    Color.Lime);
            } else if (pt.type == "necromancer") {
                g.ContrastText(new Vec(10, 500), pt.bsst + "x drone speed",
                    Color.DodgerBlue);
                g.ContrastText(new Vec(10, 520), pt.bpst + "x drone HP",
                    Color.LemonChiffon);
                g.ContrastText(new Vec(10, 540), pt.bdst + "x drone dmg",
                    Color.LightCoral);
                g.ContrastText(new Vec(10, 560), pt.brst + "x max drone count",
                    Color.Lime);
            } else {
                g.ContrastText(new Vec(10, 500), pt.bsst + "x bullet speed",
                    Color.DodgerBlue);
                g.ContrastText(new Vec(10, 520), pt.bpst + "x bullet penetration",
                    Color.LemonChiffon);
                g.ContrastText(new Vec(10, 540), pt.bdst + "x bullet dmg",
                    Color.LightCoral);
                g.ContrastText(new Vec(10, 560), pt.brst + "x reload",
                    Color.Lime);
            }
            g.ContrastText(new Vec(10, 580), pt.msst + "x movement speed",
                    Color.Cyan);
            #endregion
            if (pt.level >= (pt.tier + 1) * 15) {
                if (pt.type == "tank") {
                    g.ContrastText(new Vec(0, 0), "v - twin", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - sniper", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - machinegun", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - flank guard", Color.LemonChiffon);
                    if (pt.level >= 30)
                        g.ContrastText(new Vec(0, 80), "g - smasher", Color.DodgerBlue);
                } else if (pt.type == "twin") {
                    g.ContrastText(new Vec(0, 0), "v - triple shoot", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - quad tank", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - twin flank", Color.LightCoral);
                } else if (pt.type == "triple shoot") {
                    g.ContrastText(new Vec(0, 0), "v - triplet", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - penta shoot", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - spread shoot", Color.LightCoral);
                } else if (pt.type == "quad tank") {
                    g.ContrastText(new Vec(0, 0), "v - octo tank", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - auto 5", Color.Lime);
                } else if (pt.type == "twin flank") {
                    g.ContrastText(new Vec(0, 0), "v - triple twin", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - battleship", Color.Lime);
                } else if (pt.type == "sniper") {
                    g.ContrastText(new Vec(0, 0), "v - assasin", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - overseer", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - hunter", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - trapper", Color.LemonChiffon);
                } else if (pt.type == "assasin") {
                    g.ContrastText(new Vec(0, 0), "v - ranger", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - stalker", Color.Lime);
                } else if (pt.type == "overseer") {
                    g.ContrastText(new Vec(0, 0), "v - overlord", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - necromancer", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - manager", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - overtrapper", Color.LemonChiffon);
                    g.ContrastText(new Vec(0, 80), "g - battleship", Color.DodgerBlue);
                    g.ContrastText(new Vec(0, 100), "j - factory", Color.Purple);
                } else if (pt.type == "hunter") {
                    g.ContrastText(new Vec(0, 0), "v - predator", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - streamliner", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - xhunter", Color.LightCoral);
                } else if (pt.type == "trapper") {
                    g.ContrastText(new Vec(0, 0), "v - tri trapper", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - gunner trapper", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - overtrapper", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - mega trapper", Color.LemonChiffon);
                    g.ContrastText(new Vec(0, 80), "g - auto trapper", Color.DodgerBlue);
                } else if (pt.type == "machinegun") {
                    g.ContrastText(new Vec(0, 0), "v - destroyer", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - gunner", Color.Lime);
                    if (pt.level >= 45)
                        g.ContrastText(new Vec(0, 40), "n - sprayer", Color.LightCoral);
                } else if (pt.type == "destroyer") {
                    g.ContrastText(new Vec(0, 0), "v - hybrid", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - anihiliator", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - skimmer", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - rocketer", Color.LemonChiffon);
                } else if (pt.type == "gunner") {
                    g.ContrastText(new Vec(0, 0), "v - auto gunner", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - gunner trapper", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - streamliner", Color.LightCoral);
                } else if (pt.type == "flank guard") {
                    g.ContrastText(new Vec(0, 0), "v - tri-angle", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - quad tank", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - twin flank", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - auto 3", Color.LemonChiffon);
                } else if (pt.type == "tri-angle") {
                    g.ContrastText(new Vec(0, 0), "v - booster", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - fighter", Color.Lime);
                } else if (pt.type == "auto 3") {
                    g.ContrastText(new Vec(0, 0), "v - auto 5", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - auto gunner", Color.Lime);
                } else if (pt.type == "smasher") {
                    g.ContrastText(new Vec(0, 0), "v - landmine", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - spike", Color.Lime);
                }
            }
            return b;
        }

        public Bitmap RenderData(GameData dat) {
            Bitmap b = new Bitmap(600, 600);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.FromArgb(0xee, 0xee, 0xee));
            g.ContrastText(new Vec(), dat.msg, Color.White);
            Player pt = dat.track;
            scroll += (pt.pos - new Vec(300, 300) * pt.zoom - scroll) * 0.5f;
            float zoom = pt.drawZoom;
            foreach (Bullet t in dat.bs) {
                g.DrawBullet(t, scroll, zoom);
            }
            foreach (Shape s in dat.ss)
                g.DrawShape(s, (s.pos - scroll) * zoom, zoom);
            foreach (Player pl in dat.pls)
                g.DrawPlayer(pl.col, pl, scroll, zoom);
            #region Upgrady
            g.ContrastText(new Vec(270, 560), "score: " + pt.score, Color.White);
            g.ContrastText(new Vec(270, 580), "level " + pt.level + " " + pt.type,
                Color.White);
            g.ContrastText(new Vec(10, 420), pt.skillPts + "x", Color.White);
            g.ContrastText(new Vec(10, 440), pt.regst + "x regeneration",
                Color.SandyBrown);
            g.ContrastText(new Vec(10, 460), pt.mhpst + "x max HP",
                Color.Magenta);
            g.ContrastText(new Vec(10, 480), pt.rdst + "x body dmg",
                Color.Purple);
            if (pt.type == "smasher" || pt.type == "landmine" ||
                pt.type == "spike") {
                g.ContrastText(new Vec(10, 500), pt.bsst + "x",
                    Color.DodgerBlue);
                g.ContrastText(new Vec(10, 520), pt.bpst + "x Update to",
                    Color.LemonChiffon);
                g.ContrastText(new Vec(10, 540), pt.bdst + "x Windows 10",
                    Color.LightCoral);
                g.ContrastText(new Vec(10, 560), pt.brst + "x",
                    Color.Lime);
            } else if (pt.type == "overseer" || pt.type == "overlord" ||
                pt.type == "manager" || pt.type == "factory") {
                g.ContrastText(new Vec(10, 500), pt.bsst + "x drone speed",
                    Color.DodgerBlue);
                g.ContrastText(new Vec(10, 520), pt.bpst + "x drone HP",
                    Color.LemonChiffon);
                g.ContrastText(new Vec(10, 540), pt.bdst + "x drone dmg",
                    Color.LightCoral);
                g.ContrastText(new Vec(10, 560), pt.brst + "x reload",
                    Color.Lime);
            } else if (pt.type == "necromancer") {
                g.ContrastText(new Vec(10, 500), pt.bsst + "x drone speed",
                    Color.DodgerBlue);
                g.ContrastText(new Vec(10, 520), pt.bpst + "x drone HP",
                    Color.LemonChiffon);
                g.ContrastText(new Vec(10, 540), pt.bdst + "x drone dmg",
                    Color.LightCoral);
                g.ContrastText(new Vec(10, 560), pt.brst + "x max drone count",
                    Color.Lime);
            } else {
                g.ContrastText(new Vec(10, 500), pt.bsst + "x bullet speed",
                    Color.DodgerBlue);
                g.ContrastText(new Vec(10, 520), pt.bpst + "x bullet penetration",
                    Color.LemonChiffon);
                g.ContrastText(new Vec(10, 540), pt.bdst + "x bullet dmg",
                    Color.LightCoral);
                g.ContrastText(new Vec(10, 560), pt.brst + "x reload",
                    Color.Lime);
            }
            g.ContrastText(new Vec(10, 580), pt.msst + "x movement speed",
                    Color.Cyan);
            #endregion
            if (pt.level >= (pt.tier + 1) * 15) {
                if (pt.type == "tank") {
                    g.ContrastText(new Vec(0, 0), "v - twin", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - sniper", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - machinegun", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - flank guard", Color.LemonChiffon);
                    if (pt.level >= 30)
                        g.ContrastText(new Vec(0, 80), "g - smasher", Color.DodgerBlue);
                } else if (pt.type == "twin") {
                    g.ContrastText(new Vec(0, 0), "v - triple shoot", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - quad tank", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - twin flank", Color.LightCoral);
                } else if (pt.type == "triple shoot") {
                    g.ContrastText(new Vec(0, 0), "v - triplet", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - penta shoot", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - spread shoot", Color.LightCoral);
                } else if (pt.type == "quad tank") {
                    g.ContrastText(new Vec(0, 0), "v - octo tank", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - auto 5", Color.Lime);
                } else if (pt.type == "twin flank") {
                    g.ContrastText(new Vec(0, 0), "v - triple twin", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - battleship", Color.Lime);
                } else if (pt.type == "sniper") {
                    g.ContrastText(new Vec(0, 0), "v - assasin", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - overseer", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - hunter", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - trapper", Color.LemonChiffon);
                } else if (pt.type == "assasin") {
                    g.ContrastText(new Vec(0, 0), "v - ranger", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - stalker", Color.Lime);
                } else if (pt.type == "overseer") {
                    g.ContrastText(new Vec(0, 0), "v - overlord", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - necromancer", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - manager", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - overtrapper", Color.LemonChiffon);
                    g.ContrastText(new Vec(0, 80), "g - battleship", Color.DodgerBlue);
                    g.ContrastText(new Vec(0, 100), "j - factory", Color.Purple);
                } else if (pt.type == "hunter") {
                    g.ContrastText(new Vec(0, 0), "v - predator", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - streamliner", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - xhunter", Color.LightCoral);
                } else if (pt.type == "trapper") {
                    g.ContrastText(new Vec(0, 0), "v - tri trapper", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - gunner trapper", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - overtrapper", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - mega trapper", Color.LemonChiffon);
                    g.ContrastText(new Vec(0, 80), "g - auto trapper", Color.DodgerBlue);
                } else if (pt.type == "machinegun") {
                    g.ContrastText(new Vec(0, 0), "v - destroyer", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - gunner", Color.Lime);
                    if (pt.level >= 45)
                        g.ContrastText(new Vec(0, 40), "n - sprayer", Color.LightCoral);
                } else if (pt.type == "destroyer") {
                    g.ContrastText(new Vec(0, 0), "v - hybrid", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - anihiliator", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - skimmer", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - rocketer", Color.LemonChiffon);
                } else if (pt.type == "gunner") {
                    g.ContrastText(new Vec(0, 0), "v - auto gunner", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - gunner trapper", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - streamliner", Color.LightCoral);
                } else if (pt.type == "flank guard") {
                    g.ContrastText(new Vec(0, 0), "v - tri-angle", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - quad tank", Color.Lime);
                    g.ContrastText(new Vec(0, 40), "n - twin flank", Color.LightCoral);
                    g.ContrastText(new Vec(0, 60), "f - auto 3", Color.LemonChiffon);
                } else if (pt.type == "tri-angle") {
                    g.ContrastText(new Vec(0, 0), "v - booster", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - fighter", Color.Lime);
                } else if (pt.type == "auto 3") {
                    g.ContrastText(new Vec(0, 0), "v - auto 5", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - auto gunner", Color.Lime);
                } else if (pt.type == "smasher") {
                    g.ContrastText(new Vec(0, 0), "v - landmine", Color.Cyan);
                    g.ContrastText(new Vec(0, 20), "b - spike", Color.Lime);
                }
            }
            return b;
        }

        public static PointF[] Polygon(int angles, float rotation, Vec pos, 
            float zoom, Vec scroll, float size) {
            PointF[] pts = new PointF[angles];
            float rads = (float)Math.PI * 2 / angles;
            Vec up = new Vec(0, 1f);
            up.RotateByZero(rotation);
            for (int i = 0; i < angles; i++) {
                up.RotateByZero(rads);
                pts[i] = up * size * zoom + (pos - scroll) * zoom;
            }
            return pts;
        }

        public static PointF[] Star(int angles, float rotation, Vec pos, float zoom,
            Vec scroll, float size, float insize) {
            PointF[] pts = new PointF[angles * 2];
            float rads = (float)Math.PI / angles;
            Vec up = new Vec(0, 1f);
            up.RotateByZero(rotation);
            for (int i = 0; i < angles * 2; i++) {
                up.RotateByZero(rads);
                pts[i] = up * (i % 2 == 0 ? size : insize) * zoom + 
                    (pos - scroll) * zoom;
            }
            return pts;
        }
    }

    static class GraphicsExtensor {
        static float fc = (float)(Math.PI * 2);

        public static void DrawCircle(this Graphics g, Color color, 
            Vec center, float radius, Pen stroke = null) {
            Vec r2 = new Vec(radius, radius);
            RectangleF rect = new RectangleF(center - r2, r2 * 2);
            g.FillEllipse(new SolidBrush(color), rect);
            g.DrawEllipse(stroke ?? Game.stroke, rect);
        }

        private static Pen AStroke(int alpha) {
            if (alpha > 240)
                return Game.stroke;
            Pen strok = Game.stroke;
            strok = new Pen(Color.FromArgb((int)(alpha / 2.7f), strok.Color), 
                strok.Width);
            return strok;
        }

        private static Brush AddAlpha(this SolidBrush b, int alpha) {
            Brush br = new SolidBrush(Color.FromArgb(alpha, b.Color));
            return br;
        }

        public static void DrawBullet(this Graphics g, Bullet t, Vec scroll,
            float zoom) {
            Player pl = t.owner;
            if (t.type == "drone" || t.type == "necrodrone") {
                Bullet d = t;
                float rot = (float)-Math.Atan2(d.vel.X, d.vel.Y);
                PointF[] pts = Game.Polygon(3, rot, d.pos, zoom, scroll, d.size);
                if (t.type == "necrodrone")
                    pts = Game.Polygon(4, rot + fc / 8, d.pos, zoom, scroll, 
                        d.size);
                g.FillPolygon(new SolidBrush(d.col), pts);
                g.DrawPolygon(Game.stroke, pts);
            } else if (t.type == "trap") {
                Bullet tr = t;
                PointF[] pts = Game.Star(3, 0, tr.pos, zoom, scroll, tr.size * 1.7f,
                    tr.size / 1.7f);
                g.FillPolygon(new SolidBrush(tr.col), pts);
                g.DrawPolygon(Game.stroke, pts);
            } else {
                g.DrawCircle(t.col, (t.pos - scroll) * zoom,
                    t.size * zoom);
            }
        }

        public static void DrawPlayer(this Graphics g, Color col, Player pl,
            Vec scroll, float zoom) {
            int invis = 255;
            if (pl.Invis > 0)
                invis = (int)(pl.InvisTime / pl.Invis * 255);
            if (invis < 0)
                invis = 0;
            foreach (SmasherHull h in pl.hulls) {
                int i = invis == 255 ? 255 : (int)(invis / 2.3f);
                g.FillPolygon((Brushes.Black as SolidBrush).AddAlpha(i),
                    h.GetPts(pl.pos - scroll, zoom));
            }
            foreach (Cannon c in pl.cannons) {
                if (!(c is FullAutoCannon))
                    g.DrawCannon(c, scroll, zoom, pl);
            }
            if (pl.Invis > 0) {
                if (pl.type == "necromancer" || pl.type == "factory") {
                    PointF[] pts = Game.Polygon(4, -pl.rotation - fc / 8, 
                        pl.pos, zoom, scroll, pl.size * 1.4f);
                    g.FillPolygon(new SolidBrush(pl.col).AddAlpha(invis), pts);
                    g.DrawPolygon(AStroke(invis), pts);
                } else {
                    g.DrawCircle(Color.FromArgb(invis, col),
                        (pl.pos - scroll) * zoom, pl.size * zoom, AStroke(invis));
                }
            } else {
                if (pl.type == "necromancer" || pl.type == "factory") {
                    PointF[] pts = Game.Polygon(4, -pl.rotation - fc / 8, 
                        pl.pos, zoom, scroll, pl.size * 1.4f);
                    g.FillPolygon(new SolidBrush(pl.col), pts);
                    g.DrawPolygon(Game.stroke, pts);
                } else {
                    g.DrawCircle(col, (pl.pos - scroll) * zoom,
                        pl.size * zoom);
                }
            }
            foreach (Cannon c in pl.cannons) {
                if (c is FullAutoCannon f)
                    g.DrawFACannon(f, zoom, scroll);
            }
            if (pl.Invis == 0 || pl.InvisTime > 0)
                g.DrawHealthBar((pl.pos + new Vec(0, pl.size + 8) - scroll) * zoom,
                    pl.MaxHP, pl.HP);
        }

        public static void DrawFACannon(this Graphics g, FullAutoCannon c, 
            float zoom, Vec scroll) {
            Player pl = c.owner;
            Vec caa = new Vec(-c.insize, 0),
                cba = new Vec(c.insize, 0),
                cca = new Vec(c.outsize, c.lenght),
                cda = new Vec(-c.outsize, c.lenght);
            caa.RotateByZero(c.rotation);
            cba.RotateByZero(c.rotation);
            cca.RotateByZero(c.rotation);
            cda.RotateByZero(c.rotation);
            PointF[] ptsa = {
                    (caa + pl.pos - scroll) * zoom,
                    (cba + pl.pos - scroll) * zoom,
                    (cca + pl.pos - scroll) * zoom,
                    (cda + pl.pos - scroll) * zoom };
            if (pl.Invis > 0) {
                int invis = (int)(pl.InvisTime / pl.Invis * 255);
                if (invis < 0)
                    invis = 0;
                Brush br = (Brushes.Gray as SolidBrush).AddAlpha(invis);
                g.FillPolygon(br, ptsa);
                g.DrawPolygon(AStroke(invis), ptsa);
                g.DrawCircle(Color.FromArgb(invis, Color.Gray),
                    (pl.pos - scroll) * zoom, 10 * zoom, AStroke(invis));
            } else {
                g.FillPolygon(Brushes.Gray, ptsa);
                g.DrawPolygon(Game.stroke, ptsa);
                g.DrawCircle(Color.Gray, (pl.pos - scroll) * zoom, 10 * zoom);
            }
        }

        public static void DrawCannon(this Graphics g, Cannon c, Vec scroll,
            float zoom, Player pl = null) {
            if (pl == null)
                pl = c.owner;
            if (!Form1.isServer)
                c.ownersiz = 1;
            if (c is OffsetAutoCannon oac) {
                Vec aca = new Vec(-c.insize, 0),
                    acb = new Vec(c.insize, 0),
                    acc = new Vec(c.outsize, c.lenght),
                    acd = new Vec(-c.outsize, c.lenght);
                Vec center = c.offset;
                center.RotateByZero(c.rotpos);
                aca.RotateByZero(oac.rotation);
                acb.RotateByZero(oac.rotation);
                acc.RotateByZero(oac.rotation);
                acd.RotateByZero(oac.rotation);
                aca += center;
                acb += center;
                acc += center;
                acd += center;
                PointF[] apts = {
                    (aca + pl.pos - scroll) * zoom,
                    (acb + pl.pos - scroll) * zoom,
                    (acc + pl.pos - scroll) * zoom,
                    (acd + pl.pos - scroll) * zoom };
                if (pl.Invis > 0) {
                    int invis = (int)(pl.InvisTime / pl.Invis * 255);
                    if (invis < 0)
                        invis = 0;
                    Brush br = (Brushes.Gray as SolidBrush).AddAlpha(invis);
                    g.FillPolygon(br, apts);
                    g.DrawPolygon(AStroke(invis), apts);
                    g.DrawCircle(Color.FromArgb(invis, Color.Gray), 
                        (center + pl.pos - scroll) * zoom, 
                        10 * zoom, AStroke(invis));
                } else {
                    g.FillPolygon(Brushes.Gray, apts);
                    g.DrawPolygon(Game.stroke, apts);
                    g.DrawCircle(Color.Gray, (center + pl.pos - scroll) * zoom, 
                        10 * zoom);
                }
                return;
            }
            Vec ca = new Vec(c.offset.X * pl.size - c.insize, c.offset.Y),
                cb = new Vec(c.offset.X * pl.size + c.insize, c.offset.Y),
                cc = new Vec(c.offset.X * pl.size + c.outsize, 
                    c.offset.Y + c.lenght),
                cd = new Vec(c.offset.X * pl.size - c.outsize, 
                    c.offset.Y + c.lenght);
            if (c.censize != 0) {
                cc = new Vec(c.offset.X * pl.size + c.censize,
                    c.offset.Y + c.clenght);
                cd = new Vec(c.offset.X * pl.size - c.censize,
                    c.offset.Y + c.clenght);
            }
            ca.RotateByZero(c.rotpos - pl.rotation);
            cb.RotateByZero(c.rotpos - pl.rotation);
            cc.RotateByZero(c.rotpos - pl.rotation);
            cd.RotateByZero(c.rotpos - pl.rotation);
            PointF[] pts = {
                    (ca + pl.pos - scroll) * zoom,
                    (cb + pl.pos - scroll) * zoom,
                    (cc + pl.pos - scroll) * zoom,
                    (cd + pl.pos - scroll) * zoom };
            if (pl.Invis > 0) {
                int invis = (int)(pl.InvisTime / pl.Invis * 255);
                if (invis < 0)
                    invis = 0;
                Brush br = (Brushes.Gray as SolidBrush).AddAlpha(invis);
                g.FillPolygon(br, pts);
                g.DrawPolygon(AStroke(invis), pts);
            } else {
                g.FillPolygon(Brushes.Gray, pts);
                g.DrawPolygon(Game.stroke, pts);
            }
            if (c.censize != 0) {
                ca = new Vec(c.offset.X * pl.size - c.censize, 
                    c.offset.Y + c.clenght);
                cb = new Vec(c.offset.X * pl.size + c.censize, 
                    c.offset.Y + c.clenght);
                cc = new Vec(c.offset.X * pl.size + c.outsize,
                    c.offset.Y + c.lenght);
                cd = new Vec(c.offset.X * pl.size - c.outsize,
                    c.offset.Y + c.lenght);
                ca.RotateByZero(c.rotpos - pl.rotation);
                cb.RotateByZero(c.rotpos - pl.rotation);
                cc.RotateByZero(c.rotpos - pl.rotation);
                cd.RotateByZero(c.rotpos - pl.rotation);
                pts = new PointF[]{
                    (ca + pl.pos - scroll) * zoom,
                    (cb + pl.pos - scroll) * zoom,
                    (cc + pl.pos - scroll) * zoom,
                    (cd + pl.pos - scroll) * zoom };
                if (pl.Invis > 0) {
                    int invis = (int)(pl.InvisTime / pl.Invis * 255);
                    Brush br = (Brushes.Gray as SolidBrush).AddAlpha(invis);
                    g.FillPolygon(br, pts);
                    g.DrawPolygon(AStroke(invis), pts);
                } else {
                    g.FillPolygon(Brushes.Gray, pts);
                    g.DrawPolygon(Game.stroke, pts);
                }
            }
        }

        public static void DrawShape(this Graphics g, Shape s, Vec pos, float zoom) {
            int angles = 5;
            Brush br = new SolidBrush(Color.DodgerBlue);
            if (s.type == "triangle") {
                br = new SolidBrush(Color.LightCoral);
                angles = 3;
            } else if (s.type == "square") {
                br = new SolidBrush(Color.LemonChiffon);
                angles = 4;
            }
            PointF[] pts = new PointF[angles];
            float rads = (float)Math.PI * 2 / angles;
            Vec up = new Vec(1.3f, 1.3f);
            for (int i = 0; i < angles; i++) {
                up.RotateByZero(rads);
                pts[i] = up * s.size * zoom + pos;
            }
            g.FillPolygon(br, pts);
            g.DrawPolygon(Game.stroke, pts);
            g.DrawHealthBar(pos + new Vec(0, s.size + 8), s.HPMax, s.HP);
        }

        public static void DrawHealthBar(this Graphics g, Vec pos,
            float MHP, float HP) {
            if (MHP <= HP)
                return;
            Rectangle frame = new Rectangle(pos - new Vec(22, 4), 
                new Vec(45, 9));
            Rectangle state = new Rectangle(pos - new Vec(20, 2), 
                new Vec(HP / MHP * 40, 5));
            g.FillRectangle(Brushes.Black, frame);
            g.FillRectangle(Brushes.Lime, state);
        }

        public static void ContrastText(this Graphics g, Vec pos,
            string text, Color col) {
            g.DrawString(text, new Font("Lucida", 16), Brushes.Black, 
                pos + new Vec(1, 1));
            g.DrawString(text, new Font("Lucida", 16), Brushes.Black,
                pos + new Vec(-1, 1));
            g.DrawString(text, new Font("Lucida", 16), Brushes.Black,
                pos + new Vec(-1, -1));
            g.DrawString(text, new Font("Lucida", 16), Brushes.Black,
                pos + new Vec(1, -1));
            g.DrawString(text, new Font("Lucida", 16), new SolidBrush(col), pos);
        }

        public static void RotateByZero(this Vec v, float angle) {
            Vec c = v.Copy();
            v.X = c.X * (float)Math.Cos(angle) - c.Y * (float)Math.Sin(angle);
            v.Y = c.Y * (float)Math.Cos(angle) + c.X * (float)Math.Sin(angle);
        }
    }
}
