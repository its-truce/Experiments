using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace Experiments.Core;

public class Sandbox(Rectangle grid, int scale = 2, bool tileCollision = true)
{
    private readonly int _columns = grid.Width / scale;
    private readonly int _rows = grid.Height / scale;

    private float _value = 0.5f;

    private float[,] _currentGrid = new float[grid.Width / scale, grid.Height / scale];
    private float[,] _nextGrid;

    public void Update()
    {
        _nextGrid = new float[_columns, _rows];

        for (int i = grid.Left; i < grid.Right; i += scale)
        for (int j = grid.Top; j < grid.Bottom; j += scale)
        {
            int iIndex = i - grid.Left;
            int jIndex = j - grid.Top;

            float state = _currentGrid[iIndex, jIndex];

            if (state > 0 && j + scale < grid.Bottom)
            {
                int direction = Main.rand.NextFromCollection([1, -1]);

                if (_currentGrid[iIndex, jIndex + 1] == 0 && !CheckTiles(i, j + 1))
                {
                    _nextGrid[iIndex, jIndex + 1] = state;
                    continue;
                }

                // Ensure i + direction and i - direction are within bounds
                bool withinBoundsPlus = i + direction >= 0 && i + direction < grid.Right;
                bool withinBoundsMinus = i - direction >= 0 && i - direction < grid.Right;

                if (withinBoundsPlus && _currentGrid[iIndex + direction, jIndex + 1] == 0 && !CheckTiles(i + direction, j + scale))
                    _nextGrid[iIndex + direction, jIndex + 1] = state;
                else if (withinBoundsMinus && _currentGrid[iIndex - direction, jIndex + 1] == 0 && !CheckTiles(i - direction, j + scale))
                    _nextGrid[iIndex - direction, jIndex + 1] = state;
                else
                    _nextGrid[iIndex, jIndex] = state;
            }
            else
                _nextGrid[iIndex, jIndex] = state;
        }

        _currentGrid = _nextGrid;

        Vector2 gridIndices = Main.MouseWorld - new Vector2(grid.Left, grid.Top) + new Vector2(scale / 2);
        if (grid.Contains(Main.MouseWorld.ToPoint()) && Main.mouseLeft && !Main.mouseLeftRelease && !CheckTiles(Main.MouseWorld.X, Main.MouseWorld.Y))
        {
            _currentGrid[(int)gridIndices.X, (int)gridIndices.Y] = _value;
            _value += 0.05f;
        }

        if (_value > 1.3f)
            _value = 0.5f;
    }

    public void Draw(Color? color = null, Color? backgroundColor = null)
    {
        Color drawColor = color ?? Color.White;
        Color bgColor = backgroundColor ?? Color.Transparent;

        Texture2D texture = TextureAssets.MagicPixel.Value;

        for (int i = grid.Left; i < grid.Right; i += scale)
        for (int j = grid.Top; j < grid.Bottom; j += scale)
        {
            int iIndex = i - grid.Left;
            int jIndex = j - grid.Top;

            Main.EntitySpriteDraw(texture, new Vector2(i, j) + new Vector2(scale / 2) - Main.screenPosition, new Rectangle(0, 0, 1, 1),
                _currentGrid[iIndex, jIndex] > 0 ? drawColor * _currentGrid[i, j] : bgColor, 0, new Vector2(scale / 2), scale, SpriteEffects.None);
        }
    }

    private bool CheckTiles(float i, float j) =>
        tileCollision && Framing.GetTileSafely(new Vector2(i, j)).HasTile;
}