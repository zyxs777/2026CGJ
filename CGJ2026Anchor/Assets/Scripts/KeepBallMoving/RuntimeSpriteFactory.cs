using UnityEngine;

namespace KeepBallMoving
{
    public static class RuntimeSpriteFactory
    {
        public static Sprite CreateSquareSprite(string name, Color color)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color[] pixels = { color, color, color, color };
            texture.SetPixels(pixels);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            sprite.name = name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        public static Sprite CreateCircleSprite(string name, Color color, int size = 128)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };

            float center = (size - 1) * 0.5f;
            float radius = center - 1f;
            Color clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    texture.SetPixel(x, y, distance <= radius ? color : clear);
                }
            }

            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        public static Sprite CreateRingSprite(string name, Color color, int size = 256, float thickness = 0.045f)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };

            float center = (size - 1) * 0.5f;
            float outerRadius = center - 1f;
            float innerRadius = outerRadius * (1f - thickness);
            Color clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    texture.SetPixel(x, y, distance <= outerRadius && distance >= innerRadius ? color : clear);
                }
            }

            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
