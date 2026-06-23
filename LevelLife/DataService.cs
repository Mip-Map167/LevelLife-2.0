using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace LevelLife
{
    [Serializable]
    public class SaveData
    {
        public PlayerData Player { get; set; }
        public List<TaskData> Tasks { get; set; }
        public List<TaskData> DeletedTasks { get; set; }
    }

    public class PlayerData
    {
        public string Name { get; set; }
        public string Goal { get; set; }
        public int Level { get; set; }
        public int CurrentXP { get; set; }
        public int TotalXP { get; set; }
    }

    public class TaskData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int XpReward { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    public static class DataService
    {
        public static readonly string FilePath = Path.Combine(Application.StartupPath, "levelife_data.json");
        private static readonly string BackupPath = FilePath + ".backup";

        public static void SaveWithHistory(Player player, List<Task> tasks, List<Task> deletedTasks)
        {
            try
            {
                // Создаём резервную копию перед сохранением
                if (File.Exists(FilePath))
                {
                    File.Copy(FilePath, BackupPath, true);
                }

                var save = new SaveData
                {
                    Player = new PlayerData
                    {
                        Name = player.Name,
                        Goal = player.Goal,
                        Level = player.Level,
                        CurrentXP = player.CurrentXP,
                        TotalXP = player.TotalXP
                    },
                    Tasks = tasks.ConvertAll(t => new TaskData
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        XpReward = t.XpReward,
                        IsCompleted = t.IsCompleted,
                        CreationDate = t.CreationDate,
                        CompletionDate = t.CompletionDate
                    }),
                    DeletedTasks = deletedTasks.ConvertAll(t => new TaskData
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        XpReward = t.XpReward,
                        IsCompleted = t.IsCompleted,
                        CreationDate = t.CreationDate,
                        CompletionDate = t.CompletionDate
                    })
                };
                string json = JsonConvert.SerializeObject(save, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\nПопробуйте восстановить данные из резервной копии.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static (Player player, List<Task> tasks, List<Task> deletedTasks) LoadWithHistory()
        {
            if (!File.Exists(FilePath))
            {
                return CreateDefaultData();
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                var save = JsonConvert.DeserializeObject<SaveData>(json);
                if (save == null) throw new Exception("Файл повреждён");

                var player = new Player(save.Player.Name);
                player.Goal = save.Player.Goal ?? "Стать лучше";
                typeof(Player).GetProperty("Level")?.SetValue(player, save.Player.Level);
                typeof(Player).GetField("CurrentXP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, save.Player.CurrentXP);
                typeof(Player).GetField("TotalXP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, save.Player.TotalXP);

                var tasks = save.Tasks.ConvertAll(t => new Task
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    XpReward = t.XpReward,
                    IsCompleted = t.IsCompleted,
                    CreationDate = t.CreationDate,
                    CompletionDate = t.CompletionDate
                });

                var deletedTasks = save.DeletedTasks?.ConvertAll(t => new Task
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    XpReward = t.XpReward,
                    IsCompleted = t.IsCompleted,
                    CreationDate = t.CreationDate,
                    CompletionDate = t.CompletionDate
                }) ?? new List<Task>();

                return (player, tasks, deletedTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}\nПопытка восстановить данные из резервной копии...", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Попытка восстановить из бэкапа
                if (File.Exists(BackupPath))
                {
                    try
                    {
                        string json = File.ReadAllText(BackupPath);
                        var save = JsonConvert.DeserializeObject<SaveData>(json);
                        if (save != null)
                        {
                            var player = new Player(save.Player.Name);
                            player.Goal = save.Player.Goal ?? "Стать лучше";
                            typeof(Player).GetProperty("Level")?.SetValue(player, save.Player.Level);
                            typeof(Player).GetField("CurrentXP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, save.Player.CurrentXP);
                            typeof(Player).GetField("TotalXP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, save.Player.TotalXP);

                            var tasks = save.Tasks.ConvertAll(t => new Task
                            {
                                Id = t.Id,
                                Title = t.Title,
                                Description = t.Description,
                                XpReward = t.XpReward,
                                IsCompleted = t.IsCompleted,
                                CreationDate = t.CreationDate,
                                CompletionDate = t.CompletionDate
                            });

                            var deletedTasks = save.DeletedTasks?.ConvertAll(t => new Task
                            {
                                Id = t.Id,
                                Title = t.Title,
                                Description = t.Description,
                                XpReward = t.XpReward,
                                IsCompleted = t.IsCompleted,
                                CreationDate = t.CreationDate,
                                CompletionDate = t.CompletionDate
                            }) ?? new List<Task>();

                            MessageBox.Show("Данные успешно восстановлены из резервной копии.", "Восстановление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return (player, tasks, deletedTasks);
                        }
                    }
                    catch { /* если бэкап тоже повреждён — создаём новые данные */ }
                }

                return CreateDefaultData();
            }
        }

        private static (Player player, List<Task> tasks, List<Task> deletedTasks) CreateDefaultData()
        {
            var player = new Player("Игрок");
            player.Goal = "Стать лучше";
            var tasks = new List<Task>
            {
                new Task("Добро пожаловать в игру!", "У вас всё получится! Выполни эту задачу, чтобы начать свой путь.", 50)
            };
            var deletedTasks = new List<Task>();
            SaveWithHistory(player, tasks, deletedTasks);
            return (player, tasks, deletedTasks);
        }
    }
}