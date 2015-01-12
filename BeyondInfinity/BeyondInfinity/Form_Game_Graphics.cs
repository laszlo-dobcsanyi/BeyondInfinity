using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public partial class GameForm : Form
    {
        private static Microsoft.DirectX.Direct3D.Font DrawingFont;

        public Device Device;
        public ReaderWriterLockSlim Device_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public GraphicalProcess GraphicalProcessor;

        public void LoadMap(string Data)
        {
            Map_Locker.EnterReadLock();
            try
            {
                for (int Column = 0; Column < MapTiles_Size.Height; Column++)
                    for (int Row = 0; Row < MapTiles_Size.Width; Row++)
                        //if (MapTiles[Row, Column] != null)
                        MapTiles[Row, Column].Dispose();

                string[] Arguments = Data.Split('\t');

                Game.Area = Arguments[0];
                MapTiles_Size = new System.Drawing.Size(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));
                MapTiles = new Texture[MapTiles_Size.Width, MapTiles_Size.Height];
            }
            finally { Map_Locker.ExitReadLock(); }
        }

        public void LoadRegion(string Data)
        {
            string[] Arguments = Data.Split('\t');

            Map_Locker.EnterReadLock();
            try
            {
                MapTiles[Convert.ToInt32(Arguments[0]), Convert.ToInt32(Arguments[1])] = TextureLoader.FromFile(Device, @"data\maps\" + Game.Area + @"\" + Arguments[2] + ".jpg");
            }
            finally { Map_Locker.ExitReadLock(); }

        }

        public void DrawLoading(Device Device)
        {
            int Loaded = 0;
            Map_Locker.EnterReadLock();
            try
            {
                for (int Column = 0; Column < MapTiles_Size.Height; Column++)
                    for (int Row = 0; Row < MapTiles_Size.Width; Row++)
                        if (MapTiles[Row, Column] != null)
                            if (!MapTiles[Row, Column].Disposed) Loaded++;
            }
            finally { Map_Locker.ExitReadLock(); }

            if (Loaded != MapTiles_Size.Width * MapTiles_Size.Height)
            {
                Text = "Loading: " + Program.Loading + " Loaded = " + Program.Loaded;

                Sprite Sprite = new Sprite(Device);
                Sprite.Begin(SpriteFlags.AlphaBlend);

                RankFont.DrawText(Sprite, "Beyond Infinity", new Rectangle(ClientSize.Width / 2 - 256, 32, 512, 64), DrawTextFormat.Center, Color.Purple);

                DrawingFont.DrawText(Sprite, "Loading " + Loaded + " / " + MapTiles_Size.Width * MapTiles_Size.Height,
                    new Rectangle(ClientSize.Width / 2 - 256, ClientSize.Height - 128, 512, 64), DrawTextFormat.Center, Color.White);

                Sprite.End();
                Sprite.Dispose();
            }
            else
                if (Program.Loading || Program.Loaded != 7)
                {
                    Text = "Loading: " + Program.Loading + " Loaded = " + Program.Loaded;

                    Sprite Sprite = new Sprite(Device);
                    Sprite.Begin(SpriteFlags.AlphaBlend);

                    RankFont.DrawText(Sprite, "Beyond Infinity", new Rectangle(ClientSize.Width / 2 - 256, 32, 512, 64), DrawTextFormat.Center, Color.Purple);

                    DrawingFont.DrawText(Sprite, "Loading..",
                        new Rectangle(ClientSize.Width / 2 - 256, ClientSize.Height - 128, 512, 64), DrawTextFormat.Center, Color.White);

                    Sprite.End();
                    Sprite.Dispose();
                }
                else Phase_Game();

            /*int Loaded = 0;

              Device_Locker.EnterReadLock();
              try
              {
                  Sprite Sprite = new Sprite(Device);
                  Sprite.Begin(SpriteFlags.AlphaBlend);

                  RankFont.DrawText(Sprite, "Beyond Infinity", new Rectangle(ClientSize.Width / 2 - 256, 32, 512, 64), DrawTextFormat.Center, Color.Purple);

                  for (int Row = 0; Row < MapTiles_Size.Height; Row++)
                      for (int Column = 0; Column < MapTiles_Size.Width; Column++)
                          if (MapTiles[Column, Row] != null) Loaded++;

                  DrawingFont.DrawText(Sprite, "Loading " + Game.Area + ":" + Loaded + "/" + MapTiles_Size.Width * MapTiles_Size.Height,
                      new Rectangle(ClientSize.Width / 2 - 256, ClientSize.Height - 128, 512, 64), DrawTextFormat.Center, Color.White);

                  Sprite.End();
                  Sprite.Dispose();
              }
              finally { Device_Locker.ExitReadLock(); }

              if (Program.MapLoading)
                  if ((Loaded != 0) && (Loaded == MapTiles_Size.Width * MapTiles_Size.Height))
                  {
                      Program.MapLoading = false;
                      Phase_Game();
                  }*/
        }

        public void DrawGame(Device Device)
        {
            Device_Locker.EnterReadLock();
            try
            {
                Sprite Sprite = new Sprite(Device);
                Sprite.Begin(SpriteFlags.AlphaBlend);

                DrawMap(Sprite);
                DrawSplashes(Sprite);
                DrawObjects(Sprite);
                DrawCorpses(Sprite);
                DrawMissiles(Sprite);
                DrawCreatures(Sprite);
                DrawPersons(Sprite);
                DrawHeroes(Sprite);

                DrawBattlefield(Sprite);
                DrawChat(Sprite);
                DrawPanels(Sprite);
                DrawTargetFrame(Sprite);
                DrawMarks(Sprite);
                DrawSpells(Sprite);

                DrawCursor(Sprite);

                Sprite.End();
                Sprite.Dispose();
            }
            finally { Device_Locker.ExitReadLock(); }
        }

        #region Game_Engine
        private static Size MapTiles_Size;
        private static Texture[,] MapTiles;
        public static ReaderWriterLockSlim Map_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private void DrawMap(Sprite Sprite)
        {
            Point Location = new Point((int)(Game.Character.Location.X / 512) - 1, (int)(Game.Character.Location.Y / 512) - 1);

            Map_Locker.EnterWriteLock();
            try
            {
                for (int Row = 0; Row < 3; Row++)
                    for (int Column = 0; Column < 3; Column++)
                    {
                        Texture Tile = null;
                        if (0 <= Location.X + Column)
                            if (Location.X + Column < MapTiles_Size.Width)
                                if (0 <= Location.Y + Row)
                                    if (Location.Y + Row < MapTiles_Size.Height)
                                        Tile = MapTiles[Location.X + Column, Location.Y + Row];

                        if (Tile != null)
                            Sprite.Draw2D(Tile, Point.Empty, 0f,
                                new Point((Column - 1) * 512 - (int)(Game.Character.Location.X % 512) + ClientSize.Width / 2 + Offset.X,
                                    (Row - 1) * 512 - (int)(Game.Character.Location.Y % 512) + ClientSize.Height / 2 + Offset.Y), Color.White);
                    }
            }
            finally { Map_Locker.ExitWriteLock(); }
        }

        private void DrawSplashes(Sprite Sprite)
        {
            Game.Splashes_Locker.EnterReadLock();
            try
            {
                Point Location;
                foreach (Splash NextSplash in Game.Splashes)
                {
                    Location = new Point((int)(NextSplash.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                        (int)(NextSplash.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32);

                    Sprite.Draw2D(NextSplash.Icon, Point.Empty, 0f, Location, Color.White);
                    int Length = (int)((NextSplash.FullInterval - NextSplash.Interval) / NextSplash.FullInterval * 64);
                    if (Length != 0)
                        Sprite.Draw2D(NextSplash.Icon, new Rectangle(0, 0, 64, Length), new Rectangle(0, 0, 64, Length), new Point(Location.X + 1, Location.Y), Color.Gray);

                    CooldownFont.DrawText(Sprite, (NextSplash.Rank + 1).ToString(), new Rectangle(Location.X + 1, Location.Y + 8 + 1, 64, 64), DrawTextFormat.Center, Color.Black);
                    CooldownFont.DrawText(Sprite, (NextSplash.Rank + 1).ToString(), new Rectangle(Location.X, Location.Y + 8, 64, 64), DrawTextFormat.Center, Color.White);

                    Sprite.Draw2D(Splashes[3],
                    Rectangle.Empty, new Rectangle(0, 0, NextSplash.Diameter, NextSplash.Diameter),
                    new Point((int)((float)(NextSplash.Location.X - Game.Character.Location.X + ClientSize.Width / 2 + Offset.X) * (256 / (float)NextSplash.Diameter)) - 128,
                        (int)((float)(NextSplash.Location.Y - Game.Character.Location.Y + ClientSize.Height / 2 + Offset.Y) * (256 / (float)NextSplash.Diameter)) - 128), Color.White);
                }
            }
            finally { Game.Splashes_Locker.ExitReadLock(); }
        }

        private Texture Border_Enemy;
        private Texture Border_Friend;
        private Texture Border_Group;
        private Texture Border_Creature;

        private Texture Corpse;
        private Texture Corpse_Looting;

        private Texture Portal;
        private Texture[] Flags = new Texture[4];
        private void DrawObjects(Sprite Sprite)
        {
            if (Splash)
                if ((CursorLocation.X < ClientSize.Width + 2 * Offset.X) && (0 + 2 * Offset.X < CursorLocation.X))
                    if (Casting_Rank != -1)
                    {
                        int Radius = (int)Game.Character.Spells[Casting_Spell].Parameters[0];

                        Sprite.Draw2D(Splashes[Game.Character.Faction],
                        Rectangle.Empty, new Rectangle(0, 0, Radius, Radius),
                        new Point((int)((float)CursorLocation.X * (256 / (float)Radius)) - 128, (int)((float)CursorLocation.Y * (256 / (float)Radius)) - 128), Color.White);
                    }

            if (Game.Area == "Old World")
                foreach (Flag NextFlag in Game.Flags)
                {
                    Sprite.Draw2D(Flags[NextFlag.Owner + 1], Point.Empty, 0f,
                        new Point((int)(NextFlag.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                            (int)(NextFlag.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
                }

            foreach (Portal NextPortal in Game.Portals)
            {
                Sprite.Draw2D(Portal, Point.Empty, 0f, new Point((int)(NextPortal.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                        (int)(NextPortal.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
            }
        }

        private void DrawCorpses(Sprite Sprite)
        {
            Game.Corpses_Locker.EnterReadLock();
            try
            {
                foreach (Corpse NextCorpse in Game.Corpses)
                    if (NextCorpse.Looting)
                        Sprite.Draw2D(Corpse_Looting, Point.Empty, 0f, new Point((int)(NextCorpse.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextCorpse.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
                    else
                        Sprite.Draw2D(Corpse, Point.Empty, 0f, new Point((int)(NextCorpse.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextCorpse.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
            }
            finally { Game.Corpses_Locker.ExitReadLock(); }
        }

        private void DrawMissiles(Sprite Sprite)
        {
            Game.Missiles_Locker.EnterReadLock();
            try
            {
                Point Location;
                foreach (Missile NextMissile in Game.Missiles)
                {
                    Location = new Point((int)(NextMissile.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                            (int)(NextMissile.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32);

                    Sprite.Draw2D(NextMissile.Icon, Point.Empty, 0f, Location, Color.White);
                    CooldownFont.DrawText(Sprite, (NextMissile.Rank + 1).ToString(), new Rectangle(Location.X + 1, Location.Y + 8 + 1, 64, 64), DrawTextFormat.Center, Color.Black);
                    CooldownFont.DrawText(Sprite, (NextMissile.Rank + 1).ToString(), new Rectangle(Location.X, Location.Y + 8, 64, 64), DrawTextFormat.Center, Color.White);
                }
            }
            finally { Game.Missiles_Locker.ExitReadLock(); }
        }

        private void DrawCreatures(Sprite Sprite)
        {
            Game.Creatures_Locker.EnterReadLock();
            try
            {
                foreach (Creature NextCreature in Game.Creatures)
                {
                    int Damage = (int)(((float)(NextCreature.MaxEnergy - NextCreature.Energy) / NextCreature.MaxEnergy) * 64);
                    Sprite.Draw2D(NextCreature.Icon, Point.Empty, 0f,
                        new Point((int)(NextCreature.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                        (int)(NextCreature.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
                    Sprite.Draw2D(NextCreature.Icon, new Rectangle(0, 64 - Damage, 64, Damage), new Rectangle(0, 0, 64, Damage),
                         new Point((int)(NextCreature.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 31,
                        (int)(NextCreature.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32 + 64 - Damage), Color.Red);

                    if (NextCreature.Faction == Game.Character.Faction)
                    {
                        Sprite.Draw2D(Border_Friend, Point.Empty, 0f, new Point((int)(NextCreature.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextCreature.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
                    }
                    else
                        Sprite.Draw2D(Border_Enemy, Point.Empty, 0f, new Point((int)(NextCreature.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextCreature.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);

                    /*DrawingFont.DrawText(Sprite, NextCreature.Name, new Rectangle(new Point((int)(NextCreature.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 64,
                        (int)(NextCreature.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 48), new Size(128, 16)),
                        DrawTextFormat.Center, NextCreature.Faction == Game.Character.Faction ? Color.Green : Color.Red);*/

                    NextCreature.CombatTexts_Locker.EnterReadLock();
                    try
                    {
                        if (NextCreature.CombatTexts.Count < 5)
                        {
                            DrawingFont.DrawText(Sprite, NextCreature.Name, new Rectangle(new Point((int)(NextCreature.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 64 + 1,
                                (int)(NextCreature.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32 - 16 + 1), new Size(128, 16)), DrawTextFormat.Center, Color.Black);
                            DrawingFont.DrawText(Sprite, NextCreature.Name, new Rectangle(new Point((int)(NextCreature.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 64,
                                (int)(NextCreature.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32 - 16), new Size(128, 16)), DrawTextFormat.Center, ColorPalette[NextCreature.ItemLevel]);
                        }

                        int Number = 0;
                        foreach (CombatText NextCombatText in NextCreature.CombatTexts)
                        {
                            DrawingFont.DrawText(Sprite, NextCombatText.Value.ToString(), new Rectangle(new Point((int)(NextCreature.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32 + 1,
                                (int)(NextCreature.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y + 16 + 1 - Number * 12), new Size(64, 16)), DrawTextFormat.Center, Color.Black);
                            DrawingFont.DrawText(Sprite, NextCombatText.Value.ToString(), new Rectangle(new Point((int)(NextCreature.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextCreature.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y + 16 - Number * 12), new Size(64, 16)), DrawTextFormat.Center, NextCombatText.Value < 0 ? Color.Red : Color.Green);
                            Number++;
                        }
                    }
                    finally { NextCreature.CombatTexts_Locker.ExitReadLock(); }
                }
            }
            finally { Game.Creatures_Locker.ExitReadLock(); }
        }

        private void DrawPersons(Sprite Sprite)
        {
            Game.Persons_Locker.EnterReadLock();
            try
            {
                foreach (Person NextPerson in Game.Persons)
                {
                    int Damage = (int)(((float)(NextPerson.MaxEnergy - NextPerson.Energy) / NextPerson.MaxEnergy) * 64);
                    Sprite.Draw2D(NextPerson.Icon, Point.Empty, 0f,
                        new Point((int)(NextPerson.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                        (int)(NextPerson.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
                    Sprite.Draw2D(NextPerson.Icon, new Rectangle(0, 64 - Damage, 64, Damage), new Rectangle(0, 0, 64, Damage),
                         new Point((int)(NextPerson.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 31,
                        (int)(NextPerson.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32 + 64 - Damage), Color.Red);

                    if (NextPerson.Faction == Game.Character.Faction)
                    {
                        Sprite.Draw2D(Border_Friend, Point.Empty, 0f, new Point((int)(NextPerson.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextPerson.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
                    }
                    else
                        Sprite.Draw2D(Border_Enemy, Point.Empty, 0f, new Point((int)(NextPerson.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextPerson.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);

                    /*DrawingFont.DrawText(Sprite, NextPerson.Name, new Rectangle(new Point((int)(NextPerson.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 64,
                        (int)(NextPerson.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 48), new Size(128, 16)),
                        DrawTextFormat.Center, NextPerson.Faction == Game.Character.Faction ? Color.Green : Color.Red);*/

                    NextPerson.CombatTexts_Locker.EnterReadLock();
                    try
                    {
                        if (NextPerson.CombatTexts.Count < 5)
                        {
                            DrawingFont.DrawText(Sprite, NextPerson.Name, new Rectangle(new Point((int)(NextPerson.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 64 + 1,
                                (int)(NextPerson.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32 - 16 + 1), new Size(128, 16)), DrawTextFormat.Center, Color.Black);
                            DrawingFont.DrawText(Sprite, NextPerson.Name, new Rectangle(new Point((int)(NextPerson.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 64,
                                (int)(NextPerson.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32 - 16), new Size(128, 16)), DrawTextFormat.Center, ColorPalette[NextPerson.ItemLevel]);
                        }

                        int Number = 0;
                        foreach (CombatText NextCombatText in NextPerson.CombatTexts)
                        {
                            DrawingFont.DrawText(Sprite, NextCombatText.Value.ToString(), new Rectangle(new Point((int)(NextPerson.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32 + 1,
                                (int)(NextPerson.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y + 16 + 1 - Number * 12), new Size(64, 16)), DrawTextFormat.Center, Color.Black);
                            DrawingFont.DrawText(Sprite, NextCombatText.Value.ToString(), new Rectangle(new Point((int)(NextPerson.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextPerson.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y + 16 - Number * 12), new Size(64, 16)), DrawTextFormat.Center, NextCombatText.Value < 0 ? Color.Red : Color.Green);
                            Number++;
                        }
                    }
                    finally { NextPerson.CombatTexts_Locker.ExitReadLock(); }

                }
            }
            finally { Game.Persons_Locker.ExitReadLock(); }
        }

        private void DrawHeroes(Sprite Sprite)
        {
            Game.Heroes_Locker.EnterReadLock();
            try
            {
                foreach (Hero NextHero in Game.Heroes)
                {
                    int Damage = (int)(((double)(NextHero.MaxEnergy - NextHero.Energy) / NextHero.MaxEnergy) * 64);
                    Sprite.Draw2D(NextHero.Icon, Point.Empty, 0f,
                        new Point((int)(NextHero.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                            (int)(NextHero.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
                    Sprite.Draw2D(NextHero.Icon, new Rectangle(0, 64 - Damage, 64, Damage), new Rectangle(0, 0, 64, Damage),
                        new Point((int)(NextHero.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 31,
                            (int)(NextHero.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32 + 64 - Damage), Color.Red);

                    if (NextHero.Faction == Game.Character.Faction)
                    {
                        if (NextHero.InGroup)
                            Sprite.Draw2D(Border_Group, Point.Empty, 0f, new Point((int)(NextHero.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                    (int)(NextHero.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
                        else
                            Sprite.Draw2D(Border_Friend, Point.Empty, 0f, new Point((int)(NextHero.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                    (int)(NextHero.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);
                    }
                    else
                        Sprite.Draw2D(Border_Enemy, Point.Empty, 0f, new Point((int)(NextHero.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextHero.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32), Color.White);

                    NextHero.CombatTexts_Locker.EnterReadLock();
                    try
                    {
                        if (NextHero.CombatTexts.Count < 5)
                        {
                            DrawingFont.DrawText(Sprite, NextHero.Name, new Rectangle(new Point((int)(NextHero.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 64 + 1,
                                (int)(NextHero.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32 - 16 + 1), new Size(128, 16)), DrawTextFormat.Center, Color.Black);
                            DrawingFont.DrawText(Sprite, NextHero.Name, new Rectangle(new Point((int)(NextHero.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 64,
                                (int)(NextHero.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y - 32 - 16), new Size(128, 16)), DrawTextFormat.Center, ColorPalette[NextHero.ItemLevel]);
                        }

                        int Number = 0;
                        foreach (CombatText NextCombatText in NextHero.CombatTexts)
                        {
                            DrawingFont.DrawText(Sprite, NextCombatText.Value.ToString(), new Rectangle(new Point((int)(NextHero.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32 + 1,
                                (int)(NextHero.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y + 16 + 1 - Number * 12), new Size(64, 16)), DrawTextFormat.Center, Color.Black);
                            DrawingFont.DrawText(Sprite, NextCombatText.Value.ToString(), new Rectangle(new Point((int)(NextHero.Location.X - Game.Character.Location.X) + ClientSize.Width / 2 + Offset.X - 32,
                                (int)(NextHero.Location.Y - Game.Character.Location.Y) + ClientSize.Height / 2 + Offset.Y + 16 - Number * 12), new Size(64, 16)), DrawTextFormat.Center, NextCombatText.Value < 0 ? Color.Red : Color.Green);
                            Number++;
                        }
                    }
                    finally { NextHero.CombatTexts_Locker.ExitReadLock(); }
                }
            }
            finally { Game.Heroes_Locker.ExitReadLock(); }

            int HeroDamage = (int)(((double)(Game.Character.MaxEnergy - Game.Character.Energy) / Game.Character.MaxEnergy) * 64);
            Sprite.Draw2D(Game.Character.Icon, Point.Empty, 0f, new Point(ClientSize.Width / 2 + Offset.X - 32, ClientSize.Height / 2 + Offset.Y - 32), Game.Character.Invisible ? Color.Gray : Color.White);
            if (HeroDamage != 0)
                Sprite.Draw2D(Game.Character.Icon, new Rectangle(0, 64 - HeroDamage, 64, HeroDamage), new Rectangle(0, 0, 64, HeroDamage),
                new Point(ClientSize.Width / 2 + Offset.X - 31, ClientSize.Height / 2 + Offset.Y - 32 + 64 - HeroDamage), Color.Red);

            Game.Character.CombatTexts_Locker.EnterReadLock();
            try
            {
                if (Game.Character.CombatTexts.Count < 5)
                {
                    DrawingFont.DrawText(Sprite, Game.Character.Name, new Rectangle(new Point(ClientSize.Width / 2 + Offset.X - 64 + 1, ClientSize.Height / 2 + Offset.Y - 32 - 16 + 1), new Size(128, 16)), DrawTextFormat.Center, Color.Black);
                    DrawingFont.DrawText(Sprite, Game.Character.Name, new Rectangle(new Point(ClientSize.Width / 2 + Offset.X - 64, ClientSize.Height / 2 + Offset.Y - 32 - 16), new Size(128, 16)), DrawTextFormat.Center, ColorPalette[Game.Character.ItemLevel]);
                }

                int Number = 0;
                foreach (CombatText NextCombatText in Game.Character.CombatTexts)
                {
                    DrawingFont.DrawText(Sprite, NextCombatText.Value.ToString(), new Rectangle(new Point(ClientSize.Width / 2 + Offset.X - 32 + 1, ClientSize.Height / 2 + Offset.Y + 16 + 1 - Number * 12), new Size(64, 16)), DrawTextFormat.Center, Color.Black);
                    DrawingFont.DrawText(Sprite, NextCombatText.Value.ToString(), new Rectangle(new Point(ClientSize.Width / 2 + Offset.X - 32, ClientSize.Height / 2 + Offset.Y + 16 - Number * 12), new Size(64, 16)), DrawTextFormat.Center, NextCombatText.Value < 0 ? Color.Red : Color.Green);

                    Number++;
                }
            }
            finally { Game.Character.CombatTexts_Locker.ExitReadLock(); }
        }
        #endregion

        public enum Panels
        {
            None,
            Character,
            Attributes,
            Group,
            Quest,
            Dialog
        };

        #region Game_Interface
        private static Microsoft.DirectX.Direct3D.Font CooldownFont;
        private static Microsoft.DirectX.Direct3D.Font RankFont;
        private static Microsoft.DirectX.Direct3D.Font KeyFont;
        private static Microsoft.DirectX.Direct3D.Font MarkFont;
        private Point Offset = new Point(0, 0);

        private Texture[] Bases = new Texture[4];
        private void DrawBattlefield(Sprite Sprite)
        {
            if (Game.Arena)
            {
                DrawingFont.DrawText(Sprite, "Arena timer: " + (DateTime.Now - Game.ArenaTimer).Minutes + ":" + (DateTime.Now - Game.ArenaTimer).Seconds,
                    new Rectangle(ClientSize.Width / 2 - 100 + Offset.X, Offset.Y + 8, 200, 16), DrawTextFormat.Center, Color.Red);

                /*if (Game.Battlefield != -1)
                {
                    Sprite.Draw2D(Bases[Game.Character.Faction + 1], Point.Empty, 0f, new Point(ClientSize.Width / 2 + Offset.X - 18, Offset.Y + 56), Color.White);
                    DrawingFont.DrawText(Sprite, ": " + Game.Battlefields[Game.Battlefield].Flags_Occupied, new Point(ClientSize.Width / 2 + Offset.X + 6, Offset.Y + 54), Color.White);
                    Sprite.Draw2D(Bases[0], Point.Empty, 0f, new Point(ClientSize.Width / 2 + Offset.X - 18, Offset.Y + 80), Color.White);
                    DrawingFont.DrawText(Sprite, ": " + Game.Battlefields[Game.Battlefield].Flags_Unoccupied, new Point(ClientSize.Width / 2 + Offset.X + 6, Offset.Y + 78), Color.White);
                }*/
            }
            else
            {
                int Seconds = (int)(DateTime.Now - Game.EventTimer).TotalSeconds;
                if (Game.CalltoArms)
                {
                    DrawingFont.DrawText(Sprite, "Call to Arms for: " + (Program.CALLTOARMS_END / 1000 - Seconds) / 60 + ":" + (Program.CALLTOARMS_END / 1000 - Seconds) % 60,
                        new Rectangle(ClientSize.Width / 2 - 100 + Offset.X, Offset.Y + 8, 200, 16), DrawTextFormat.Center, Color.Red);
                    DrawingFont.DrawText(Sprite, Battlefields[Game.Battlefield + 1], new Rectangle(ClientSize.Width / 2 - 100 + Offset.X, Offset.Y + 32, 200, 16), DrawTextFormat.Center, Color.White);
                    if (Game.Battlefield != -1)
                    {
                        Sprite.Draw2D(Bases[Game.Character.Faction + 1], Point.Empty, 0f, new Point(ClientSize.Width / 2 + Offset.X - 18, Offset.Y + 56), Color.White);
                        DrawingFont.DrawText(Sprite, ": " + Game.Battlefields[Game.Battlefield].Flags_Occupied, new Point(ClientSize.Width / 2 + Offset.X + 6, Offset.Y + 54), Color.White);
                        Sprite.Draw2D(Bases[0], Point.Empty, 0f, new Point(ClientSize.Width / 2 + Offset.X - 18, Offset.Y + 80), Color.White);
                        DrawingFont.DrawText(Sprite, ": " + Game.Battlefields[Game.Battlefield].Flags_Unoccupied, new Point(ClientSize.Width / 2 + Offset.X + 6, Offset.Y + 78), Color.White);
                    }
                }
                else
                {
                    DrawingFont.DrawText(Sprite, "Call to Arms in: " + (Program.CALLTOARMS_START / 1000 - Seconds) / 60 + ":" + (Program.CALLTOARMS_START - Seconds) % 60,
                        new Rectangle(ClientSize.Width / 2 - 100 + Offset.X, 8 + Offset.Y, 200, 16), DrawTextFormat.Center, Color.White);
                }
            }

            for (int Current = 0; Current < Game.Messages_Count; Current++)
                DrawingFont.DrawText(Sprite, Game.Messages[Current].Text, new Rectangle(ClientSize.Width / 2 - 100 + Offset.X, 8 + Offset.Y + 64 + Current * 16, 200, 16), DrawTextFormat.Center, Color.Red);
        }

        private Mark Tooltip_Mark;
        private Texture Border_Supportive;
        private Texture Border_Unsupportive;
        private void DrawMarks(Sprite Sprite)
        {
            int Number = 0;
            Point Location = new Point(2 * (Offset.X < 0 ? 0 : Offset.X) + 4, Offset.Y + 4);

            Game.Character.Marks_Locker.EnterReadLock();
            try
            {
                Tooltip_Mark = null;
                foreach (Mark NextMark in Game.Character.Marks)
                {
                    Sprite.Draw2D(NextMark.Icon, Point.Empty, 0f, new Point(Location.X + (Number % 6) * 68, Location.Y + (Number / 6) * 68), Color.White);
                    int Length = (int)((NextMark.FullDuration - NextMark.Duration) / NextMark.FullDuration * 64);
                    if (Length != 0)
                        Sprite.Draw2D(NextMark.Icon, new Rectangle(0, 0, 64, Length), new Rectangle(0, 0, 64, Length),
                            new Point(Location.X + (Number % 6) * 68 + 1, Location.Y + (Number / 6) * 68), Color.Gray);

                    Sprite.Draw2D(NextMark.Supportive ? Border_Supportive : Border_Unsupportive, Point.Empty, 0f, new Point(Location.X + (Number % 6) * 68, Location.Y + (Number / 6) * 68), Color.White);

                    CooldownFont.DrawText(Sprite, NextMark.Stack.ToString(), new Point(Location.X + (Number % 6) * 68 + 40 + 1,
                        Location.Y + (Number / 6) * 68 - 8 + 32 + 1), Color.Black);
                    CooldownFont.DrawText(Sprite, NextMark.Stack.ToString(), new Point(Location.X + (Number % 6) * 68 + 40,
                        Location.Y + (Number / 6) * 68 - 8 + 32), Color.Yellow);


                    if (Collide(new Rectangle(Location.X + (Number % 6) * 68, Location.Y + (Number / 6) * 68, 64, 64), CursorLocation))
                        Tooltip_Mark = NextMark;

                    Number++;
                }
            }
            finally { Game.Character.Marks_Locker.ExitReadLock(); }

            Number = 0;
            Location = new Point(ClientSize.Width + 2 * (Offset.X < 0 ? Offset.X : 0) - 4 - 68, Offset.Y + 4);

            Game.Character.Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Game.Character.Impacts)
                {
                    int Length = (int)(NextImpact.Duration / NextImpact.FullDuration * 64);
                    Sprite.Draw2D(NextImpact.Icon, Point.Empty, 0f, new Point(Location.X - (Number % 6) * 68, Location.Y + (Number / 6) * 68), Color.White);
                    if (Length != 0)
                        Sprite.Draw2D(NextImpact.Icon, new Rectangle(0, 0, 64, Length), new Rectangle(0, 0, 64, Length),
                            new Point(Location.X - (Number % 6) * 68 + 1, Location.Y + (Number / 6) * 68), Color.Gray);

                    Sprite.Draw2D(Mark.Support[NextImpact.EffectID] ? Border_Supportive : Border_Unsupportive, Point.Empty, 0f, new Point(Location.X - (Number % 6) * 68, Location.Y + (Number / 6) * 68), Color.White);

                    CooldownFont.DrawText(Sprite, (NextImpact.Rank + 1).ToString(), new Point(Location.X - (Number % 6) * 68 + 40 + 1,
                        Location.Y + (Number / 6) * 68 + 24 + 1), Color.Black);
                    CooldownFont.DrawText(Sprite, (NextImpact.Rank + 1).ToString(), new Point(Location.X - (Number % 6) * 68 + 40,
                        Location.Y + (Number / 6) * 68 + 24), Color.Yellow);

                    Number++;
                }
            }
            finally { Game.Character.Impacts_Locker.ExitReadLock(); }
        }

        private Spell Tooltip_Spell;
        private int Casting_Rank = -1;
        private int Casting_Spell = -1;
        private Texture Spell_Selected;
        private Texture Spell_Casting;
        private void DrawSpells(Sprite Sprite)
        {
            Tooltip_Spell = null;
            for (int Current = 0; Current < 6; Current++)
            {
                Sprite.Draw2D(Game.Character.Spells[Current].Icon, Point.Empty, 0f, new Point(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X, ClientSize.Height - 64 - 8), Game.Character.Muted ? Color.Gray : Color.White);
                RankFont.DrawText(Sprite, (Game.Character.Spells[Current].RandomBonusRank + 1).ToString(),
                    new Point(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X + 18 + 1, ClientSize.Height - 64 - 4 + 1), Color.Black);
                RankFont.DrawText(Sprite, (Game.Character.Spells[Current].RandomBonusRank + 1).ToString(),
                    new Point(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X + 18, ClientSize.Height - 64 - 4), Color.White);

                if (!Game.Character.Muted)
                {
                    if (Game.Character.Spells[Current].Cooldown != 0)
                    {
                        int Cooldown = (int)(Game.Character.Spells[Current].Cooldown / Game.Character.Spells[Current].FullCooldown * 64);
                        Sprite.Draw2D(Game.Character.Spells[Current].Icon, new Rectangle(0, 64 - Cooldown, 64, Cooldown), new Rectangle(0, 0, 64, Cooldown),
                            new Point(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X + 1, ClientSize.Height - Cooldown - 8), Color.DarkSlateGray);
                    }
                    else
                        if (Casting_Spell == -1)
                        {
                            KeyFont.DrawText(Sprite, Game.Character.Spells[Current].KeyData.ToString(), new Point(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X + 6 + 1,
                                ClientSize.Height - 64 - 12 + 1), Color.Black);
                            KeyFont.DrawText(Sprite, Game.Character.Spells[Current].KeyData.ToString(), new Point(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X + 6,
                                ClientSize.Height - 64 - 12), Color.White);
                        }

                    if (Casting_Spell == Current)
                        if (Casting_Rank != -1)
                            Sprite.Draw2D(Spell_Casting, Point.Empty, 0f, new Point(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X, ClientSize.Height - 64 - 8), Color.White);
                        else
                            Sprite.Draw2D(Spell_Selected, Point.Empty, 0f, new Point(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X, ClientSize.Height - 64 - 8), Color.White);
                    {
                        /*if (Casting_Rank != -1)
                            RankFont.DrawText(Sprite, (Casting_Rank + 1).ToString(),
                                new Point(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X + 18, ClientSize.Height - 64 - 4), Color.Red);*/
                    }
                }

                if (Collide(new Rectangle(Current * 64 + (ClientSize.Width - 6 * 64) / 2 + Offset.X, ClientSize.Height - 64 - 8, 64, 64), CursorLocation))
                    Tooltip_Spell = Game.Character.Spells[Current];
            }
        }

        private Texture Panel_Left;
        private Texture Panel_Right;
        private Texture Panel_Dialog;

        private Panels CurrentPanel = Panels.None;
        private void DrawPanels(Sprite Sprite)
        {
            switch (CurrentPanel)
            {
                case Panels.Character: DrawPanels_Character(Sprite); break;
                case Panels.Attributes: DrawPanels_Attributes(Sprite); break;
                case Panels.Group: DrawPanels_Group(Sprite); break;
                case Panels.Quest: DrawPanels_Quest(Sprite); break;
                case Panels.Dialog: DrawPanels_Dialog(Sprite); break;
            }
        }

        private Texture[] EmptySlots = new Texture[9];
        private void DrawPanels_Character(Sprite Sprite)
        {
            Sprite.Draw2D(Panel_Left, Rectangle.Empty, new Rectangle(0, 0, 384, 768), Point.Empty, 0f, new Point(0, 0), Color.White);
            RankFont.DrawText(Sprite, Game.Character.Name, new Rectangle(0 + 1, 16 + 1, 470, 64), DrawTextFormat.Center, Color.Black);
            RankFont.DrawText(Sprite, Game.Character.Name, new Rectangle(0, 16, 470, 64), DrawTextFormat.Center, Faction_Colors[Game.Character.Faction]);

            //Equipped Items
            for (int Current = 0; Current < 10; Current++)
                if (Game.Character.Equipped[Slots[Current].SlotID] != null) Sprite.Draw2D(Game.Character.Equipped[Slots[Current].SlotID].Icon, Point.Empty, 0f, Slots[Current].Location, Color.White);
                else Sprite.Draw2D(EmptySlots[Slots[Current].SlotID], Point.Empty, 0f, Slots[Current].Location, Color.White);

            //Spells
            for (int Column = 0; Column < 2; Column++)
                for (int Row = 0; Row < 3; Row++)
                    Sprite.Draw2D(Game.Character.Spells[Row * 2 + Column].Icon, Point.Empty, 0f, new Point(Row * 72 + 6 + 64, Column * 72 + 448 - 38 - 32), Color.White);

            //Loot Items
            for (int Current = 0; Current < 5; Current++)
                if (Game.Character.Equipment_Loot[Current] != null) Sprite.Draw2D(Game.Character.Equipment_Loot[Current].Icon, Point.Empty, 0f, new Point(Current * 64 + 14, 576 - 30), Color.White);
                else Sprite.Draw2D(EmptySlots[8], Point.Empty, 0f, new Point(Current * 64 + 14, 576 - 30), Color.White);

            //Backpack Items
            for (int Column = 0; Column < 2; Column++)
                for (int Row = 0; Row < 5; Row++)
                    if (Game.Character.Equipment_Backpack[Column * 5 + Row] != null) Sprite.Draw2D(Game.Character.Equipment_Backpack[Column * 5 + Row].Icon, Point.Empty, 0f, new Point(Row * 64 + 14, Column * 64 + 640 - 14), Color.White);
                    else Sprite.Draw2D(EmptySlots[8], Point.Empty, 0f, new Point(Row * 64 + 14, Column * 64 + 640 - 14), Color.White);
        }

        private static Random Random = new Random();
        private uint Slot = (uint)Random.Next(7);
        private uint School = (uint)Random.Next(6);
        private int Attributes_Number = Random.Next(3) + 1;
        private uint[] Attributes = new uint[3] { (uint)Random.Next(7), (uint)Random.Next(7), (uint)Random.Next(7) };
        private Texture[] Buttons = new Texture[2];
        private void DrawPanels_Attributes(Sprite Sprite)
        {
            Sprite.Draw2D(Panel_Left, Rectangle.Empty, new Rectangle(0, 0, 384, 768), Point.Empty, 0f, new Point(0, 0), Color.White);
            RankFont.DrawText(Sprite, "Attributes", new Rectangle(0 + 1, 16 + 1, 470, 64), DrawTextFormat.Center, Color.Black);
            RankFont.DrawText(Sprite, "Attributes", new Rectangle(0, 16, 470, 64), DrawTextFormat.Center, Faction_Colors[Game.Character.Faction]);

            RankFont.DrawText(Sprite, "Energy:", new Point(16 + 1, 64 + 1), Color.Black);
            RankFont.DrawText(Sprite, "Energy:", new Point(16, 64), Color.White);
            RankFont.DrawText(Sprite, Game.Character.Energy + "/" + Game.Character.MaxEnergy, new Rectangle(128, 64 + 1, 320, 64), DrawTextFormat.Right, Color.Black);
            RankFont.DrawText(Sprite, Game.Character.Energy + "/" + Game.Character.MaxEnergy, new Rectangle(128, 64, 320, 64), DrawTextFormat.Right, Color.White);

            RankFont.DrawText(Sprite, "Accuracy:", new Point(16 + 1, 96 + 1), Color.Black);
            RankFont.DrawText(Sprite, "Accuracy:", new Point(16, 96), Color.White);
            RankFont.DrawText(Sprite, Game.Character.Global_Accuracy.ToString(), new Rectangle(448 - 128, 96 + 1, 128, 64), DrawTextFormat.Right, Color.Black);
            RankFont.DrawText(Sprite, Game.Character.Global_Accuracy.ToString(), new Rectangle(448 - 128, 96, 128, 64), DrawTextFormat.Right, Color.White);

            RankFont.DrawText(Sprite, "Clearcast:", new Point(16 + 1, 128 + 1), Color.Black);
            RankFont.DrawText(Sprite, "Clearcast:", new Point(16, 128), Color.White);
            RankFont.DrawText(Sprite, Game.Character.Global_ClearcastChance.ToString(), new Rectangle(448 - 128, 128 + 1, 128, 64), DrawTextFormat.Right, Color.Black);
            RankFont.DrawText(Sprite, Game.Character.Global_ClearcastChance.ToString(), new Rectangle(448 - 128, 128, 128, 64), DrawTextFormat.Right, Color.White);

            RankFont.DrawText(Sprite, "Haste:", new Point(16 + 1, 160 + 1), Color.Black);
            RankFont.DrawText(Sprite, "Haste:", new Point(16, 160), Color.White);
            RankFont.DrawText(Sprite, Game.Character.Global_Haste.ToString(), new Rectangle(448 - 128, 160 + 1, 128, 64), DrawTextFormat.Right, Color.Black);
            RankFont.DrawText(Sprite, Game.Character.Global_Haste.ToString(), new Rectangle(448 - 128, 160, 128, 64), DrawTextFormat.Right, Color.White);

            RankFont.DrawText(Sprite, "Power:", new Point(16 + 1, 192 + 1), Color.Black);
            RankFont.DrawText(Sprite, "Power:", new Point(16, 192), Color.White);
            RankFont.DrawText(Sprite, Game.Character.Global_Power.ToString(), new Rectangle(448 - 128, 192 + 1, 128, 64), DrawTextFormat.Right, Color.Black);
            RankFont.DrawText(Sprite, Game.Character.Global_Power.ToString(), new Rectangle(448 - 128, 192, 128, 64), DrawTextFormat.Right, Color.White);

            RankFont.DrawText(Sprite, "Resistance:", new Point(16 + 1, 224 + 1), Color.Black);
            RankFont.DrawText(Sprite, "Resistance:", new Point(16, 224), Color.White);
            RankFont.DrawText(Sprite, Game.Character.Global_Resistance.ToString(), new Rectangle(448 - 128, 224 + 1, 128, 64), DrawTextFormat.Right, Color.Black);
            RankFont.DrawText(Sprite, Game.Character.Global_Resistance.ToString(), new Rectangle(448 - 128, 224, 128, 64), DrawTextFormat.Right, Color.White);

            RankFont.DrawText(Sprite, "Reputation: ", new Point(16 + 1, 288 + 1), Color.Black);
            RankFont.DrawText(Sprite, "Reputation: ", new Point(16, 288), Color.White);
            RankFont.DrawText(Sprite, Game.Character.Reputation.ToString(), new Rectangle(448 - 180, 288 + 1, 180, 64), DrawTextFormat.Right, Color.Black);
            RankFont.DrawText(Sprite, Game.Character.Reputation.ToString(), new Rectangle(448 - 180, 288, 180, 64), DrawTextFormat.Right, Color.White);

            RankFont.DrawText(Sprite, School_Names[School], new Rectangle(16 + 1, 320 + 1, 448, 64), DrawTextFormat.Center, Color.Black);
            RankFont.DrawText(Sprite, School_Names[School], new Rectangle(16, 320, 448, 64), DrawTextFormat.Center, School_Colors[School]);
            RankFont.DrawText(Sprite, Slot_Names[Slot], new Rectangle(16 + 1, 352 + 1, 448, 64), DrawTextFormat.Center, Color.Black);
            RankFont.DrawText(Sprite, Slot_Names[Slot], new Rectangle(16 , 352 , 448, 64), DrawTextFormat.Center, Color.White);
            RankFont.DrawText(Sprite, "Attributes:  " + Attributes_Number, new Rectangle(16+1, 384+1, 448, 64), DrawTextFormat.Center, Color.Black);
            RankFont.DrawText(Sprite, "Attributes:  " + Attributes_Number, new Rectangle(16, 384, 448, 64), DrawTextFormat.Center, Color.White);

            for (int Current = 0; Current < Attributes_Number; Current++)
            {
                RankFont.DrawText(Sprite, Attribute_Names[Attributes[Current]], new Rectangle(16 + 1, 448 + 1 + Current * 32, 448, 64), DrawTextFormat.Center, Color.Black);
                RankFont.DrawText(Sprite, Attribute_Names[Attributes[Current]], new Rectangle(16, 448 + Current * 32, 448, 64), DrawTextFormat.Center, Color.White);
            }

            Sprite.Draw2D(Buttons[0], new Rectangle(0, 0, 170, 40), new Rectangle(0, 0, 170, 40), new Point(16 + 80, 448 + 0 * 32), Color.White);
            KeyFont.DrawText(Sprite, "Buy Item", new Rectangle(16, 448+4, 448-128+16, 64), DrawTextFormat.Center, Color.Red);
        }

        public bool InArenaQueue = false;
        private void DrawPanels_Group(Sprite Sprite)
        {
            Sprite.Draw2D(Panel_Right, Rectangle.Empty, new Rectangle(0, 0, 384, 768), Point.Empty, 0f, new Point(ClientSize.Width - 170, 0), Color.White);
            RankFont.DrawText(Sprite, "Group", new Rectangle(ClientSize.Width - 150 + 1, 16 + 1, 512, 64), DrawTextFormat.Center, Color.Black);
            RankFont.DrawText(Sprite, "Group", new Rectangle(ClientSize.Width - 150, 16, 512, 64), DrawTextFormat.Center, Faction_Colors[Game.Character.Faction]);

            Game.Character.Group.Members_Locker.EnterReadLock();
            try
            {
                for (int Current = 0; Current < Game.Character.Group.Members.Count; Current++)
                {
                    Sprite.Draw2D(Game.Character.Group.Members[Current].Icon, Point.Empty, 0f, new Point(ClientSize.Width + 2 * Offset.X + 32 + 32, Current * 64 + 64), Color.White);
                    DrawingFont.DrawText(Sprite, Game.Character.Group.Members[Current].Name, new Point(ClientSize.Width + 2 * Offset.X + 104 + 32, Current * 64 + 64), Color.Orange);
                }
            }
            finally { Game.Character.Group.Members_Locker.ExitReadLock(); }

            Sprite.Draw2D(Buttons[0], new Rectangle(0, 0, 170, 40), new Rectangle(0, 0, 170, 40), new Point(ClientSize.Width - 256, 448 + 0 * 32), Color.White);
            KeyFont.DrawText(Sprite, "Join Arena", new Rectangle(ClientSize.Width - 256, 448 + 4, 448 - 256 - 32, 64), DrawTextFormat.Center, InArenaQueue? Color.Gray : Color.Green);


            int Scale = (int)(256 / MapTiles_Size.Width);
            int Space = 512 - Scale + 128;
            Point Location = new Point((int)(Game.Character.Location.X / 512), (int)(Game.Character.Location.Y / 512));
            for (int Column = 0; Column < MapTiles_Size.Height; Column++)
                for (int Row = 0; Row < MapTiles_Size.Width; Row++)
                    Sprite.Draw2D(MapTiles[Row, Column], new Rectangle(0, 0, 512, 512), new Rectangle(0, 0, Scale, Scale), new Point(8 * Space + Row * 512, 24 * Space + Column * 512), (Row == Location.X && Column == Location.Y) ? Color.White : Color.Gray);


            /*Point Location = new Point((int)(Game.Character.Location.X / 512), (int)(Game.Character.Location.Y / 512));
              for (int Column = 0; Column < MapTiles_Size.Height; Column++)
                  for (int Row = 0; Row < MapTiles_Size.Width; Row++)
                      Sprite.Draw2D(MapTiles[Row, Column], new Rectangle(0, 0, 512, 512), new Rectangle(0, 0, (int)(64 * 512 / (MapTiles_Size.Width * 128)), (int)(64 * 512 / (MapTiles_Size.Width * 128))),
                          new Point((int)(32*Scale) + Row * 512, (int)(512*Scale)  + Column * 512), (Row == Location.X && Column == Location.Y) ? Color.White : Color.Gray);*/
        }

        private void DrawPanels_Quest(Sprite Sprite)
        {
            Sprite.Draw2D(Panel_Right, Rectangle.Empty, new Rectangle(0, 0, 384, 768), Point.Empty, 0f, new Point(ClientSize.Width - 170, 0), Color.White);
            RankFont.DrawText(Sprite, "Quests", new Rectangle(ClientSize.Width - 150 + 1, 16 + 1, 512, 64), DrawTextFormat.Center, Color.Black);
            RankFont.DrawText(Sprite, "Quests", new Rectangle(ClientSize.Width - 150, 16, 512, 64), DrawTextFormat.Center, Faction_Colors[Game.Character.Faction]);
        }

        private string Dialog;
        private int[] Answers;
        private int CurrentAnswer = -1;
        private Person TargetPerson;
        private void DrawPanels_Dialog(Sprite Sprite)
        {
            Sprite.Draw2D(Panel_Dialog, Point.Empty, 0f, new Point(512 - 384, 72), Color.White);

            KeyFont.DrawText(Sprite, Dialog, new Rectangle(512 - 384 + 16, 72 + 16, 384 - 32, 512 - 32), DrawTextFormat.WordBreak, Color.White);
            for (int Current = 0; Current < Answers.Length; Current++)
                KeyFont.DrawText(Sprite, "Anser#"+Answers[Current]+": "+Person.Answers[Answers[Current]], new Rectangle(512 + 16, 72 + 16 + Current * 64, 384 - 32, 64), DrawTextFormat.WordBreak, CurrentAnswer == Current ? Color.Yellow : Color.White);
        }

        private void DrawChat(Sprite Sprite)
        {
            if (CurrentPanel == Panels.None || CurrentPanel == Panels.Dialog)
            {
                DrawingFont.DrawText(Sprite, Game.Chat_LeftString, new Rectangle(6 + 1, ClientSize.Height - 128 + 1, 308, 128), DrawTextFormat.WordBreak, Color.Black);
                DrawingFont.DrawText(Sprite, Game.Chat_LeftString, new Rectangle(6, ClientSize.Height - 128, 308, 128), DrawTextFormat.WordBreak, Color.White);

                DrawingFont.DrawText(Sprite, Game.Chat_RightString, new Rectangle(ClientSize.Width - 308 + 1, ClientSize.Height - 128 + 1, 308, 128), DrawTextFormat.WordBreak, Color.Black);
                DrawingFont.DrawText(Sprite, Game.Chat_RightString, new Rectangle(ClientSize.Width - 308, ClientSize.Height - 128, 308, 128), DrawTextFormat.WordBreak, Color.White);
            }
            else
            {
                DrawingFont.DrawText(Sprite, Game.Chat_LeftString, new Rectangle((6 + 2 * Offset.X < 0 ? 6 : 6 + 2 * Offset.X) + 1, ClientSize.Height - 230 + 1, 280, 128), DrawTextFormat.WordBreak, Color.Black);
                DrawingFont.DrawText(Sprite, Game.Chat_LeftString, new Rectangle(6 + 2 * Offset.X < 0 ? 6 : 6 + 2 * Offset.X, ClientSize.Height - 230, 280, 128), DrawTextFormat.WordBreak, Color.White);

                DrawingFont.DrawText(Sprite, Game.Chat_RightString, new Rectangle((ClientSize.Width < ClientSize.Width + 2 * Offset.X ? ClientSize.Width - 280 : ClientSize.Width + 2 * Offset.X - 280) + 1, ClientSize.Height - 230 + 1, 280, 128), DrawTextFormat.WordBreak, Color.Black);
                DrawingFont.DrawText(Sprite, Game.Chat_RightString, new Rectangle(ClientSize.Width < ClientSize.Width + 2 * Offset.X ? ClientSize.Width - 280 : ClientSize.Width + 2 * Offset.X - 280, ClientSize.Height - 230, 280, 128), DrawTextFormat.WordBreak, Color.White);
            }

            if (ChatMode)
            {
                string Channel = "Say: ";
                switch (Token)
                {
                    case "/f": Channel = "Faction: "; break;
                    case "/b": Channel = "Battleground: "; break;
                    case "/g": Channel = "Group: "; break;
                }

                DrawingFont.DrawText(Sprite, Channel + new string(chars, 0, pointer)+"|", new Rectangle(320 + Offset.X + 1, ClientSize.Height - 96 + Offset.Y + 1, 384, 20), DrawTextFormat.Left, Color.Black);
                DrawingFont.DrawText(Sprite, Channel + new string(chars, 0, pointer)+"|", new Rectangle(320 + Offset.X, ClientSize.Height + Offset.Y - 96, 384, 20), DrawTextFormat.Left, Color.White);
            }
        }

        private void DrawTargetFrame(Sprite Sprite)
        {
            if (Game.Target != null)
            {
                Game.Target_Locker.EnterWriteLock();
                try
                {
                    Sprite.Draw2D(Game.Target.Icon, Point.Empty, 0f, new Point(ClientSize.Width / 2 - 32 + Offset.X, ClientSize.Height - 164 + Offset.Y), Color.White);
                    int TargetDamage = (int)(((float)(Game.Target.MaxEnergy - Game.Target.Energy) / Game.Target.MaxEnergy) * 64);
                    if (TargetDamage != 0)
                        Sprite.Draw2D(Game.Target.Icon, new Rectangle(0, 64 - TargetDamage, 64, TargetDamage), new Rectangle(0, 0, 64, TargetDamage),
                        new Point(ClientSize.Width / 2 - 31 + Offset.X, ClientSize.Height - 164 + Offset.Y + 64 - TargetDamage), Color.Red);
                    DrawingFont.DrawText(Sprite, Game.Target.Name, new Rectangle(new Point(ClientSize.Width / 2 + Offset.X - 64 + 1, ClientSize.Height - 164 + Offset.Y - 16 + 1), new Size(128, 16)), DrawTextFormat.Center, Color.Black);
                    DrawingFont.DrawText(Sprite, Game.Target.Name, new Rectangle(new Point(ClientSize.Width / 2 + Offset.X - 64, ClientSize.Height - 164 + Offset.Y - 16), new Size(128, 16)), DrawTextFormat.Center, Color.White);

                    int Current = 0;
                    Game.Target.Marks_Locker.EnterReadLock();
                    try
                    {
                        foreach (Mark NextMark in Game.Target.Marks)
                        {
                            Sprite.Draw2D(NextMark.Icon, Rectangle.Empty, new Rectangle(0, 0, 32, 32),
                                new Point((ClientSize.Width / 2 - 32 - 32 - 8 - (Current / 2) * 32 + Offset.X) * 2, (ClientSize.Height - 164 + (Current % 2) * 32 + Offset.Y) * 2), Color.White);
                            Sprite.Draw2D(NextMark.Supportive ? Border_Supportive : Border_Unsupportive, Rectangle.Empty, new Rectangle(0, 0, 32, 32),
                                new Point((ClientSize.Width / 2 - 32 - 32 - 8 - (Current / 2) * 32 + Offset.X) * 2, (ClientSize.Height - 164 + (Current % 2) * 32 + Offset.Y) * 2), Color.White);

                            MarkFont.DrawText(Sprite, NextMark.Stack.ToString(), new Point((ClientSize.Width / 2 - 32 - 24 - 8 - (Current / 2) * 32 + Offset.X + 1) * 2, (ClientSize.Height - 168 + (Current % 2) * 32 + Offset.Y + 1) * 2), Color.Black);
                            MarkFont.DrawText(Sprite, NextMark.Stack.ToString(), new Point((ClientSize.Width / 2 - 32 - 24 - 8 - (Current / 2) * 32 + Offset.X) * 2, (ClientSize.Height - 168 + (Current % 2) * 32 + Offset.Y) * 2), Color.White);

                            Current++;
                        }
                    }
                    finally { Game.Target.Marks_Locker.ExitReadLock(); }

                    Current = 0;
                    Game.Target.Impacts_Locker.EnterReadLock();
                    try
                    {
                        foreach (Impact NextImpact in Game.Target.Impacts)
                        {
                            Sprite.Draw2D(NextImpact.Icon, Rectangle.Empty, new Rectangle(0, 0, 32, 32),
                                new Point((ClientSize.Width / 2 + 32 + 8 + (Current / 2) * 32 + Offset.X) * 2, (ClientSize.Height - 164 + (Current % 2) * 32 + Offset.Y) * 2), Color.White);
                            Sprite.Draw2D(Mark.Support[NextImpact.EffectID] ? Border_Supportive : Border_Unsupportive, Rectangle.Empty, new Rectangle(0, 0, 32, 32),
                                new Point((ClientSize.Width / 2 + 32 + 8 + (Current / 2) * 32 + Offset.X) * 2, (ClientSize.Height - 164 + (Current % 2) * 32 + Offset.Y) * 2), Color.White);

                            MarkFont.DrawText(Sprite, (NextImpact.Rank + 1).ToString(), new Point((ClientSize.Width / 2 + 32 + 16 + (Current / 2) * 32 + Offset.X + 1) * 2, (ClientSize.Height - 168 + (Current % 2) * 32 + Offset.Y + 1) * 2), Color.Black);
                            MarkFont.DrawText(Sprite, (NextImpact.Rank + 1).ToString(), new Point((ClientSize.Width / 2 + 32 + 16 + (Current / 2) * 32 + Offset.X) * 2, (ClientSize.Height - 168 + (Current % 2) * 32 + Offset.Y) * 2), Color.White);

                            Current++;
                        }
                    }
                    finally { Game.Target.Impacts_Locker.ExitReadLock(); }
                }
                finally { Game.Target_Locker.ExitWriteLock(); }
            }
        }

        private Texture Cursor_Hand;
        private Texture Cursor_Casting;
        private Texture[] Splashes = new Texture[4];

        private Item Showing_Item;
        private Spell Showing_Spell;

        public static bool Target;
        public static bool Splash;
        public static Point CursorLocation;
        private void DrawCursor(Sprite Sprite)
        {
            if (CurrentPanel == Panels.Character)
            {
                Point TooltipLocation = CursorLocation;

                if (Showing_Item != null)
                {
                    if (ClientSize.Height < CursorLocation.Y + (Showing_Item.Attributes.Length + (Game.Character.Global_SchoolPowers[Showing_Item.School] == 0 ? 4 : 6)) * 16+4)
                        TooltipLocation = new Point(CursorLocation.X, CursorLocation.Y - ((Showing_Item.Attributes.Length + (Game.Character.Global_SchoolPowers[Showing_Item.School] == 0 ? 5 : 6)) * 16+4));

                    DrawBox(Sprite, TooltipLocation, 7, Showing_Item.Attributes.Length + (Game.Character.Global_SchoolPowers[Showing_Item.School] == 0 ? 2 : 4));

                    TooltipLocation.X -= 8;
                    TooltipLocation.Y -= 8;

                    DrawingFont.DrawText(Sprite, Showing_Item.Name, new Point(TooltipLocation.X + 16, TooltipLocation.Y + 16), School_Colors[Showing_Item.School]);
                    DrawingFont.DrawText(Sprite, "Item level: " + Showing_Item.Level, new Point(TooltipLocation.X + 16, TooltipLocation.Y + 32), ColorPalette[Showing_Item.Level * 4]);

                    for (int Current = 0; Current < Showing_Item.Attributes.Length; Current++)
                        DrawingFont.DrawText(Sprite, "+" + Attribute_Names[Showing_Item.Attributes[Current].ID] + " : " + Showing_Item.Attributes[Current].Value.ToString(),
                            new Point(TooltipLocation.X + 16, TooltipLocation.Y + 64 + Current * 16), Color.White);

                    if (Game.Character.Global_SchoolPowers[Showing_Item.School] != 0)
                        DrawingFont.DrawText(Sprite, "+" + Attribute_Names[7] + " : " + Game.Character.Global_SchoolPowers[Showing_Item.School].ToString(),
                            new Point(TooltipLocation.X + 16, TooltipLocation.Y + 32 + (Showing_Item.Attributes.Length + 3) * 16), Color.White);
                    return;
                }

                if (Showing_Spell != null)
                {
                    if (ClientSize.Height < CursorLocation.Y + 10*16)
                        TooltipLocation = new Point(CursorLocation.X, CursorLocation.Y - 10*16);

                    DrawBox(Sprite, TooltipLocation, 6, 8);

                    TooltipLocation.X -= 8;
                    TooltipLocation.Y -= 8;

                    DrawingFont.DrawText(Sprite, Showing_Spell.Name, new Point(TooltipLocation.X + 16, TooltipLocation.Y + 16), School_Colors[Showing_Spell.Effect % 6]);
                    //DrawingFont.DrawText(Sprite, Spell_TypeNames[Showing_Spell.Type], new Point(TooltipLocation.X + 16, TooltipLocation.Y + 32), Color.White);
                    for (int Current = 0; Current < 3; Current++)
                        DrawingFont.DrawText(Sprite, Spell_ParameterNames[Current] + " : " + Showing_Spell.Parameters[Current].ToString(),
                            new Point(TooltipLocation.X + 16, TooltipLocation.Y + 48 + Current * 16), Color.White);
                    for (int Current = 0; Current < 3; Current++)
                        DrawingFont.DrawText(Sprite, Spell_EffectParameterNames[Current] + " : " + Showing_Spell.Effect_Parameters[Current].ToString(),
                            new Point(TooltipLocation.X + 16, TooltipLocation.Y + 112 + Current * 16), Color.White);
                    return;
                }
            }

            if (Tooltip_Mark != null)
            {
                DrawBox(Sprite, new Point(CursorLocation.X, CursorLocation.Y), 6, 1);
                DrawingFont.DrawText(Sprite, Mark.Tooltips[Tooltip_Mark.EffectID], new Rectangle(CursorLocation.X + 8, CursorLocation.Y + 8, 5 * 32, 64),
                    DrawTextFormat.WordBreak, Color.White);
                return;
            }

            if (Tooltip_Spell != null)
            {
                DrawBox(Sprite, new Point(CursorLocation.X, CursorLocation.Y - 96), 6, 3);
                DrawingFont.DrawText(Sprite, Spell.Names[Tooltip_Spell.Effect],
                    new Rectangle(CursorLocation.X+8 , CursorLocation.Y - 96+8 , 5 * 32, 64), DrawTextFormat.WordBreak, School_Colors[Tooltip_Spell.Effect % 6]);
                DrawingFont.DrawText(Sprite, Spell.Tooltips[Tooltip_Spell.Effect],
                    new Rectangle(CursorLocation.X+8 , CursorLocation.Y - 64+8, 5 * 32, 64), DrawTextFormat.WordBreak, Color.White);
            }


            if ((CursorLocation.X < ClientSize.Width + 2 * Offset.X) && (0 + 2 * Offset.X < CursorLocation.X) && Target)
            {
                Sprite.Draw2D(Cursor_Casting, Point.Empty, 0f, new Point(CursorLocation.X, CursorLocation.Y), Color.White);
                return;
            }

            Sprite.Draw2D(Cursor_Hand, Point.Empty, 0f, new Point(CursorLocation.X, CursorLocation.Y), Color.White);
        }
        #endregion

        private Texture[] Box = new Texture[9];
        private void DrawBox(Sprite Sprite, Point Location, int Width, int Height)
        {
            Location.X -= 8;
            Location.Y -= 8;
            Sprite.Draw2D(Box[0], Point.Empty, 0f, Location, Color.White);
            Sprite.Draw2D(Box[1], Point.Empty, 0f, new Point(Location.X + (Width - 1) * 32, Location.Y), Color.White);
            Sprite.Draw2D(Box[2], Point.Empty, 0f, new Point(Location.X + (Width - 1) * 32, Location.Y + (Height + 1) * 16), Color.White);
            Sprite.Draw2D(Box[3], Point.Empty, 0f, new Point(Location.X, Location.Y + (Height + 1) * 16), Color.White);

            for (int Current = 1; Current < Width - 1; Current++)
                Sprite.Draw2D(Box[4], Point.Empty, 0f, new Point(Location.X + Current * 32, Location.Y), Color.White);
            for (int Current = 2; Current < Height + 1; Current++)
                Sprite.Draw2D(Box[5], Point.Empty, 0f, new Point(Location.X + (2*Width - 1) * 16, Location.Y + Current * 16), Color.White);
            for (int Current = 1; Current < Width - 1; Current++)
                Sprite.Draw2D(Box[6], Point.Empty, 0f, new Point(Location.X + Current * 32, Location.Y + (Height + 1) * 16), Color.White);
            for (int Current = 2; Current < Height+1; Current++)
                Sprite.Draw2D(Box[7], Point.Empty, 0f, new Point(Location.X, Location.Y + Current * 16), Color.White);

            for (int Column = 2; Column < Height + 1; Column++)
                for (int Row = 1; Row < 2*Width - 1; Row++)
                    Sprite.Draw2D(Box[8], Point.Empty, 0f, new Point(Location.X + Row * 16, Location.Y + Column * 16), Color.White);
        }

        private Slot[] Slots = new Slot[10]
        {
            new Slot(0, new Point(74+0*70,80+2*70)),
            new Slot(1, new Point(74+2*70,80+2*70)),
            new Slot(2, new Point(74+1*70,80+0*70)),
            new Slot(3, new Point(74+0*70,80+0*70)),
            new Slot(3, new Point(74+2*70,80+0*70)),
            new Slot(4, new Point(74+1*70,80+1*70)),
            new Slot(5, new Point(74+0*70,80+1*70)),
            new Slot(5, new Point(74+2*70,80+1*70)),
            new Slot(6, new Point(74+1*70,80+2*70)),
            new Slot(7, new Point(74+1*70,80+3*70))
        };

        private string[] Slot_Names = new string[9] { "Main Hand", "Off Hand", "Head", "Shoulders", "Chest", "Hands", "Legs", "Feet", "Glyph" };

        private string[] School_Names = new string[6] { "Fire", "Arcane", "Frost", "Nature", "Shadow", "Holy" };

        private Color[] School_Colors = new Color[6] { Color.Red, Color.Purple, Color.LightBlue, Color.Green, Color.Gray, Color.Yellow };

        private string[] Attribute_Names = new string[8] { "Accuracy", "Clearcast Chance", "Haste", "Power", "Resistances", "Energy", "Speed", "Set Power" };

        private string[] Spell_ParameterNames = new string[3] { "Range", "Accuracy", "Speed" };

        private string[] Spell_EffectParameterNames = new string[3] { "Cost", "Power", "Cooldown" };

        public string[] Battlefields = new string[5] {"Not assigned", "Southern battlefield", "Eastern battlefield", "Western battlefield", "Stormridge" };

        public Color[] Faction_Colors = new Color[3] { Color.Red, Color.Blue, Color.Green };
    }

    public class Slot
    {
        public uint SlotID;
        public Point Location;

        public Slot(uint slotid, Point location)
        {
            SlotID = slotid;
            Location = location;
        }
    }
}
