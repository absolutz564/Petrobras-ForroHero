using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ForroHero.Scripts.Ranking
{
    [Serializable]
    public class RankingEntry
    {
        public int Score;
        public string PhotoPath;
        public long Ticks; // UTC ticks de quando foi registrado (desempate)
    }

    /// <summary>
    /// Ranking diário top-10, persistido em disco, que reseta às 03:00 (horário de
    /// Brasília, UTC-3). Eventos que viram a noite (até ~03:00) ainda contam no ranking
    /// do dia anterior — o corte do "dia" é às 03:00, não à meia-noite.
    /// Reset é preguiçoso: acontece na primeira leitura/escrita após cruzar as 03:00.
    /// Requer o relógio do device correto (UtcNow é independente de fuso).
    /// </summary>
    public static class DailyRanking
    {
        public const int MaxEntries = 10;
        private const string FileName = "ranking.json";
        private const string PhotoDir = "ranking_photos";

        [Serializable]
        private class RankingData
        {
            public string DayKey = "";
            public List<RankingEntry> Entries = new List<RankingEntry>();
        }

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);
        private static string PhotoDirPath => Path.Combine(Application.persistentDataPath, PhotoDir);

        /// <summary>
        /// Chave do "dia do ranking". Um instante pertence ao dia D se está em
        /// [D 03:00, (D+1) 03:00) no horário de Brasília. BRT = UTC-3; desloca mais 3h
        /// para jogar o corte do dia às 03:00 (== UTC-6).
        /// </summary>
        public static string CurrentDayKey()
        {
            DateTime brt = DateTime.UtcNow.AddHours(-3);
            return brt.AddHours(-3).ToString("yyyy-MM-dd");
        }

        public static List<RankingEntry> GetTop10()
        {
            return Load().Entries;
        }

        /// <summary>
        /// Registra uma pontuação com a foto. Copia a foto para um arquivo próprio do
        /// ranking (a foto original é sobrescrita a cada jogada). Mantém só o top 10.
        /// </summary>
        public static void AddEntry(int score, string sourcePhotoPath)
        {
            RankingData data = Load();

            data.Entries.Add(new RankingEntry
            {
                Score = score,
                PhotoPath = CopyPhoto(sourcePhotoPath),
                Ticks = DateTime.UtcNow.Ticks
            });

            // Ordena: maior score primeiro; empate -> quem chegou antes.
            data.Entries.Sort((a, b) =>
            {
                int c = b.Score.CompareTo(a.Score);
                return c != 0 ? c : a.Ticks.CompareTo(b.Ticks);
            });

            // Remove o excedente (e apaga as fotos que saíram do top 10).
            for (int i = data.Entries.Count - 1; i >= MaxEntries; i--)
            {
                TryDelete(data.Entries[i].PhotoPath);
                data.Entries.RemoveAt(i);
            }

            Save(data);
        }

        // ------------------------------------------------------------------

        private static RankingData Load()
        {
            RankingData data;
            try
            {
                data = File.Exists(FilePath)
                    ? (JsonUtility.FromJson<RankingData>(File.ReadAllText(FilePath)) ?? new RankingData())
                    : new RankingData();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Ranking] Falha ao ler ranking, recriando: " + e.Message);
                data = new RankingData();
            }

            string today = CurrentDayKey();
            if (data.DayKey != today)
            {
                // Virou o dia (passou das 03:00) -> reseta tudo.
                ClearPhotos();
                data = new RankingData { DayKey = today };
                Save(data);
            }
            return data;
        }

        private static void Save(RankingData data)
        {
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(data)); }
            catch (Exception e) { Debug.LogError("[Ranking] Falha ao salvar: " + e.Message); }
        }

        private static string CopyPhoto(string sourcePhotoPath)
        {
            if (string.IsNullOrEmpty(sourcePhotoPath) || !File.Exists(sourcePhotoPath))
                return "";
            try
            {
                Directory.CreateDirectory(PhotoDirPath);
                string dest = Path.Combine(PhotoDirPath, "rank_" + DateTime.UtcNow.Ticks + ".png");
                File.Copy(sourcePhotoPath, dest, true);
                return dest;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Ranking] Falha ao copiar foto: " + e.Message);
                return "";
            }
        }

        private static void ClearPhotos()
        {
            try { if (Directory.Exists(PhotoDirPath)) Directory.Delete(PhotoDirPath, true); }
            catch (Exception e) { Debug.LogWarning("[Ranking] Falha ao limpar fotos: " + e.Message); }
        }

        private static void TryDelete(string path)
        {
            try { if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path); }
            catch { /* ignora */ }
        }
    }
}
