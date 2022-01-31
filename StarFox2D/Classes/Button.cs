using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    public class Button
    {
        /// <summary>
        /// The location of the centre of the button.
        /// </summary>
        public Vector2 Position { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Texture2D Texture { get; private set; }

        public Vector2 TextureOriginPosition { get; private set; }

        public Texture2D HoverTexture { get; private set; }

        public Vector2 HoverTextureOriginPosition { get; private set; }

        public Color Colour { get; set; }

        public Color HoverColour { get; set; }

        public static Color InactiveColour = Color.Gray;

        public bool IsActive { get; set; }

        /// <summary>
        /// The text for the button (if it has any text).
        /// </summary>
        public TextBox Text { get; private set; }

        /// <summary>
        /// The action to perform when this button is clicked. Does not take any parameters.
        /// </summary>
        public delegate void ClickAction();

        private ClickAction clickAction;

        /// <summary>
        /// The action to perform when this button is clicked. Takes a float as input, useful for setting values.
        /// </summary>
        public delegate void ClickActionFloat(float input);

        private ClickActionFloat clickActionFloat;


        private Button(Vector2 position, int width, int height, Color colour, Color hoverColour, Texture2D texture, Texture2D hoverTexture = null, string text = "")
        {
            Position = position;
            Width = width;
            Height = height;
            Colour = colour;
            HoverColour = hoverColour;
            Text = new TextBox(text, new Vector2(Position.X - Width/2, Position.Y - Height/2), new Vector2(Width, Height), FontSize.Regular, new Vector2(5), Color.White, 0);
            IsActive = true;
            
            Texture = texture;
            TextureOriginPosition = new Vector2(Texture.Width / 2, Texture.Height / 2);

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

        public Button(Vector2 position, int width, int height, Color colour, Color hoverColour, ClickAction action, Texture2D texture, Texture2D hoverTexture = null, string text = "")
            : this(position, width, height, colour, hoverColour, texture, hoverTexture, text)
        {
            clickAction = action;
        }

        public Button(Vector2 position, int width, int height, Color colour, Color hoverColour, ClickActionFloat action, Texture2D texture, Texture2D hoverTexture = null, string text = "")
            : this(position, width, height, colour, hoverColour, texture, hoverTexture, text)
        {
            clickActionFloat = action;
        }

        public Button(Vector2 position, int width, int height, ClickAction action, Texture2D texture, Texture2D hoverTexture = null, string text = "") 
            : this(position, width, height, Color.Blue, Color.CornflowerBlue, texture, hoverTexture, text)
        {
            clickAction = action;
        }

        public Button(Vector2 position, int width, int height, ClickActionFloat action, Texture2D texture, Texture2D hoverTexture = null, string text = "")
            : this(position, width, height, Color.Blue, Color.CornflowerBlue, texture, hoverTexture, text)
        {
            clickActionFloat = action;
        }

        public bool MouseHoversButton(Vector2 mousePosition)
        {
            return mousePosition.X >= Position.X - Width / 2 && mousePosition.X <= Position.X + Width / 2
                && mousePosition.Y >= Position.Y - Height / 2 && mousePosition.Y <= Position.Y + Height / 2;
        }

        public void Clicked()
        {
            if (IsActive)
                clickAction();
        }

        public void Clicked(float input)
        {
            if (IsActive)
                clickActionFloat(input);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 mousePosition)
        {
            if (!IsActive)
            {
                spriteBatch.Draw(Texture, Position, null, InactiveColour, 0, TextureOriginPosition, new Vector2((float)Width / Texture.Width, (float)Height / Texture.Height), SpriteEffects.None, 0f);
            }
            else if (MouseHoversButton(mousePosition))
            {
                spriteBatch.Draw(HoverTexture, Position, null, HoverColour, 0, HoverTextureOriginPosition, new Vector2((float)Width / HoverTexture.Width, (float)Height / HoverTexture.Height), SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(Texture, Position, null, Colour, 0, TextureOriginPosition, new Vector2((float)Width / Texture.Width, (float)Height / Texture.Height), SpriteEffects.None, 0f);
            }
            Text.Draw(spriteBatch);
        }
    }
}
