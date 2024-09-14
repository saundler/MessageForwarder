using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MessageForwarder.Data
{
    public class BadWordsRepository
    {
        private readonly MyDbContext _context;
        private readonly HashSet<BadWord> fastdb;

        public BadWordsRepository(MyDbContext context)
        {
            _context = context;
            fastdb = new HashSet<BadWord>(_context.BadWords);
        }

        // Метод для добавления нового слова в базу данных
        public void AddBadWord(string word)
        {
            word = word.Trim().Split(' ')[0].ToLower();
            // Проверяем, существует ли слово уже в базе данных
            if (_context.BadWords.Any(bw => bw.Word == word))
            {
                throw new InvalidOperationException("Слово уже существует в базе данных.");
            }

            var badWord = new BadWord { Word = word };
            fastdb.Add(badWord);
            _context.BadWords.Add(badWord);
            _context.SaveChanges();
        }

        // Метод для удаления слова из базы данных по его идентификатору
        public void DeleteBadWord(int id)
        {
            var badWord = _context.BadWords.Find((long)id);
            if (badWord != null)
            {
                fastdb.Remove(badWord);
                _context.BadWords.Remove(badWord);
                _context.SaveChanges();
                if (fastdb.Count == 0)
                {
                    _context.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name = 'BadWords'");
                }
            }
        }

        // Метод для удаления всех слов из базы данных
        public void DeleteAllBadWords()
        {
            // Очистка in-memory HashSet
            fastdb.Clear();

            // Удаление всех записей из базы данных
            var allBadWords = _context.BadWords.ToList();
            _context.BadWords.RemoveRange(allBadWords);
            _context.SaveChanges();

            // Сброс автоинкрементного счётчика в таблице BadWords (только для SQLite)
            _context.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name = 'BadWords'");
        }


        // Метод для поиска слова в базе данных по его идентификатору
        public BadWord GetBadWordById(int id)
        {
            return fastdb.FirstOrDefault(bw => bw.Id == id);
        }

        // Метод для получения всех слов из базы данных
        public IEnumerable<BadWord> GetAllBadWords()
        {
            return fastdb;
        }

        // Метод для поиска слова в базе данных по его содержимому
        public BadWord GetBadWordByContent(string word)
        {
            return fastdb.FirstOrDefault(bw => bw.Word == word);
        }
    }
}
