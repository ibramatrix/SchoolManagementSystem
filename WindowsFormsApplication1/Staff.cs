using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class Staff
    {
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Phone { get; set; }
        public string Salary { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Manages { get; set; }  // this is the class assigned (e.g., ONE, NURSERY)
        public bool isActive { get; set; }
    }
}
