using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Students.Web.Models;

namespace Students.Tests
{
    public class StudentTests
    {
        private const string CorrectFileName = "2019esstu1SMD";
        private const string CorrectYear = "2019";
        private const string CorrectUniversity = "esstu1";
        private const string CorrectStudent = "S";
        private const string CorrectFaculty1 = "M";
        private const string CorrectFaculty2 = "T";
        private const string CorrectFaculty3 = "I";
        private const string CorrectForm1 = "D";
        private const string CorrectForm2 = "Z";

        private const string IncorrectFileName = "2019esstu1";
        private const string IncorrectYear1 = "2120";
        private const string IncorrectYear2 = "awbd";
        private const string IncorrectYear3 = "1899";
        private const string IncorrectUniversity = "bsu";
        private const string IncorrectStudent = "T";
        private const string IncorrectFaculty = "Z";
        private const string IncorrectForm = "M";

        [Test] 
        [TestCase(CorrectFileName)]
        public void Parse_ShouldNotReturnNull_IfCorrectDataLength(string fileName)
        {
            var s = StudentFile.Parse(fileName);
            
            Assert.IsNotNull(s);
            Assert.Greater(s.Year, 0);
        }

        [Test]
        [TestCase(IncorrectFileName)]
        public void Parse_ShouldReturnNull_IfIncorrectData(string fileName)
        {
            var s = StudentFile.Parse(fileName);

            Assert.IsNull(s);
        }

        [Test]
        [TestCase(CorrectYear, CorrectUniversity , CorrectStudent, CorrectFaculty1, CorrectForm1)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty1, CorrectForm2)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm1)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm2)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm1)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm2)]
        public void Check_ShouldReturnNull_IfAllDataCorrect(string year,string university,string student,string faculty,string form)
        {
            var fileName = $"{year}{university}{student}{faculty}{form}";
            var s = StudentFile.Parse(fileName);

            var result = s.CheckStudentFile();

            Assert.IsNotNull(s);
            Assert.IsNull(result);
        }
        [Test]
        [TestCase(IncorrectYear1, CorrectUniversity, CorrectStudent, CorrectFaculty1, CorrectForm1)]
        [TestCase(IncorrectYear1, CorrectUniversity, CorrectStudent, CorrectFaculty1, CorrectForm2)]
        [TestCase(IncorrectYear1, CorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm1)]
        [TestCase(IncorrectYear1, CorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm2)]
        [TestCase(IncorrectYear1, CorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm1)]
        [TestCase(IncorrectYear1, CorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm2)]

        [TestCase(IncorrectYear2, CorrectUniversity, CorrectStudent, CorrectFaculty1, CorrectForm1)]
        [TestCase(IncorrectYear2, CorrectUniversity, CorrectStudent, CorrectFaculty1, CorrectForm2)]
        [TestCase(IncorrectYear2, CorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm1)]
        [TestCase(IncorrectYear2, CorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm2)]
        [TestCase(IncorrectYear2, CorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm1)]
        [TestCase(IncorrectYear2, CorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm2)]

        [TestCase(IncorrectYear3, CorrectUniversity, CorrectStudent, CorrectFaculty1, CorrectForm1)]
        [TestCase(IncorrectYear3, CorrectUniversity, CorrectStudent, CorrectFaculty1, CorrectForm2)]
        [TestCase(IncorrectYear3, CorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm1)]
        [TestCase(IncorrectYear3, CorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm2)]
        [TestCase(IncorrectYear3, CorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm1)]
        [TestCase(IncorrectYear3, CorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm2)]
        public void Check_ShouldReturnErrorMessage_IfYearIsIncorrect(string year, string university, string student, string faculty, string form)
        {
            var fileName = $"{year}{university}{student}{faculty}{form}";
            var s = StudentFile.Parse(fileName);

            var result = s.CheckStudentFile();

            Assert.IsNotNull(s);
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.StartsWith("(0)"));
        }

        [Test]
        [TestCase(CorrectYear, IncorrectUniversity, CorrectStudent, CorrectFaculty1, CorrectForm1)]
        [TestCase(CorrectYear, IncorrectUniversity, CorrectStudent, CorrectFaculty1, CorrectForm2)]
        [TestCase(CorrectYear, IncorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm1)]
        [TestCase(CorrectYear, IncorrectUniversity, CorrectStudent, CorrectFaculty2, CorrectForm2)]
        [TestCase(CorrectYear, IncorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm1)]
        [TestCase(CorrectYear, IncorrectUniversity, CorrectStudent, CorrectFaculty3, CorrectForm2)]
        public void Check_ShouldReturnErrorMessage_IfUniversityIsIncorrect(string year, string university, string student, string faculty, string form)
        {
            var fileName = $"{year}{university}{student}{faculty}{form}";
            var s = new StudentFile(int.Parse(year),university,student,faculty,form);

            var result = s.CheckStudentFile();

            Debug.WriteLine(result);
            Assert.IsNotNull(s);
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.StartsWith("(1)") || result.StartsWith("(2)"));
        }
        [Test]
        [TestCase(CorrectYear, CorrectUniversity, IncorrectStudent, CorrectFaculty1, CorrectForm1)]
        [TestCase(CorrectYear, CorrectUniversity, IncorrectStudent, CorrectFaculty1, CorrectForm2)]
        [TestCase(CorrectYear, CorrectUniversity, IncorrectStudent, CorrectFaculty2, CorrectForm1)]
        [TestCase(CorrectYear, CorrectUniversity, IncorrectStudent, CorrectFaculty2, CorrectForm2)]
        [TestCase(CorrectYear, CorrectUniversity, IncorrectStudent, CorrectFaculty3, CorrectForm1)]
        [TestCase(CorrectYear, CorrectUniversity, IncorrectStudent, CorrectFaculty3, CorrectForm2)]
        public void Check_ShouldReturnErrorMessage_IfStudentIsIncorrect(string year, string university, string student, string faculty, string form)
        {
            var fileName = $"{year}{university}{student}{faculty}{form}";
            var s = StudentFile.Parse(fileName);

            var result = s.CheckStudentFile();

            Assert.IsNotNull(s);
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.StartsWith("(3)") || result.StartsWith("(4)"));
        }
        [Test]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, IncorrectFaculty, CorrectForm1)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, IncorrectFaculty, CorrectForm2)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, IncorrectFaculty, CorrectForm1)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, IncorrectFaculty, CorrectForm2)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, IncorrectFaculty, CorrectForm1)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, IncorrectFaculty, CorrectForm2)]
        public void Check_ShouldReturnErrorMessage_IfFacultyIsIncorrect(string year, string university, string student, string faculty, string form)
        {
            var fileName = $"{year}{university}{student}{faculty}{form}";
            var s = StudentFile.Parse(fileName);

            var result = s.CheckStudentFile();

            Assert.IsNotNull(s);
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.StartsWith("(5)") || result.StartsWith("(6)") || result.StartsWith("(7)"));
        }
        [Test]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty1, IncorrectForm)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty1, IncorrectForm)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty2, IncorrectForm)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty2, IncorrectForm)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty3, IncorrectForm)]
        [TestCase(CorrectYear, CorrectUniversity, CorrectStudent, CorrectFaculty3, IncorrectForm)]
        public void Check_ShouldReturnErrorMessage_IfFormIsIncorrect(string year, string university, string student, string faculty, string form)
        {
            var fileName = $"{year}{university}{student}{faculty}{form}";
            var s = StudentFile.Parse(fileName);

            var result = s.CheckStudentFile();

            Assert.IsNotNull(s);
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.StartsWith("(8)") || result.StartsWith("(9)") || result.StartsWith("(10)"));
        }
    }
}
