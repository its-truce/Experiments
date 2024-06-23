using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace Experiments.Core;

public class GrainSimulation(Rectangle grid, int scale = 2, bool tileCollision = true)
{
    private readonly int _columns = grid.Width / scale;
    private readonly int _rows = grid.Height / scale;

    private float _colorMult = 0.5f;

    private float[,] _currentGrid = new float[grid.Width / scale, grid.Height / scale];
    private float[,] _nextGrid;

    public void Update()
    {
        _nextGrid = new float[_columns, _rows];

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

        _currentGrid = _nextGrid;

        Point gridIndices = ToGridIndices(Main.MouseWorld) + new Point(scale, scale);
        if (grid.Contains(Main.MouseWorld.ToPoint()) && Main.mouseLeft && !Main.mouseLeftRelease && !CheckTiles(gridIndices.X, gridIndices.Y))
        {
            _currentGrid[gridIndices.X, gridIndices.Y] = _colorMult;
            _colorMult += 0.05f;
        }

        if (_colorMult > 1.3f)
            _colorMult = 0.5f;
    }

    public void Draw(Color? color = null, Color? backgroundColor = null)
    {
        Color drawColor = color ?? Color.White;
        Color bgColor = backgroundColor ?? Color.Transparent;

        Texture2D texture = TextureAssets.MagicPixel.Value;

        for (int i = 0; i < _columns; i++)
        for (int j = 0; j < _rows; j++)
        {
            Main.EntitySpriteDraw(texture, ToGridCoordinates(new Point(i, j)).ToVector2() - Main.screenPosition, new Rectangle(0, 0, 1, 1),
                _currentGrid[i, j] > 0 ? drawColor * _currentGrid[i, j] : bgColor, 0, new Vector2(scale / 2), scale, SpriteEffects.None);
        }
    }

    private Point ToGridCoordinates(Point point) => new(point.X * scale + grid.X, point.Y * scale + grid.Y);

    private Point ToGridIndices(Vector2 vector2) => new((vector2.ToPoint().X - grid.X) / scale, (vector2.ToPoint().Y - grid.Y) / scale);

    private bool CheckTiles(int i, int j) =>
        tileCollision && Framing.GetTileSafely(ToGridCoordinates(new Point(i, j)).ToVector2() - new Vector2(scale * 2)).HasTile;
}