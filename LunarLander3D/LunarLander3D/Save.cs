using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;



namespace LunarLander3D
{
    public class Save
    {
        StreamReader reader;
        StreamWriter writer;
        XmlSerializer serializer;
        FileStream fileStream;
        
        

        [Serializable]
        public class ScoreData
        {
            public int[] scoreList = new int[10];
        }
        
        //Transcreve a lista de Scores em um arquivo, em texto

        public void SaveScore(int[] score)
        {
            
            serializer = new XmlSerializer(typeof(ScoreData));
            fileStream = File.Open("highscores.xml", FileMode.OpenOrCreate);
            writer = new StreamWriter(fileStream);

            ScoreData score2 = new ScoreData();

            for (int i = 0; i < 10; i++)
            {
                score2.scoreList[i] = score[i];
            }
            
            

            serializer.Serialize(fileStream, score2);

            //for (int i = 0; i < 10; i++)
            //{
            //    writer.WriteLine(score[i]);
            //}

            writer.Flush();

            writer.Close();
        }

        //Le o arquivo de texto e transcreve seus dados para a Lista de Scores

        public int[] LoadScore()
        {
            if (File.Exists("highscore.xml"))
            {
                fileStream = File.Open("highscores.xml", FileMode.Open);
                //reader = new StreamReader("highscores.xml");

                serializer = new XmlSerializer(typeof (String));
                serializer.Deserialize(fileStream);

                int[] list = new int[10];
                for (int i = 0; i < 0; i++)
                {
                    list[i] = int.Parse(reader.ReadLine());
                }
                return list;
            }
            else
            {
                int[] list = new int[10];
                for (int i = 0; i < 0; i++)
                {
                    list[i] = 0;
                }
                return list;
            }
        }

        public void SaveGame()
        {

        }

        public void LoadGame()
        {

        }
    }
}
