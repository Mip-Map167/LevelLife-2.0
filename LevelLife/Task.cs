using System;

namespace LevelLife
{
    public class Task
    {
        private static int _nextId = 1;
        public DateTime? CompletionDate { get; set; } // Дата выполнения задачи
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int XpReward { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreationDate { get; set; }

        public Task()
        {
            Id = _nextId++;
            CreationDate = DateTime.Now;
            IsCompleted = false;
        }

        public Task(string title, string description, int xpReward) : this()
        {
            Title = title;
            Description = description;
            XpReward = xpReward;
        }

        public int Complete()
        {
            if (IsCompleted) return 0;
            IsCompleted = true;
            return XpReward;
        }

        // Для восстановления счётчика при загрузке из файла
        public static void SetNextId(int maxId)
        {
            _nextId = maxId + 1;
        }
    }
}