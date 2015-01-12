using System;
using System.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Person : Unit
    {
        public uint ID;

        public Person(string Data)
        {
            string[] Arguments = Data.Split('\t');

            ID = Convert.ToUInt32(Arguments[0]);
            Name = Arguments[1];
            Faction = Convert.ToUInt32(Arguments[2]);
            Icon = TextureLoader.FromFile(Program.GameForm.Device, @"data\icons\person_" + Arguments[3] + ".png");
            ItemLevel = (uint)(Convert.ToUInt32(Arguments[4]) * 0.6);

            Energy = Convert.ToDouble(Arguments[5]);
            MaxEnergy = Convert.ToDouble(Arguments[6]);

            Location = new PointF(Convert.ToSingle(Arguments[7]), Convert.ToSingle(Arguments[8]));

            Rotation = Convert.ToInt32(Arguments[9]);
            Speed = Convert.ToDouble(Arguments[10]);
        }

        public void Dispose()
        {
            Icon.Dispose();
        }

        public static string[] Dialogs = new string[6]
        {
            "Hello, i'm Person#0, alis Leader0 or something.",
            "You can choose from 2 quest, quite easy!\n1) Kill that shit\n2) Collect that shit",
            "So you are that killer type! Okey i'm waiting for you'r return! Bring the head of the red dragon!",
            "You have to get me 2 sacks of shit. I forgot where i put my piles of shit, so you have to find them!",
            "I'm waiting for that dragon head btch!",
            "I'm waiting for thoose sacks of shit!"
        };

        public static string[] Answers = new string[7]
        {
            "#Hi bitch",
            "Great, i don't care, what can you do for me?",
            "I'll collect the head!",
            "I'll collect the sacks of shit!",
            "#answ0",
            "#answ1",
            "#answ2"
        };
    }
}
