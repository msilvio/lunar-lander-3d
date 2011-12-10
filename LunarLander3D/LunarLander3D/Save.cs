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

        XmlSerializer serializer;
        FileStream fileStream;
        
        //Estrutura Serializável para os Scores

        [Serializable]
        public class ScoreData
        {
            public int[] scoreList = new int[10];
        }


        //Estrutura Serializável para os dados do Jogo

        [Serializable]
        public class SaveGameData
        {
            public Vector3 landerPosition;
            public float combustivel;
            public float oxigenio;
        }

        
        //Transcreve a lista de Scores em um arquivo XML

        public void SaveScore(int[] score)
        {
            
            serializer = new XmlSerializer(typeof(ScoreData));
            fileStream = File.Open("highscores.xml", FileMode.OpenOrCreate);

            ScoreData score2 = new ScoreData();

            for (int i = 0; i < 10; i++)
            {
                score2.scoreList[i] = score[i];
            }
            
            serializer.Serialize(fileStream, score2);

            fileStream.Close();

        }

        //Le o arquivo de texto e transcreve seus dados para a Lista de Scores

        public int[] LoadScore()
        {
            if (File.Exists("highscore.xml"))
            {
                fileStream = File.Open("highscores.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
   
                serializer = new XmlSerializer(typeof(ScoreData));

                ScoreData scoreValue = (ScoreData)serializer.Deserialize(fileStream);

                fileStream.Close();

                return scoreValue.scoreList;
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

        //Transcreve as informações de jogo corrente em um arquivo XML

        public void SaveGame(Vector3 LanderPosition, float Combustivel, float Oxigenio)
        {
            serializer = new XmlSerializer(typeof(SaveGameData));
            fileStream = File.Open("SaveGameData.xml", FileMode.OpenOrCreate);

            SaveGameData saveGame = new SaveGameData();

            saveGame.landerPosition = LanderPosition;
            saveGame.oxigenio = Oxigenio;
            saveGame.combustivel = Combustivel;

            serializer.Serialize(fileStream, saveGame);
            fileStream.Close();
        }

        //Carrega e retorna as informações do jogo salvo

        public SaveGameData LoadGame()
        {
            if (!File.Exists("SaveGameData.xml"))
            {
                return null;
            }
            else
            {
                fileStream = File.Open("SaveGameData.xml", FileMode.Open, FileAccess.Read, FileShare.Read);

                serializer = new XmlSerializer(typeof(SaveGameData));

                SaveGameData saveData = (SaveGameData)serializer.Deserialize(fileStream);

                fileStream.Close();

                return saveData;
            }
        }
    }
}
