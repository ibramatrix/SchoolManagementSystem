using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class StudentRow
    {
        public string StudentID { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Address { get; set; }
        public int ObtainedMarks { get; set; }
    }

    public class TestStudentRecord
    {
        public string Name { get; set; }
        public int ObtainedMarks { get; set; }
    }

    public class TestReport
    {
        public string TestName { get; set; }
        public int TotalMarks { get; set; }
        public string ReportType { get; set; }
        public string Subject { get; set; }
        public Dictionary<string, TestStudentRecord> Students { get; set; }
    }

    public class AllTestReports : Dictionary<string, TestReport> { }

}
