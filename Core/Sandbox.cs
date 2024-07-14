using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace Experiments.Core;

public class Sandbox(Rectangle grid, int scale = 2, bool tileCollision = true)
{
    private readonly int _columns = grid.Width / scale;
    private readonly int _rows = grid.Height / scale;

    private float _colorMult = 0.5f;

    private float[,] _currentGrid = new float[grid.Width / scale, grid.Height / scale];
    private float[,] _nextGrid;

    public void Update()
    {
        _nextGrid = new float[_columns, _rows];

        FasterParallel.For(0, _columns, IterateOverGrid);

        _currentGrid = _nextGrid;

        Point16 gridIndices = ToGridIndices(Main.MouseWorld);
        if (grid.Contains(Main.MouseWorld.ToPoint()) && Main.mouseLeft && !Main.mouseLeftRelease && !CheckTiles(gridIndices.X, gridIndices.Y))
        {
            Dust.NewDustPerfect(gridIndices.ToWorldCoordinates(), 1);
            _currentGrid[gridIndices.X, gridIndices.Y] = _colorMult;
            _colorMult += 0.05f;
        }

        if (_colorMult > 1.3f)
            _colorMult = 0.5f;
    }

    private void IterateOverGrid(int start, int end, object context)
    {
        for (int i = 0; i < _columns; i++)
        for (int j = 0; j < _rows; j++)
        {
            float state = _currentGrid[i, j];

            if (state > 0)
            {
                int direction = Main.rand.NextFromCollection([1, -1]);

                if (j + 1 < _rows)
                {
                    if (_currentGrid[i, j + 1] == 0 && !CheckTiles(i, j + 1))
                    {
                        _nextGrid[i, j + 1] = state;
                        continue;
                    }

                    // Ensure i + direction and i - direction are within bounds
                    bool withinBoundsPlus = i + direction >= 0 && i + direction < _columns;
                    bool withinBoundsMinus = i - direction >= 0 && i - direction < _columns;

                    if (withinBoundsPlus && _currentGrid[i + direction, j + 1] == 0 && !CheckTiles(i + direction, j + 1))
                        _nextGrid[i + direction, j + 1] = state;
                    else if (withinBoundsMinus && _currentGrid[i - direction, j + 1] == 0 && !CheckTiles(i - direction, j + 1))
                        _nextGrid[i - direction, j + 1] = state;
                    else
                        _nextGrid[i, j] = state;
                }
                else
                    _nextGrid[i, j] = state;
            }
        }
    }

    public void Draw(Color? color = null, Color? backgroundColor = null)
    {
        Color drawColor = color ?? Color.White;
        Color bgColor = backgroundColor ?? Color.Transparent;

        Texture2D texture = Graphics.GetTexture("Pixel");

        for (int i = 0; i < _columns; i++)
        for (int j = 0; j < _rows; j++)
        {
            Main.EntitySpriteDraw(texture, ToWorldCoordinates(i, j) - Main.screenPosition, null,
                _currentGrid[i, j] > 0 ? drawColor * _currentGrid[i, j] : bgColor, 0, texture.Size() / 2, scale, SpriteEffects.None);
        }
    }

    private Vector2 ToWorldCoordinates(int i, int j) => new Vector2(i * scale + grid.X, j * scale + grid.Y) + new Vector2(scale / 2);

    private Point16 ToGridIndices(Vector2 vector2) => new Point16((int)(vector2.X - grid.X) / scale, (int)(vector2.Y - grid.Y) / scale) - new Point16(scale / 2);

    private bool CheckTiles(int i, int j) => tileCollision && Framing.GetTileSafely(ToWorldCoordinates(i, j)).HasTile;
}