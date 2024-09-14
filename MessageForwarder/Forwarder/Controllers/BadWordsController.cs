using System.Text;
using MessageForwarder.Data;
using Microsoft.AspNetCore.Mvc;

namespace Forwarder.Controllers
{
    public class BadWordsController : Controller
    {
        private readonly BadWordsRepository _badWordsRepository;

        public BadWordsController(BadWordsRepository badWordsRepository)
        {
            _badWordsRepository = badWordsRepository;
        }

        // Отображение всех плохих слов
        public IActionResult Index()
        {
            var badWords = _badWordsRepository.GetAllBadWords();
            return View(badWords);
        }

        // Метод для удаления слова по его идентификатору
        [HttpPost]
        public IActionResult Delete(int id)
        {
            _badWordsRepository.DeleteBadWord(id);
            return RedirectToAction(nameof(Index));
        }

        // Метод для отображения формы добавления нового слова
        public IActionResult Add()
        {
            return View();
        }

        // Метод для обработки добавления нового слова
        [HttpPost]
        public IActionResult Add(string newWord)
        {
            if (!string.IsNullOrWhiteSpace(newWord))
            {
                try
                {
                    _badWordsRepository.AddBadWord(newWord);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View();
        }
        
        // Метод для очистки всех слов
        [HttpPost]
        public IActionResult ClearAll()
        {
            _badWordsRepository.DeleteAllBadWords();
            return RedirectToAction(nameof(Index));
        }
        
        // Метод для показа страницы импорта
        public IActionResult Import()
        {
            return View();
        }

        // Метод для импорта запрещённых слов из CSV файла
        [HttpPost]
        public IActionResult Import(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Файл не выбран.";
                return RedirectToAction(nameof(Import));
            }

            try
            {
                using (var stream = new StreamReader(csvFile.OpenReadStream(), Encoding.UTF8))
                {
                    while (!stream.EndOfStream)
                    {
                        try
                        {
                            var line = stream.ReadLine();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                // Добавляем каждое слово, убирая лишние пробелы
                                _badWordsRepository.AddBadWord(line.Trim());
                            }
                        }
                        catch  { }
                        
                    }
                }

                TempData["SuccessMessage"] = "Слова успешно импортированы!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при обработке файла: {ex.Message}";
            }

            return RedirectToAction(nameof(Import));
        }

    }
}