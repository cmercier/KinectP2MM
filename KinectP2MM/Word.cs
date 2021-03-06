﻿using System.Windows.Controls;
using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KinectP2MM
{
    public enum WordType
    {
        FULL = 0,
        BOTTOM = 1,
        TOP = 2
    }

    public class Word : GUIObject
    {
        public static int MIN_FONTSIZE = 20, MAX_FONTSIZE = 200;
        public static int FONTSIZE = 50, MARGIN_BOTTOM = Properties.Settings.Default.decalage_polices;
        public static double ZOOM_FACTOR = 1.05, UNZOOM_FACTOR = 0.95;

        public double beginRotation { get; set; }
        public Label wordTop { get; set; }
        public Label wordBottom { get; set; }
        public WordType typeWord { get; set; }
        public double fontSize { get; set; }
        public double marginBas { get; set; }
        public String fontFamily { get; set; }
        public Guid id { get; set; }
        public String colorString { get; set; } 

        public Word()
        {
            this.beginRotation = 0;
            this._hover = false;
            this.typeWord = WordType.FULL;
            this.id = Guid.NewGuid();
            Loaded +=WordLoaded;
        }

        public Word(String content, int x = 0, int y = 0, String color = "", WordType type = WordType.FULL)
        {
            this.beginRotation = 0;
            this._hover = false;
            this.id = Guid.NewGuid();

            this.typeWord = type;
            this.x = x;
            this.y = y;
            this.fontSize = FONTSIZE;
            this.marginBas = MARGIN_BOTTOM;
            Point center = new Point(0.5, 0.5);
            this.RenderTransformOrigin = center;

            this.fontFamily = App.FONT_FAMILY;
            FontFamily FontHaut = new FontFamily(App.FONT_FAMILY_TOP);
            FontFamily FontBas = new FontFamily(App.FONT_FAMILY_BOTTOM);
            Thickness MarginHaut = new Thickness(0, 0, 0, 0);
            Thickness MarginBas = new Thickness(0, MARGIN_BOTTOM, 0, 0);

            this.wordBottom = new Label();
            this.Children.Add(this.wordBottom);
            this.wordBottom.Content = content;
            this.wordBottom.Margin = MarginBas;
            this.wordBottom.FontFamily = FontBas;
            this.wordBottom.FontSize = FONTSIZE;
            if (type == WordType.TOP) this.wordBottom.Opacity = 0;

            this.wordTop = new Label();
            this.Children.Add(this.wordTop);
            this.wordTop.Content = content;
            this.wordTop.Margin = MarginHaut;
            this.wordTop.FontFamily = FontHaut;
            this.wordTop.FontSize = FONTSIZE;
            if (type == WordType.BOTTOM) this.wordTop.Opacity = 0;

            setColor(color);

        }

        public Word(String content, String fontFamily, double fontSize, int x = 0, int y = 0, WordType type = WordType.FULL, String color = "")
        {
            this.beginRotation = 0;
            this._hover = false;
            this.id = Guid.NewGuid();

            this.typeWord = type;
            this.x = x;
            this.y = y;
            this.fontSize = FONTSIZE;
            this.marginBas = MARGIN_BOTTOM;
            Point center = new Point(0.5,0.5);
            this.RenderTransformOrigin = center;

            this.fontFamily = fontFamily;
            String saveFontFamily = App.FONT_FAMILY;
            App.loadFont(fontFamily);
            FontFamily FontHaut = new FontFamily(App.FONT_FAMILY_TOP);
            FontFamily FontBas = new FontFamily(App.FONT_FAMILY_BOTTOM);
            Thickness MarginHaut = new Thickness(0, 0, 0, 0);
            Thickness MarginBas = new Thickness(0, MARGIN_BOTTOM, 0, 0);
            App.loadFont(saveFontFamily);

            this.wordBottom = new Label();
            this.Children.Add(this.wordBottom);
            this.wordBottom.Content = content;
            this.wordBottom.Margin = MarginBas;
            this.wordBottom.FontFamily = FontBas;
            this.wordBottom.FontSize = FONTSIZE;
            if (type == WordType.TOP) this.wordBottom.Opacity = 0;

            this.wordTop = new Label();
            this.Children.Add(this.wordTop);
            this.wordTop.Content = content;
            this.wordTop.Margin = MarginHaut;
            this.wordTop.FontFamily = FontHaut;
            this.wordTop.FontSize = FONTSIZE;
            if (type == WordType.BOTTOM) this.wordTop.Opacity = 0;

            setColor(color);

            this.applyZoom(fontSize / FONTSIZE);

        }

        public Word(Word source): base(source)
        {
            this.beginRotation = source.beginRotation;
            this._hover = source._hover;
            this.id = Guid.NewGuid();
            this.fontSize = source.fontSize;
            this.marginBas = source.marginBas;
            this.RenderTransform = source.RenderTransform;

            this.wordBottom = new Label();
            this.wordBottom.Margin = source.wordBottom.Margin;
            this.wordBottom.Height = source.wordBottom.Height;
            this.wordBottom.Width = source.wordBottom.Width;
            this.wordBottom.FontFamily = source.wordBottom.FontFamily;
            this.wordBottom.FontSize = source.wordBottom.FontSize;
            this.wordBottom.Content = source.wordBottom.Content;

            this.wordTop = new Label();
            this.wordTop.Margin = source.wordTop.Margin;
            this.wordTop.Height = source.wordTop.Height;
            this.wordTop.Width = source.wordTop.Width;
            this.wordTop.FontFamily = source.wordTop.FontFamily;
            this.wordTop.FontSize = source.wordTop.FontSize;
            this.wordTop.Content = source.wordTop.Content;

            this.colorString = source.colorString;
            this.setColor(colorString);


            this.fontFamily = source.fontFamily;
            
            this.Children.Add(this.wordTop);
            this.Children.Add(this.wordBottom);           

        }
                
        private void WordLoaded(object sender, RoutedEventArgs e)
        {
            this.wordTop = (Label)this.Children[0];
            this.wordBottom = (Label)this.Children[1];
        }

        public void setColor(String color)
        {
            if (color.Equals("")) color = Properties.Settings.Default.foreground_color;
            if (color != null)
            {
                var bc = new BrushConverter();
                this.wordTop.Foreground = (Brush)bc.ConvertFrom(color);
                this.wordBottom.Foreground = (Brush)bc.ConvertFrom(color);
                this.colorString = color;
            }
        }
               
        private bool _hover { get; set; }
        public bool hover
        {
            get { return _hover; }
            set
            {
                _hover = value;
                if (_hover)
                    this.Opacity = 0.6;
                else
                    this.Opacity = 1;
            }
        }

        public override double x
        {
            get
            {
                if (_x == 0)
                    _x = Canvas.GetLeft(this);
                return _x;
            }
            set
            {
                _x = value;
                Canvas.SetLeft(this, value);
            }
        }

        public override double y
        {
            get
            {
                if (_y == 0)
                    _y = Canvas.GetTop(this);
                return _y;
            }
            set
            {
                _y = value;
                Canvas.SetTop(this, value);
            }
        }

        public Word Duplicate() //Return a copy of the word with blank bottom image. Delete top bottom image from current word.
        {
            Word top = new Word(this);
            this.wordTop.Opacity = 0;
            this.typeWord = WordType.BOTTOM;
            
            top.wordBottom.Opacity = 0;
            top.typeWord = WordType.TOP;
            return top;
        }

        public void Fusion(Word secondWord)
        {
            if(this.typeWord == WordType.TOP)
            {                
                this.wordBottom.Opacity = 1;
                this.wordBottom.Content = secondWord.wordBottom.Content;
                this.typeWord = WordType.FULL;
                this.id = Guid.NewGuid();
            }
            else
                if(this.typeWord == WordType.BOTTOM)
                {                
                    this.wordTop.Opacity = 1;
                    this.wordTop.Content = secondWord.wordTop.Content;
                    this.typeWord = WordType.FULL;
                    this.id = Guid.NewGuid();
                }          
        }

        public String getContent()
        {
            if (this.typeWord == WordType.BOTTOM)
                return this.wordBottom.Content.ToString();

            return this.wordTop.Content.ToString();
        }

        public void applyZoom(double zoom)
        {
            if (this.fontSize * zoom > MAX_FONTSIZE || this.fontSize * zoom < MIN_FONTSIZE)
                return;
            this.fontSize = this.fontSize *  zoom;
            this.marginBas = this.marginBas * zoom;

            this.wordTop.FontSize = this.fontSize;
            this.wordBottom.FontSize = this.fontSize;
            this.wordTop.Margin = new Thickness(0, 0, 0, 0);
            this.wordBottom.Margin = new Thickness(0, this.marginBas, 0, 0);
        }
    }

}
