using System;
using System.IO;
using System.Linq;

namespace Students.Web.Models
{
    public class StudentFile
    {
        public int Year { get; set; }

        public string University { get; set; }

        public string Student { get; set; }

        public string Faculty { get; set; }

        public string Form { get; set; }

        public StudentFile()
        {
        }

        public StudentFile(int year, string university, string student, string faculty, string form)
        {
            Year = year;
            University = university;
            Student = student;
            Faculty = faculty;
            Form = form;
        }

        public string CheckStudentFile()
        {
            if (Year < 1900 || Year > 2025)
            {
                return "(0) Ошибка указания года (символы с 1 по 4)";
            }

            if (string.IsNullOrWhiteSpace(University))
            {
                return "(1) Код вуза пустой или равен NULL (символы с 5 по 10)";
            }

            if (University.Length != 6)
            {
                return "(2) Длина кода вуза не равна 6 (символы с 5 по 10)";
            }

            if (string.IsNullOrWhiteSpace(Student))
            {
                return "(3) Код данных пустой или равен NULL (символ 11)";
            }

            if (Student != "S")
            {
                return "(4) Указаны не данные студента (символ 11)";
            }

            if (string.IsNullOrWhiteSpace(Faculty))
            {
                return "(5) Код факультета пустой или равен NULL (символ 12)";
            }

            if (Faculty.Length != 1)
            {
                return "(6) Длина кода факультета не равна 1 (символ 12)";
            }

            if (!("MTI".Any(q=> q.ToString() == Faculty)))
            {
                return "(7) Код не равен допустимому значению \"M\" \"T\" \"I\" (символ 12)";
            }

            if (string.IsNullOrWhiteSpace(Form))
            {
                return "(8) Код формы обучения пустой или равен NULL (символ 13)";
            }

            if (Form.Length != 1)
            {
                return "(9) Длина кода формы обучения не равна 1 (символ 13)";
            }

            if (!("DZ".Any(q => q.ToString() == Form)))
            {
                return "(10) Код не равен допустимому значению \"D\" \"Z\" (символ 13)";
            }
            return null;
        }

        public string ReadFile(string root)
        {
            var checkStatus = CheckStudentFile();
            if (checkStatus == null)
            {
                var file = Path.Combine(root, "files", $"{Year}{University}{Student}{Faculty}{Form}.txt");
                if (System.IO.File.Exists(file))
                {
                    var text = System.IO.File.ReadAllText(file).Replace(Environment.NewLine,"<br/>");
                    return text;
                }
                else
                {
                    throw new FileNotFoundException("Файл по указанной моделе не найден");
                }
            }
            else
            {
                throw new ArgumentException("Ошибка проверки класса " + checkStatus);
            }
        }

        public void SaveFile(string root,string content)
        {
            var checkStatus = CheckStudentFile();

            if (checkStatus == null)
            {
                var file = Path.Combine(root, "files", $"{Year}{University}{Student}{Faculty}{Form}.txt");
                if (System.IO.File.Exists(file))
                {
                    throw new Exception("Файл уже существует");
                }
                File.WriteAllText(file, content);
            }
            else
            {
                throw new ArgumentException("Ошибка проверки класса " + checkStatus);
            }
        }

        public static StudentFile Parse(string fileName)
        {
            if (fileName.Length < 13)
            {
                return null;
            }
            var model = new StudentFile
            {
                University = fileName.Substring(4,6),
                Student = fileName.Substring(10,1),
                Faculty = fileName.Substring(11,1),
                Form = fileName.Substring(12,1)
            };
            int.TryParse(fileName.Substring(0, 4), out var year);
            model.Year = year;
            return model;
        }
    }
}