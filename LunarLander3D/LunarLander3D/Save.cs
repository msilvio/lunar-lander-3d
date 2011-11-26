using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;



namespace LunarLander3D
{
    static class Save
    {
        static StreamReader reader;
        static StreamWriter writer;
        static XmlSerializer serializer;
        static FileStream fileStream;
        
        //Transcreve a lista de Scores em um arquivo, em texto

        public static void SaveScore(int[] score)
        {
            
            serializer = new XmlSerializer(typeof(object));
            fileStream = File.Open("highscores.xml", FileMode.OpenOrCreate);
            writer = new StreamWriter(fileStream);

            //for (int i = 0; i < 10; i++)
            //{
            //    writer.WriteLine(score[i]);
            //}

            writer.Flush();

            serializer.Serialize(fileStream, score.ToString());

            writer.Close();
        }

        //Le o arquivo de texto e transcreve seus dados para a Lista de Scores

        public static int[] LoadScore()
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

        public static void SaveGame()
        {

        }

        public static void LoadGame()
        {

        }
    }
}
