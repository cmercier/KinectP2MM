﻿using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.Toolkit.Interaction;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KinectP2MM
{
    class MainManager
    {
        //main window
        private MainWindow window;

        //kinect manager
        private KinectManager kinectManager;

        // our two hands
        private Tuple<Hand, Hand> hands;

        //list of words to manipulate
        private List<Word> words;

        //list of words couple split
        private List<Tuple<Word, Word>> splitWordsCouple;

        private Bin bin;

        public MainManager(MainWindow window)
        {
            this.window = window;
            this.hands = new Tuple<Hand, Hand>(this.window.leftCursor, this.window.rightCursor);
            this.hands.Item1.path = "images/left_cursor";
            this.hands.Item2.path = "images/right_cursor";

            words = new List<Word>();

            // pas sur, pour ajouter le mot dans l'affichage
            Word newWord = new Word("clement");
            this.window.canvas.Children.Add(newWord);

            foreach (var word in this.window.canvas.Children.OfType<Word>())
            {
                words.Add(word);
            }
                                    
            splitWordsCouple = new List<Tuple<Word, Word>>();

            bin = this.window.corbeille;

            //has to be done at last
            this.kinectManager = new KinectManager(this, this.window.sensorChooserUi);
        }

        public void newImageReceived(UserInfo userInfo)
        {
            var hands = userInfo.HandPointers;
            foreach (var hand in hands)
            {
                updateHand(hand);
            }

            interactionOnText();
        }

        //function to update the model of the hand and to draw the cursor corresponding to the given hand
        private void updateHand(InteractionHandPointer h)
        {
            //current Hand
            Hand hand = h.HandType == InteractionHandType.Left ? hands.Item1 : hands.Item2;

            //update the model of the hand
            hand.x = h.X * window.canvas.Width;
            hand.y = h.Y * window.canvas.Height;

            //check if the hand is grip
            if (!hand.grip && h.HandEventType == InteractionHandEventType.Grip)
            {
                hand.grip = true;
            }
            else
                if (hand.grip && h.HandEventType == InteractionHandEventType.GripRelease)
                {
                    hand.grip = false;
                    hand.attachedObjectName = "";
                }

            //check if the hand is pressed
            if (h.PressExtent > 1 && !hand.pressed)
            {
                hand.pressed = true;
            }
            else
                if (h.PressExtent == 0 && hand.pressed)
                {
                    hand.pressed = false;
                }

            //set the just grip or just release value
            if (h.HandEventType == InteractionHandEventType.Grip && hand.lastEvent != "Grip")
            {
                hand.justGrip = true;
                Hand.distance = ImageTools.getDistance(hands.Item1, hands.Item2);
            }
            else
                hand.justGrip = false;

            //set just release value
            if (h.HandEventType == InteractionHandEventType.GripRelease && hand.lastEvent != "GripRelease")
            {
                hand.justReleased = true;
                Hand.distance = ImageTools.getDistance(hands.Item1, hands.Item2);
            }
            else
                hand.justReleased = false;

            //update of the lastHandEvent
            switch (h.HandEventType)
            {
                case InteractionHandEventType.Grip:
                    hand.lastEvent = "Grip";
                    break;
                case InteractionHandEventType.GripRelease:
                    hand.lastEvent = "GripRelease";
                    break;
                case InteractionHandEventType.None:
                    hand.lastEvent = "None";
                    break;
            }
        }

        
        private void interactionOnText()
        {
            bin.hover = false;
            reinitialize(splitWordsCouple);

            foreach (var word in words.ToList())
            {
                Console.WriteLine(word.wordTop.Content); // pour voir la liste des mots
                word.hover = false;
                moveDetectionWord(word, InteractionHandType.Left);
                moveDetectionWord(word, InteractionHandType.Right);
                

                //if the left hand is on the word
                if (hands.Item1.grip && hands.Item1.attachedObjectName == word.Name)
                {
                    //zoomDetection(word, hands.Item2);
                    rotationDetection(word, hands.Item2);
                    fusionDetection(word, hands.Item2);
                    separationDetection(word, hands.Item2);
                    
                }
                else
                    //if the right hand is on the word
                    if (hands.Item2.grip && hands.Item2.attachedObjectName == word.Name)
                    {
                        //zoomDetection(word, hands.Item1);
                        rotationDetection(word, hands.Item1);
                        fusionDetection(word, hands.Item1);
                        separationDetection(word, hands.Item1);
                    }

            }
        }

        private void reinitialize(List<Tuple<Word, Word>> couplesList)
        {
            foreach (var couple in couplesList.ToList())
            {
                if (ImageTools.getDistance(couple.Item1, couple.Item2) > 20000)
                {
                    couplesList.Remove(couple);
                }
            }

        }

        private void moveDetectionWord(Word word, InteractionHandType which)
        {
            var hand = which == InteractionHandType.Left ? hands.Item1 : hands.Item2;
            var secondHand = which == InteractionHandType.Right ? hands.Item1 : hands.Item2;

            //if the word is on the bin
            if (ImageTools.isOn(word, bin))
            {
                //if we release the hand on the bin, we delete the word
                if (hand.justReleased)
                {
                    this.window.canvas.Children.Remove(word);
                    words.Remove(word);
                }
                bin.hover = true;
            }

            //if the hand is on the text
            if (ImageTools.isOn(hand, word))
            {
                //detection of grip
                if (hand.justGrip)
                {
                    //we attach the label to the hand until the grip is released
                    hand.attachedObjectName = word.Name;

                    //we savethe current rotation of the word
                    double rotationInDegrees = 0;
                    RotateTransform rotation = word.RenderTransform as RotateTransform;
                    if (rotation != null) // Make sure the transform is actually a RotateTransform
                    {
                        rotationInDegrees = rotation.Angle;
                    }
                    word.beginRotation = rotationInDegrees;
                }
                word.hover = true;
            }

            //moving of the attached text
            if (hand.grip && !hand.pressed && hand.attachedObjectName == word.Name)
            {
                //this is to keep the words inside the canvas
                Point newPos = new Point(hand.x + hand.ActualWidth / 2 - word.ActualWidth / 2, hand.y + hand.ActualHeight / 2 - word.ActualHeight / 2);
                if (newPos.X > 0 && newPos.X - word.ActualWidth / 2 < this.window.canvas.Width - word.ActualWidth && newPos.Y > 0 && newPos.Y < this.window.canvas.Height - word.ActualHeight)
                {
                    word.x = newPos.X;
                    word.y = newPos.Y;
                    word.hover = true;
                }
                else if (newPos.X > 0 && newPos.X - word.ActualWidth / 2 < this.window.canvas.Width - word.ActualWidth)
                {
                    word.x = newPos.X;
                }
                else if (newPos.Y > 0 && newPos.Y < this.window.canvas.Height - word.ActualHeight)
                {
                    word.y = newPos.Y;
                }

            }
        }

        // method to manage to zoom detection
        private void zoomDetection(Word word, Hand secondHand)
        {
            //zoom if second hand gripping and without a text attached
            if (secondHand.grip && secondHand.attachedObjectName == "")
            {
                var distance = ImageTools.getDistance(hands.Item1, hands.Item2);
                var difference = distance - Hand.distance;
                var facteur = difference / 1000000 + 1;
                if (difference > 5000 && word.Width * facteur <= Word.MAX_WIDTH)
                {

                    word.Width *= facteur;
                    word.Height *= facteur;
                }
                else
                    if (difference < -5000 && word.Width * facteur >= Word.MIN_WIDTH)
                    {
                        word.Width *= facteur;
                        word.Height *= facteur;
                    }
                Hand.distance = distance;
            }

        }

        // method to manage to rotate detection
        private void rotationDetection(Word word, Hand secondHand)
        {
            //mzoom if second hand open and without a text attached
            if (secondHand.grip && secondHand.attachedObjectName == "")
            {
                double wordRotation = word.beginRotation;

                var newRotation = (wordRotation - ImageTools.getRotation(hands.Item1, hands.Item2)) % 360;
                word.RenderTransform = new RotateTransform(newRotation);
            }

        }

        private void separationDetection(Word word, Hand secondHand)
        {   // Detect if both hands are on the word
            if (secondHand.grip && secondHand.attachedObjectName == word.Name)
            {
                if (word.typeWord == "complete")
                {
                    Word nouveau = word.Duplicate();
                    words.Add(nouveau);
                    Tuple<Word, Word> newCouple = new Tuple<Word, Word>(word, nouveau);
                    splitWordsCouple.Add(newCouple);
                    /*foreach (var c in splitWordsCouple) to test splitWordsCouple
                    {
                        Console.WriteLine(c.Item1.wordBottom.Content);
                        Console.WriteLine(c.Item1.wordTop.Content);
                        Console.WriteLine(c.Item2.wordBottom.Content);
                        Console.WriteLine(c.Item2.wordTop.Content);
                    }*/

                    

                    this.window.canvas.Children.Add(nouveau);
                    secondHand.attachedObjectName = nouveau.Name;                    
                }                
            }

        }

        // method to test if the fusion between 2 words is allowed, then it do the fusion
        private void fusionDetection(Word currentWord, Hand secondHand)
        {
            foreach (var word in words.ToList())
            {
                if (word != currentWord && !forbiddenCoupleDetection(word, currentWord))
                {
                    if ((word.typeWord == "top" && currentWord.typeWord == "bottom") || (currentWord.typeWord == "top" && word.typeWord == "bottom"))
                    {
                        if (ImageTools.getDistance(word, currentWord) < 4000)
                        {
                            //do fusion
                            currentWord.Fusion(word);
                            this.window.canvas.Children.Remove(word);
                            words.Remove(word);
                        }
                    }
                }
            }
        }
        
        //method to detect if the couple of 2 words is in splitWordsCouple or not
        private bool forbiddenCoupleDetection(Word word1, Word word2)
        {
            Tuple<Word, Word> currentCouple = new Tuple<Word, Word>(word1, word2);

            foreach (var couple in splitWordsCouple.ToList())
            {
                if (couple.Item1.wordTop.Content == currentCouple.Item1.wordTop.Content && 
                    couple.Item1.wordBottom.Content == currentCouple.Item1.wordBottom.Content &&
                    couple.Item2.wordTop.Content == currentCouple.Item2.wordTop.Content &&
                    couple.Item2.wordBottom.Content == currentCouple.Item2.wordBottom.Content)
                {
                    return true;
                }
            }

            return false;
        }

        //to set the text of the information label on the screen
        public void setInformationText(String s)
        {
            window.userText.Content = s;
        }
    }
}
