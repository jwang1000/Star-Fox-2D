using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StarFox2D.Classes
{
    /// <summary>
    /// Draws text on the screen with proper word wrapping. If text does not fit, it will overflow from the bottom first.
    /// </summary>
    public class TextBox
    {
        public static SpriteFont FontSmall;

        public static SpriteFont FontMedium;

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

        /// <summary>
        /// The coordinates of the top left of the text.
        /// </summary>
        private Vector2 TextPosition;

        /// <summary>
        /// Align text to the top left while still keeping word wrap if true. Only checked if Dimensions are given.
        /// </summary>
        private bool LeftAlignText;

        
        public TextBox(string text, Vector2 position, Vector2 dimensions, FontSize size, Vector2 padding, Color colour, float rotation, bool leftAlignText = false)
        {
            Text = text;
            Position = position;
            Dimensions = dimensions;
            Padding = padding;
            Colour = colour;
            Rotation = rotation;
            LeftAlignText = leftAlignText;

            switch (size)
            {
                case FontSize.Small:
                    Font = FontSmall;
                    break;
                case FontSize.Medium:
                    Font = FontMedium;
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
            else
            {
                TextPosition = Position;
            }
        }

        public TextBox(string text, Vector2 position, Vector2 dimensions, FontSize size = FontSize.Regular, bool leftAlignText = false) : this(text, position, dimensions, size, Vector2.Zero, Color.White, 0, leftAlignText) { }

        /// <summary>
        /// When no dimensions are given, no word wrapping will be performed.
        /// </summary>
        public TextBox(string text, Vector2 position, FontSize size = FontSize.Regular, bool leftAlignText = false) : this(text, position, Vector2.Zero, size, Vector2.Zero, Color.White, 0, leftAlignText) { }

        /// <summary>
        /// Modifies the Text field to add newline characters as necessary to fit the bounds. Only called if dimensions are given.
        /// </summary>
        private void WrapText()
        {
            string[] words = Text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = Font.MeasureString(" ").X;

            for (int i = 0; i < words.Length; ++i)
            {
                string word = words[i];
                Vector2 size = Font.MeasureString(word);

                if (lineWidth + size.X >= Dimensions.X)
                {
                    sb.Append("\n");
                    lineWidth = 0;
                }
                sb.Append(word);
                lineWidth += size.X;

                if (i < words.Length - 1)
                {
                    sb.Append(" ");
                    lineWidth += spaceWidth;
                }
            }

            Text = sb.ToString();
            Vector2 textDimensions = Font.MeasureString(Text);

            // set position for text to be centered in the given dimensions
            if (LeftAlignText)
            {
                TextPosition = Position;
            }
            else
            {
                TextPosition.X = Position.X + (Dimensions.X - textDimensions.X) / 2;
                TextPosition.Y = Position.Y + (Dimensions.Y - textDimensions.Y) / 2;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Font, Text, TextPosition, Colour, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
    }


    public enum FontSize
    {
        Small,
        Medium,
        Regular,
        Large,
        Title
    }
}
