using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Management.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public DateTime IssueDate { get; set; }
        public string Supplier { get; set; }
        public bool Status { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public string CategoryName{ get; set; }
        public string ImagePath { get; set; }
    }
}
