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

        public static Sprite CreateTalentIconSprite(string name, Color accentColor, int style, int size = 96)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            Color softColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.38f);
            Color brightColor = new Color(1f, 1f, 1f, 0.9f);
            float scale = size / 96f;

            void DrawRect(float x, float y, float width, float height, Color color)
            {
                int minX = Mathf.Clamp(Mathf.RoundToInt(x * scale), 0, size - 1);
                int maxX = Mathf.Clamp(Mathf.RoundToInt((x + width) * scale), 0, size - 1);
                int minY = Mathf.Clamp(Mathf.RoundToInt(y * scale), 0, size - 1);
                int maxY = Mathf.Clamp(Mathf.RoundToInt((y + height) * scale), 0, size - 1);

                for (int py = minY; py <= maxY; py++)
                {
                    for (int px = minX; px <= maxX; px++)
                    {
                        texture.SetPixel(px, py, color);
                    }
                }
            }

            void DrawCircle(float centerX, float centerY, float radius, Color color)
            {
                float scaledCenterX = centerX * scale;
                float scaledCenterY = centerY * scale;
                float scaledRadius = radius * scale;
                float radiusSqr = scaledRadius * scaledRadius;
                int minX = Mathf.Clamp(Mathf.FloorToInt(scaledCenterX - scaledRadius), 0, size - 1);
                int maxX = Mathf.Clamp(Mathf.CeilToInt(scaledCenterX + scaledRadius), 0, size - 1);
                int minY = Mathf.Clamp(Mathf.FloorToInt(scaledCenterY - scaledRadius), 0, size - 1);
                int maxY = Mathf.Clamp(Mathf.CeilToInt(scaledCenterY + scaledRadius), 0, size - 1);

                for (int py = minY; py <= maxY; py++)
                {
                    for (int px = minX; px <= maxX; px++)
                    {
                        Vector2 offset = new Vector2(px - scaledCenterX, py - scaledCenterY);
                        if (offset.sqrMagnitude <= radiusSqr)
                        {
                            texture.SetPixel(px, py, color);
                        }
                    }
                }
            }

            void DrawRing(float centerX, float centerY, float outerRadius, float innerRadius, Color color)
            {
                float scaledCenterX = centerX * scale;
                float scaledCenterY = centerY * scale;
                float scaledOuterRadius = outerRadius * scale;
                float scaledInnerRadius = innerRadius * scale;
                float outerSqr = scaledOuterRadius * scaledOuterRadius;
                float innerSqr = scaledInnerRadius * scaledInnerRadius;
                int minX = Mathf.Clamp(Mathf.FloorToInt(scaledCenterX - scaledOuterRadius), 0, size - 1);
                int maxX = Mathf.Clamp(Mathf.CeilToInt(scaledCenterX + scaledOuterRadius), 0, size - 1);
                int minY = Mathf.Clamp(Mathf.FloorToInt(scaledCenterY - scaledOuterRadius), 0, size - 1);
                int maxY = Mathf.Clamp(Mathf.CeilToInt(scaledCenterY + scaledOuterRadius), 0, size - 1);

                for (int py = minY; py <= maxY; py++)
                {
                    for (int px = minX; px <= maxX; px++)
                    {
                        Vector2 offset = new Vector2(px - scaledCenterX, py - scaledCenterY);
                        float sqrMagnitude = offset.sqrMagnitude;
                        if (sqrMagnitude <= outerSqr && sqrMagnitude >= innerSqr)
                        {
                            texture.SetPixel(px, py, color);
                        }
                    }
                }
            }

            switch (style)
            {
                case 0:
                    DrawCircle(48f, 48f, 17f, accentColor);
                    DrawCircle(25f, 48f, 10f, softColor);
                    DrawCircle(71f, 48f, 10f, softColor);
                    DrawRect(28f, 22f, 40f, 5f, brightColor);
                    break;
                case 1:
                    DrawCircle(55f, 56f, 20f, accentColor);
                    DrawRect(24f, 35f, 42f, 26f, accentColor);
                    DrawRect(59f, 30f, 22f, 14f, accentColor);
                    DrawRect(24f, 66f, 48f, 5f, brightColor);
                    break;
                case 2:
                    for (int i = 0; i < 7; i++)
                    {
                        DrawCircle(24f + i * 8f, 36f + Mathf.Sin(i * 0.75f) * 14f, 5f, accentColor);
                    }
                    DrawRect(21f, 24f, 4f, 18f, brightColor);
                    DrawRect(71f, 55f, 4f, 18f, brightColor);
                    break;
                case 3:
                    DrawCircle(42f, 48f, 17f, accentColor);
                    DrawRect(64f, 44f, 18f, 8f, brightColor);
                    DrawRect(69f, 39f, 8f, 18f, brightColor);
                    DrawRect(22f, 68f, 52f, 5f, softColor);
                    break;
                case 4:
                    DrawCircle(42f, 48f, 17f, accentColor);
                    DrawRect(64f, 44f, 18f, 8f, brightColor);
                    DrawRect(69f, 39f, 8f, 18f, brightColor);
                    DrawRect(22f, 47f, 52f, 5f, softColor);
                    break;
                case 5:
                    DrawCircle(42f, 48f, 17f, accentColor);
                    DrawRect(64f, 44f, 18f, 8f, brightColor);
                    DrawRect(69f, 39f, 8f, 18f, brightColor);
                    DrawRect(22f, 24f, 52f, 5f, softColor);
                    break;
                case 6:
                    DrawRing(48f, 48f, 33f, 25f, softColor);
                    DrawRing(48f, 48f, 22f, 15f, accentColor);
                    DrawCircle(48f, 48f, 9f, brightColor);
                    break;
                case 7:
                    DrawRing(48f, 48f, 35f, 29f, softColor);
                    DrawCircle(31f, 48f, 11f, accentColor);
                    DrawRect(40f, 44f, 26f, 8f, accentColor);
                    DrawRect(62f, 38f, 8f, 20f, brightColor);
                    break;
                case 8:
                    DrawRing(48f, 48f, 35f, 29f, softColor);
                    DrawCircle(48f, 48f, 15f, accentColor);
                    DrawCircle(68f, 48f, 7f, brightColor);
                    DrawRect(26f, 46f, 16f, 4f, accentColor);
                    break;
                default:
                    DrawCircle(48f, 48f, 20f, accentColor);
                    break;
            }

            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
