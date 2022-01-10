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

        private bool HasTexture;

        public Color Colour { get; private set; }

        public Color HoverColour { get; private set; }

        /// <summary>
        /// The action to perform when this button is clicked. Does not take any parameters.
        /// </summary>
        public delegate void ClickAction();

        /// <summary>
        /// The action to perform when this button is clicked. Takes a float as input, useful for setting values.
        /// </summary>
        public delegate void ClickActionFloat(float input);


        public Button(Vector2 position, int width, int height, Color colour, Color hoverColour, Texture2D texture = null, Texture2D hoverTexture = null)
        {
            Position = position;
            Width = width;
            Height = height;
            Colour = colour;
            HoverColour = hoverColour;
            
            if (texture != null)
            {
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
                HasTexture = true;
            }
            else
            {
                // TODO blank button doesn't centre properly, replace with sprite
                // can remove HasTexture field afterwards
                Texture = Textures.Button;
                TextureOriginPosition = Vector2.Zero;
                HoverTexture = Texture;
                HoverTextureOriginPosition = TextureOriginPosition;
            }
        }

        public Button(Vector2 position, int width, int height, Texture2D texture = null, Texture2D hoverTexture = null) 
            : this(position, width, height, Color.White, Color.AliceBlue, texture, hoverTexture) { }

        public bool MouseHoversButton(Vector2 mousePosition)
        {
            return mousePosition.X >= Position.X - Width / 2 && mousePosition.X <= Position.X + Width / 2
                && mousePosition.Y >= Position.Y - Height / 2 && mousePosition.Y <= Position.Y + Height / 2;
        }

        public void Clicked(ClickAction action)
        {
            action();
        }

        public void Clicked(ClickActionFloat action, float input)
        {
            action(input);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 mousePosition)
        {
            if (MouseHoversButton(mousePosition))
            {
                if (HasTexture)
                {
                    spriteBatch.Draw(HoverTexture, Position, null, HoverColour, 0, HoverTextureOriginPosition, new Vector2((float)Width / HoverTexture.Width, (float)Height / HoverTexture.Height), SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(HoverTexture, new Vector2(Position.X - Width/2, Position.Y - Height/2), null, HoverColour, 0, HoverTextureOriginPosition, new Vector2(Width, Height), SpriteEffects.None, 0f);
                }
            }
            else
            {
                if (HasTexture)
                {
                    spriteBatch.Draw(Texture, Position, null, Colour, 0, TextureOriginPosition, new Vector2((float)Width / Texture.Width, (float)Height / Texture.Height), SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(Texture, new Vector2(Position.X - Width / 2, Position.Y - Height / 2), null, Colour, 0, TextureOriginPosition, new Vector2(Width, Height), SpriteEffects.None, 0f);
                }
            }
        }
    }
}
