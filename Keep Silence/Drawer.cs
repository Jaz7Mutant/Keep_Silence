﻿using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keep_Silence
{
    public static class Drawer
    {
        private static int noiseCircleRadius;

        public static void DrawGame(PaintEventArgs e, Game game, GameState gameState, Dictionary<string,Bitmap> bitmaps, Timer timer, int tickCount)
        {
            //todo Collect Bitmaps and draw it
            e.Graphics.TranslateTransform(0, GameState.CellSize);
            e.Graphics.FillRectangle(
                Brushes.Black, 0, 0, GameState.CellSize * game.CurrentRoom.Width,
                GameState.CellSize * game.CurrentRoom.Height);
            DrawEnvironment(e, game, gameState, bitmaps);
            DrawCreatures(e, gameState, bitmaps, tickCount);

            e.Graphics.ResetTransform();

            if (game.CurrentMessage != null)
            {
                timer.Enabled = false;
                MessageBox.Show(game.CurrentMessage);
                timer.Enabled = true;
                game.CurrentMessage = null;
            }

            e.Graphics.DrawString(
                game.Player.GetHealthPoints().ToString(CultureInfo.InvariantCulture) + "\t" +
                game.Player.GetFlashlightPoints(), new Font("Vendara", 15), Brushes.DarkMagenta, 0, 0);

        }

        private static void DrawCreatures(PaintEventArgs e, GameState gameState, Dictionary<string, Bitmap> bitmaps, int tickCount)
        {
            foreach (var a in gameState.Animations)
            {
                bitmaps[a.Creature.GetHitImageFileName()].RotateFlip(a.Command.Rotate);
                bitmaps[a.Creature.GetImageFileName()].RotateFlip(a.Command.Rotate);
                var image = a.HitAnimation
                    ? bitmaps[a.Creature.GetHitImageFileName()]
                    : bitmaps[a.Creature.GetImageFileName()];
                DrawNoiseCircle(e, a, tickCount);
                if (a.Creature is Monster monster && monster.GetVisibility() < 1)
                    continue;
                e.Graphics.DrawImage(image, a.Location.X, a.Location.Y, GameState.CellSize, GameState.CellSize);
                noiseCircleRadius += a.Creature.GetNoiseLevel()*3;
            }
        }

        private static void DrawEnvironment(PaintEventArgs e, Game game, GameState gameState, Dictionary<string, Bitmap> bitmaps)
        {
            for (var x = 0; x < game.CurrentRoom.Map.GetLength(0); x++)
                for (var y = 0; y < game.CurrentRoom.Map.GetLength(1); y++)
                {
                    var environment = game.CurrentRoom.Map[x, y];
                    var img = bitmaps[environment.GetImageFileName()];
                    if (environment.Illumination < 1)
                        continue;

                    var imgPos = gameState.ConvertPointToImageSize(new Point(x, y));
                    e.Graphics.DrawImage(img, imgPos.X, imgPos.Y, GameState.CellSize, GameState.CellSize);
                }
        }

        
        private static void DrawNoiseCircle(PaintEventArgs e, CreatureAnimation a, int tickCount)
        {
            if (a.Creature.GetNoiseLevel() == 0)
                return;
            var radius = (a.Creature.GetNoiseLevel() + 1) * GameState.CellSize;
            if (noiseCircleRadius <= radius)
                radius = noiseCircleRadius;       
            else
                noiseCircleRadius = 0;

            var circleRectangle = new Rectangle(
                a.Location.X - (radius - GameState.CellSize) / 2,
                a.Location.Y - (radius - GameState.CellSize) / 2,
                radius,
                radius);
            e.Graphics.DrawEllipse(new Pen(Color.LightGray, 3),
                circleRectangle);
        }
    }
}
