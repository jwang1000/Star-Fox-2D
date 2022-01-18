using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public class Slider
    {
        /// <summary>
        /// The location of the centre of the slider.
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// The total width of the slider from end to end.
        /// </summary>
        public int Width { get; private set; }

        public int Height { get; private set; }

        public int ButtonRadius { get; private set; }

        public Texture2D Texture { get; private set; }

        public Vector2 TextureOriginPosition { get; private set; }

        public Texture2D HoverTexture { get; private set; }

        public Vector2 HoverTextureOriginPosition { get; private set; }

        public Texture2D BackgroundTexture { get; private set; }

        public Vector2 BackgroundTextureOriginPosition { get; private set; }

        public Color Colour { get; private set; }

        public Color HoverColour { get; private set; }

        public Color BackgroundColour { get; private set; }

        private Button.ClickActionFloat clickActionFloat;

        /// <summary>
        /// The current slider being adjusted. Enables dragging the slider wildly while the mouse is held down.
        /// Set to null in MainGame.MouseLeftReleased().
        /// </summary>
        public static Slider ActiveSlider = null;

        /// <summary>
        /// The value of the slider from 0 to 1.
        /// </summary>
        public float Value { get; private set; }


        private Slider(Vector2 position, int totalWidth, int totalHeight, int buttonRadius, Color colour, Color hoverColour, Color backgroundColor, Button.ClickActionFloat action, Texture2D texture, Texture2D backgroundTexture, Texture2D hoverTexture = null, float startValue = 1)
        {
            Position = position;
            Width = totalWidth;
            Height = totalHeight;
            ButtonRadius = buttonRadius;
            Colour = colour;
            HoverColour = hoverColour;
            BackgroundColour = backgroundColor;
            clickActionFloat = action;

            Texture = texture;
            BackgroundTexture = backgroundTexture;
            TextureOriginPosition = new Vector2(Texture.Width / 2, Texture.Height / 2);
            BackgroundTextureOriginPosition = new Vector2(BackgroundTexture.Width / 2, BackgroundTexture.Height / 2);
            Value = startValue;

            if (hoverTexture != null)
            {
                HoverTexture = hoverTexture;
                HoverTextureOriginPosition = new Vector2(HoverTexture.Width / 2, HoverTexture.Height / 2);
            }
            else
            {
                HoverTexture = Texture;
                HoverTextureOriginPosition = TextureOriginPosition;
            }
        }

        public Slider(Vector2 position, int totalWidth, int totalHeight, int buttonRadius, Button.ClickActionFloat action, Texture2D buttonTexture, Texture2D backgroundTexture, Texture2D hoverTexture = null, float startValue = 1)
            : this(position, totalWidth, totalHeight, buttonRadius, Color.Blue, Color.CornflowerBlue, Color.DarkBlue, action, buttonTexture, backgroundTexture, hoverTexture, startValue) { }

        public bool MouseHoversButton(Vector2 mousePosition)
        {
            return mousePosition.X >= GetButtonPosition().X - ButtonRadius && mousePosition.X <= GetButtonPosition().X + ButtonRadius
                && mousePosition.Y >= Position.Y - ButtonRadius && mousePosition.Y <= Position.Y + ButtonRadius;
        }

        /// <summary>
        /// Sets the value of the slider according to the mouse position, then calls the given function with the slider's value.
        /// </summary>
        /// <param name="mousePosition"></param>
        public void ChangePosition(Vector2 mousePosition)
        {
            if (mousePosition.X <= Position.X - Width / 2)
                Value = 0;
            else if (mousePosition.X >= Position.X + Width / 2)
                Value = 1;
            else
                Value = (mousePosition.X - (Position.X - Width / 2)) / Width;
            clickActionFloat(Value);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 mousePosition)
        {
            spriteBatch.Draw(BackgroundTexture, Position, null, BackgroundColour, 0, BackgroundTextureOriginPosition, new Vector2((float)Width / BackgroundTexture.Width, (float)Height / BackgroundTexture.Height), SpriteEffects.None, 0f);
            if (MouseHoversButton(mousePosition) || ActiveSlider == this)
            {
                spriteBatch.Draw(HoverTexture, GetButtonPosition(), null, HoverColour, 0, HoverTextureOriginPosition, new Vector2((float)ButtonRadius * 2 / HoverTexture.Width, (float)ButtonRadius * 2 / HoverTexture.Height), SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(Texture, GetButtonPosition(), null, Colour, 0, TextureOriginPosition, new Vector2((float)ButtonRadius * 2 / Texture.Width, (float)ButtonRadius * 2 / Texture.Height), SpriteEffects.None, 0f);
            }
        }

        private Vector2 GetButtonPosition()
        {
            return new Vector2(Position.X - (Width / 2) + (Width * Value), Position.Y);
        }
    }
}
