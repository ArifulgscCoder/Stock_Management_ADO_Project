using Stock_Management.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stock_Management
{
    public partial class ProductReport : Form
    {
        List<ProductViewModel> _list;
        public ProductReport(List<ProductViewModel> list)
        {
            InitializeComponent();
            _list = list;
        }

        private void ProductReport_Load(object sender, EventArgs e)
        {
            rptProductInfo rpt = new rptProductInfo();
            rpt.SetDataSource(_list);
            crystalReportViewer1.ReportSource = rpt;
            crystalReportViewer1.Refresh();
        }

       
    }
}
