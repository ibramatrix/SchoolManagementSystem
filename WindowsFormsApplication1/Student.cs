using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class Student
    {
        public string Name { get; set; } = "";
        public string FatherName { get; set; } = "";
        public string Address { get; set; } = "";
        public string Contact { get; set; } = "";
        public string Fee { get; set; } = "";
        public string AdmissionDate { get; set; } = "";

        public string fcmToken { get; set; } = "";  // <- Add this

        public Dictionary<string, bool> FeeStatus { get; set; } = new Dictionary<string, bool>
    {
        { "January", false },
        { "February", false },
        { "March", false },
        { "April", false },
        { "May", false },
        { "June", false },
        { "July", false }
    };
    }



}
