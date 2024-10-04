using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIA.Models
{
    public class UserInfoApi
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string EmployeeID { get; set; }
        public int AgencyCode { get; set; }
        public string AgencyName { get; set; }
        public string UserName { get; set; }
        public string JobPositionName { get; set; }
    }
}
