﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.IO;

namespace KinectP2MM
{
    class JsonManager
    {

        private String file;

        public JsonManager(String file="")
        {
            this.file = file;
        }

        public List<Sequence> load()
        {
            List<Sequence> listSequences = new List<Sequence>();
            if (file != String.Empty)
            {
                WordType type;

                //lecture du fichier json et creation des mots
                foreach (JsonSequence jSeq in JsonConvert.DeserializeObject<List<JsonSequence>>(File.ReadAllText(file)))
                {

                    List<Word> words = new List<Word>();
                    foreach (var jWord in jSeq.words)
                    {
                        if (jWord.type == 0)
                            type = WordType.FULL;
                        else if (jWord.type == 1)
                            type = WordType.BOTTOM;
                        else if (jWord.type == 2)
                            type = WordType.TOP;
                        else
                            type = WordType.FULL;

                        words.Add(new Word(jWord.content, jWord.fontFamily, jWord.fontSize, jWord.x, jWord.y, type, jSeq.foregroundColor));
                    }
                    listSequences.Add(new Sequence(words, jSeq.canZoom, jSeq.canRotate, jSeq.leftBottomCornerWords,
            jSeq.rightBottomCornerWords, jSeq.leftTopCornerWords, jSeq.rightTopCornerWords,
            jSeq.foregroundColor, jSeq.backgroundColor));
                }
            }
            return listSequences;
        }

        public void save(List<Sequence> listSequences, String file)
        {
            // Exemple de creation de json et d'ecriture sur fichier
            List<JsonSequence> listJsonSeq = new List<JsonSequence>();

            foreach (Sequence sequence in listSequences)
            {
                List<JsonWord> list = new List<JsonWord>();
                foreach (Word word in sequence.words)
                {
                    list.Add(new JsonWord((String)word.wordTop.Content, (int)word.x, (int)word.y, (int)word.typeWord, (String)word.fontFamily, (double)word.fontSize));
                }
                listJsonSeq.Add(new JsonSequence(list, sequence.canZoom, sequence.canRotate, sequence.leftBottomCornerWords,
            sequence.rightBottomCornerWords, sequence.leftTopCornerWords, sequence.rightTopCornerWords,
            sequence.foregroundColor, sequence.backgroundColor));
            }

            string json = JsonConvert.SerializeObject(listJsonSeq, Formatting.Indented);
            File.WriteAllText(file, json);
        }
     }

    class JsonWord
    {
        public JsonWord(String content, int x, int y, int type, String fontFamily, double fontSize)
        {
            this.content = content;
            this.x = x;
            this.y = y;
            this.type = type;
            this.fontFamily = fontFamily;
            this.fontSize = fontSize;
        }
        public int x { get; set; }
        public int y { get; set; }
        public String content { get; set; }
        public int type { get; set; }
        public String fontFamily { get; set; }
        public double fontSize { get; set; }
    }


    class JsonSequence
    {
        public JsonSequence(List<JsonWord> words, bool canZoom, bool canRotate, List<String> leftBottomCornerWords,
            List<String> rightBottomCornerWords, List<String> leftTopCornerWords, List<String> rightTopCornerWords,
            String foreground, String background)
        {
            this.words = words;
            this.canZoom = canZoom;
            this.canRotate = canRotate;
            this.leftBottomCornerWords = leftBottomCornerWords;
            this.rightBottomCornerWords = rightBottomCornerWords;
            this.leftTopCornerWords = leftTopCornerWords;
            this.rightTopCornerWords = rightTopCornerWords;
            this.foregroundColor = foreground;
            this.backgroundColor = background;
        }
        public List<JsonWord> words { get; set; }
        public bool canZoom { get; set; }
        public bool canRotate { get; set; }
        public List<String> leftBottomCornerWords { get; set; }
        public List<String> rightBottomCornerWords { get; set; }
        public List<String> leftTopCornerWords { get; set; }
        public List<String> rightTopCornerWords { get; set; }
        public String foregroundColor { get; set; }
        public String backgroundColor { get; set; }
    }

}
