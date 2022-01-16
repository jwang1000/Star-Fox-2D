using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarFox2D.Classes
{
    /// <summary>
    /// Draws text on the screen with proper word wrapping. If text does not fit, it will overflow from the bottom first.
    /// </summary>
    public class TextBox
    {
        public static SpriteFont FontSmall;

        public static SpriteFont FontRegular;

        public static SpriteFont FontLarge;

        public static SpriteFont FontTitle;


        public string Text { get; private set; }

        /// <summary>
        /// The coordinates of the TOP LEFT of the text box.
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// The width and height of the text box, including padding.
        /// Setting this to Vector2.Zero will deactivate word wrap.
        /// </summary>
        public Vector2 Dimensions { get; private set; }

        public Vector2 Padding { get; private set; }

        public Color Colour { get; private set; }

        public float Rotation { get; private set; }

        /// <summary>
        /// The appropriate font size, based on the FontSize passed to the constructor.
        /// </summary>
        private SpriteFont Font;

        
        public TextBox(string text, Vector2 position, Vector2 dimensions, FontSize size, Vector2 padding, Color colour, float rotation)
        {
            Text = text;
            Position = position;
            Dimensions = dimensions;
            Padding = padding;
            Colour = colour;
            Rotation = rotation;

            switch (size)
            {
                case FontSize.Small:
                    Font = FontSmall;
                    break;
                case FontSize.Regular:
                    Font = FontRegular;
                    break;
                case FontSize.Large:
                    Font = FontLarge;
                    break;
                case FontSize.Title:
                    Font = FontTitle;
                    break;
            }

            if (dimensions != Vector2.Zero)
            {
                WrapText();
            }
        }

        public TextBox(string text, Vector2 position, Vector2 dimensions, FontSize size = FontSize.Regular) : this(text, position, dimensions, size, Vector2.Zero, Color.White, 0) { }

        /// <summary>
        /// When no dimensions are given, no word wrapping will be performed.
        /// </summary>
        public TextBox(string text, Vector2 position, FontSize size = FontSize.Regular) : this(text, position, Vector2.Zero, size, Vector2.Zero, Color.White, 0) { }

        /// <summary>
        /// Modifies the Text field to add newline characters as necessary to fit the bounds.
        /// </summary>
        private void WrapText()
        {
            string[] words = Text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = Font.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = Font.MeasureString(word);

                if (lineWidth + size.X < Dimensions.X)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            Text = sb.ToString();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Font, Text, Position, Colour, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
    }


    public enum FontSize
    {
        Small,
        Regular,
        Large,
        Title
    }
}
