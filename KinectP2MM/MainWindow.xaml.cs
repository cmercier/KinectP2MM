﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;


namespace KinectP2MM
{
    // Logique d'interaction pour MainWindow.xaml
    public partial class MainWindow : Window
    {
        public SequenceManager sequenceManager;
        private bool fullScreen;
        private bool inputOpen;
        private JsonManager jsonManager;
        private WordType wordType;
        private String lastAddedWord;
        private Configuration config;
        private Editor editor;


        public MainWindow()
        {
            InitializeComponent();
            //references the OnLoaded function on the OnLoaded event
            Loaded += WindowLoaded;
            KeyUp += KeyUpManager;
            KeyDown += KeyDownManager;
            Closed += WindowClosed;
        }

        // Execute startup tasks
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            fullScreen = false;
            inputOpen = false;
            this.sequenceManager = new SequenceManager(this);
            this.Activate();
            jsonManager = new JsonManager();
            lastAddedWord = "";

            updateColors();
        }

        public void updateColors(String foreground = "", String background = "")
        {

            if (background != null)
            {
                if (background.Equals("")) background = Properties.Settings.Default.background_color;
                var bc = new BrushConverter();
                Background = (Brush)bc.ConvertFrom(background);
            }
            if (foreground != null)
            {
                if (foreground.Equals("")) foreground = Properties.Settings.Default.foreground_color;
                var bc = new BrushConverter();
                InputTextBox.Foreground = (Brush)bc.ConvertFrom(foreground);
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (config != null)
                config.Close();
            if (editor != null)
                editor.Close();           
        }

        // Manage Key Events
        private void KeyUpManager(object sender, KeyEventArgs e)
        {
            if (!inputOpen)
            {
                if ((e.Key >= Key.D1 && e.Key <= Key.D9) || (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9))
                {
                    //create list from JSON
                    List<Sequence> sequences = jsonManager.load();

                    if ((e.Key == Key.D1 || e.Key == Key.NumPad1) && sequences.Count >= 1)
                        this.sequenceManager.loadSequence(sequences[0]);
                    else if ((e.Key == Key.D2 || e.Key == Key.NumPad2) && sequences.Count >= 2)
                        this.sequenceManager.loadSequence(sequences[1]);
                    else if ((e.Key == Key.D3 || e.Key == Key.NumPad3) && sequences.Count >= 3)
                        this.sequenceManager.loadSequence(sequences[2]);
                    else if ((e.Key == Key.D4 || e.Key == Key.NumPad4) && sequences.Count >= 4)
                        this.sequenceManager.loadSequence(sequences[3]);
                    else if ((e.Key == Key.D5 || e.Key == Key.NumPad5) && sequences.Count >= 5)
                        this.sequenceManager.loadSequence(sequences[4]);
                    else if ((e.Key == Key.D6 || e.Key == Key.NumPad6) && sequences.Count >= 6)
                        this.sequenceManager.loadSequence(sequences[5]);
                    else if ((e.Key == Key.D7 || e.Key == Key.NumPad7) && sequences.Count >= 7)
                        this.sequenceManager.loadSequence(sequences[6]);
                    else if ((e.Key == Key.D8 || e.Key == Key.NumPad8) && sequences.Count >= 8)
                        this.sequenceManager.loadSequence(sequences[7]);
                    else if ((e.Key == Key.D9 || e.Key == Key.NumPad9) && sequences.Count >= 9)
                        this.sequenceManager.loadSequence(sequences[8]);
                }

                if (e.Key == Key.Back)
                    this.sequenceManager.loadSequence(new Sequence());

                if (e.Key == Key.Escape)
                    toggleFullscreen(false);
                if (e.Key == Key.F11 || e.Key == Key.F)
                    toggleFullscreen(!fullScreen);

                if (e.Key == Key.N)
                    showAddWord();
                if (e.Key == Key.H)
                    showAddWordHaut();
                if (e.Key == Key.B)
                    showAddWordBas();

                if (e.Key == Key.E && !fullScreen)
                    openEditor();
                if (e.Key == Key.C && !fullScreen)
                    openConfig();

                if (e.Key == Key.O)
                    openFileChooser();

                if (e.Key == Key.R)
                    this.sequenceManager.toggleRotation();
                if (e.Key == Key.Z)
                    this.sequenceManager.toggleZoom();
                if (e.Key == Key.P)
                    changeFont();
                if (e.Key == Key.I)
                    this.sequenceManager.toggleAPI();

            }
            else
            {
                if (e.Key == Key.Enter)
                {
                    validateInput();
                }
                if (e.Key == Key.Escape)
                    cancelInput();
            }
        }
        // Manage Key Events
        private void KeyDownManager(object sender, KeyEventArgs e)
        {
            if (!inputOpen)
            {                
                if (e.Key == Key.Up)
                    this.sequenceManager.kinectManager.incElevation();

                if (e.Key == Key.Down)
                    this.sequenceManager.kinectManager.decElevation();
            }
        }

        private void changeFont()
        {
            if (App.FONT_FAMILY == KinectP2MM.Properties.Settings.Default.font1_full)
                App.loadFont(Properties.Settings.Default.font2_full);
            else
                App.loadFont(Properties.Settings.Default.font1_full);
        }


        private void openEditor()
        {
            editor = new Editor(this);
            editor.Show();
        }

        private void openConfig()
        {
            config = new Configuration(this);
            config.Show();
        }

        private void openFileChooser()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".p2mm";
            dlg.Filter = "Fichiers P2MM (*.p2mm)|*.p2mm";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                loadJson(filename);
            }
        }

        public void loadJson(string filename)
        {
            jsonManager = new JsonManager(filename);
            this.sequenceManager.loadSequence(new Sequence());
        }



        private void showAddWord()
        {
            inputOpen = true;
            InputBox.Visibility = System.Windows.Visibility.Visible;
            InputTextBox.Focusable = true;
            InputTextBox.FontFamily = new FontFamily(App.FONT_FAMILY);
            InputTextBox.FontSize = 100;
            Keyboard.Focus(InputTextBox);
            wordType = WordType.FULL;
        }

        private void showAddWordBas()
        {
            inputOpen = true;
            InputBox.Visibility = System.Windows.Visibility.Visible;
            InputTextBox.Focusable = true;
            InputTextBox.FontFamily = new FontFamily(App.FONT_FAMILY_BOTTOM);
            InputTextBox.FontSize = 48;
            Keyboard.Focus(InputTextBox);
            wordType = WordType.BOTTOM;
        }

        private void showAddWordHaut()
        {
            inputOpen = true;
            InputBox.Visibility = System.Windows.Visibility.Visible;
            InputTextBox.Focusable = true;
            InputTextBox.FontFamily = new FontFamily(App.FONT_FAMILY_TOP);
            InputTextBox.FontSize = 48;
            Keyboard.Focus(InputTextBox);
            wordType = WordType.TOP;
        }


        private void validateInput()
        {
            inputOpen = false;
            lastAddedWord = InputTextBox.Text;
            this.sequenceManager.addWord(lastAddedWord, wordType, false);
            InputBox.Visibility = System.Windows.Visibility.Collapsed;
            InputTextBox.Text = String.Empty;
        }

        private void cancelInput()
        {
            inputOpen = false;
            InputBox.Visibility = System.Windows.Visibility.Collapsed;
            InputTextBox.Text = String.Empty;
        }

        private void toggleFullscreen(bool fs)
        {
            if (fs)
            {
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.Topmost = true;
                fullScreen = true;
            }
            else
            {
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowState = System.Windows.WindowState.Normal;
                this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                this.Topmost = false;
                fullScreen = false;
            }
        }


        public void reload()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}


