using Stock_Management.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stock_Management
{
    public partial class Form1 : Form
    {

        string conStr = ConfigurationManager.ConnectionStrings["con"].ConnectionString;
        int intProductId = 0;
        string strPreviousImage = "";
        bool defaultImage = true;
        OpenFileDialog ofd = new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadCategoryCmb();
            LoadProductList();
            Clear();
        }

        private void LoadProductList()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("ViewAllProducts", con);
                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dt.Columns.Add("Image", Type.GetType("System.Byte[]"));
                foreach (DataRow dr in dt.Rows)
                {
                    //dr["Image"] = File.ReadAllBytes(Application.StartupPath + "\\images\\" + dr["ImagePath"].ToString());
                }
                dgvProductList.RowTemplate.Height = 80;
                dgvProductList.DataSource = dt;

                ((DataGridViewImageColumn)dgvProductList.Columns[dgvProductList.Columns.Count - 1]).ImageLayout = DataGridViewImageCellLayout.Stretch;

                sda.Dispose();
            }
        }

        private void LoadCategoryCmb()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM Category", con);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                DataRow topRow = dt.NewRow();
                topRow[0] = 0;
                topRow[1] = "-----Select-----";
                dt.Rows.InsertAt(topRow, 0);
                cmbCategory.ValueMember = "CategoryID";
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.DataSource = dt;
            }
        }

        private void Clear()
        {
            txtProCode.Text = "";
            txtProName.Text = "";
            cmbCategory.SelectedIndex = 0;
            dateIssue.Value = DateTime.Now;
            rbtnTechNova.Checked = true;
            chkStatus.Checked = true;
            intProductId = 0;
            btnDelete.Enabled = false;
            btnSave.Text = "Save";
            pictureBoxProduct.Image = Image.FromFile(Application.StartupPath + "\\images\\noimage2.png");
            defaultImage = true;
            if (dgvStockValue.DataSource == null)
            {
                dgvStockValue.Rows.Clear();
            }
            else
            {
                dgvStockValue.DataSource = (dgvStockValue.DataSource as DataTable).Clone();
            }
        }
        private void btnReset_Click_1(object sender, EventArgs e)
        {
            Clear();
        }

        private void btnBrowse_Click_1(object sender, EventArgs e)
        {
            ofd.Filter = "Images(.jpg,.png,.png)|*.png;*.jpg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBoxProduct.Image = new Bitmap(ofd.FileName);
                if (intProductId == 0)
                {
                    defaultImage = false;
                    strPreviousImage = "";
                }

            }
        }
        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            pictureBoxProduct.Image = new Bitmap(Application.StartupPath + "\\images\\noimage2.png");
            defaultImage = true;
            strPreviousImage = "";
        }

        bool ValidateMasterDetailForm()
        {
            bool isValid = true;
            if (txtProName.Text.Trim() == "")
            {
                MessageBox.Show("Product name is required");
                isValid = false;
            }
            return isValid;
        }
        string SaveImage(string imgPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(imgPath);
            string ext = Path.GetExtension(imgPath);
            fileName = fileName.Length <= 15 ? fileName : fileName.Substring(0, 15);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + ext;
            pictureBoxProduct.Image.Save(Application.StartupPath + "\\images\\" + fileName);
            return fileName;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateMasterDetailForm())
            {
                int ProductId = 0;
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("ProductAddOrEdit", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", intProductId);
                    cmd.Parameters.AddWithValue("@ProductCode", txtProCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@ProductName", txtProName.Text.Trim());
                    cmd.Parameters.AddWithValue("@CategoryID", Convert.ToInt16(cmbCategory.SelectedValue));
                    cmd.Parameters.AddWithValue("@IssueDate", dateIssue.Value);
                    cmd.Parameters.AddWithValue("@Status", chkStatus.Checked ? "True" : "False");
                    cmd.Parameters.AddWithValue("@Supplier", rbtnNexTech.Checked ? "NexTech" : "TechNova");
                    cmd.Parameters.AddWithValue("@Quantity", intProductId);
                    cmd.Parameters.AddWithValue("@Price", intProductId);
                    if (defaultImage)
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                    }

                    else if (intProductId > 0 && strPreviousImage != "")
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", strPreviousImage);
                        if (ofd.FileName != strPreviousImage)
                        {
                            var filename = Application.StartupPath + "\\images\\" + strPreviousImage;
                            if (pictureBoxProduct.Image != null)
                            {
                                pictureBoxProduct.Image.Dispose();
                                pictureBoxProduct.Image = null;
                                File.Delete(filename);
                            }
                        }

                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    }
                    ProductId = Convert.ToInt16(cmd.ExecuteScalar());
                }
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    foreach (DataGridViewRow item in dgvStockValue.Rows)
                    {
                        if (item.IsNewRow) break;
                        else
                        {
                            SqlCommand cmd = new SqlCommand("ProductStockValueAddAndEdit", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@StockValueID", Convert.ToInt32(item.Cells["dgvStockValueID"].Value == DBNull.Value ? "0" : item.Cells["dgvStockValueID"].Value));
                            cmd.Parameters.AddWithValue("@ProductId", ProductId);
                            cmd.Parameters.AddWithValue("@Quantity", item.Cells["dgvQuantity"].Value);
                            cmd.Parameters.AddWithValue("@Price", item.Cells["dgvPrice"].Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                LoadProductList();
                Clear();
                MessageBox.Show(" Data Submitted Successfully");
            }
        }


        private void dgvProductList_DoubleClick(object sender, EventArgs e)
        {
            if (dgvProductList.CurrentRow.Index != -1)
            {
                DataGridViewRow dgvRow = dgvProductList.CurrentRow;
                intProductId = Convert.ToInt32(dgvRow.Cells[0].Value);
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("ViewProductByProductId", con);
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddWithValue("@ProductId", intProductId);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    //--Master---
                    DataRow dr = ds.Tables[0].Rows[0];
                    txtProCode.Text = dr["ProductCode"].ToString();
                    txtProName.Text = dr["ProductName"].ToString();
                    cmbCategory.SelectedValue = Convert.ToInt32(dr["CategoryID"].ToString());
                    dateIssue.Value = Convert.ToDateTime(dr["IssueDate"].ToString());
                    if (Convert.ToBoolean(dr["Status"].ToString()))
                    {
                        chkStatus.Checked = true;
                    }
                    else
                    {
                        chkStatus.Checked = false;
                    }
                    if ((dr["Supplier"].ToString().Trim()) == "TechNova")
                    {
                        rbtnTechNova.Checked = true;
                    }
                    else
                    {
                        rbtnTechNova.Checked = false;
                    }
                    if ((dr["Supplier"].ToString().Trim()) == "NexTech")
                    {
                        rbtnNexTech.Checked = true;
                    }
                    else
                    {
                        rbtnNexTech.Checked = false;
                    }
                    if (dr["ImagePath"] == DBNull.Value)
                    {
                        pictureBoxProduct.Image = new Bitmap(Application.StartupPath + "\\images\\noimage2.png");
                    }
                    else
                    {
                        string image = dr["ImagePath"].ToString();
                        //pictureBoxProduct.Image = new Bitmap(Application.StartupPath + "\\images\\" + dr["ImagePath"].ToString());
                        strPreviousImage = dr["ImagePath"].ToString();
                        defaultImage = false;
                    }
                    //Details//
                    dgvStockValue.AutoGenerateColumns = false;
                    dgvStockValue.DataSource = ds.Tables[1];
                    btnDelete.Enabled = true;
                    btnSave.Text = "Update";
                    tabControl1.SelectedIndex = 0;
                }
            }
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete this record?", "Master Details", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string image = "";
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("ViewProductByProductId", con);
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddWithValue("@ProductId", intProductId);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    DataRow dr = ds.Tables[0].Rows[0];
                    if (dr["ImagePath"] != DBNull.Value)
                    {
                        image = dr["ImagePath"].ToString();
                        var filename = Application.StartupPath + "\\images\\" + image;
                        if (pictureBoxProduct.Image != null)
                        {
                            pictureBoxProduct.Image.Dispose();
                            pictureBoxProduct.Image = null;
                            System.IO.File.Delete(filename);
                        }

                    }
                    SqlCommand cmd = new SqlCommand("ProductStockValueDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", intProductId);
                    sda.Dispose();
                    cmd.ExecuteNonQuery();
                    LoadProductList();
                    Clear();
                    MessageBox.Show("Data Deleted Successfully");
                }

            }
        }

        private void dgvStockValue_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dgvRow = dgvStockValue.CurrentRow;
            if (dgvRow.Cells["dgvStockValueID"].Value != DBNull.Value)
            {
                if (MessageBox.Show("Are you sure to delete this record?", "Master Details", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection con = new SqlConnection(conStr))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("StockValueDelete", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@StockValueID", dgvRow.Cells["dgvStockValueID"].Value);
                        cmd.ExecuteNonQuery();
                    }

                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
        private void btnReport_Click_1(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("ViewAllProducts", con);
                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();
                sda.Fill(dt);
                List<ProductViewModel> list = new List<ProductViewModel>();
                ProductViewModel productVm;
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        productVm = new ProductViewModel();
                        productVm.ProductId = Convert.ToInt32(dt.Rows[i]["ProductId"]);
                        productVm.ProductCode = dt.Rows[i]["ProductCode"].ToString();
                        productVm.ProductName = dt.Rows[i]["ProductName"].ToString();
                        productVm.IssueDate = Convert.ToDateTime(dt.Rows[i]["IssueDate"].ToString());
                        productVm.Supplier = dt.Rows[i]["Supplier"].ToString();
                        productVm.Status = Convert.ToBoolean(dt.Rows[i]["Status"].ToString());
                        productVm.Price = Convert.ToInt32(dt.Rows[i]["Price"]);
                        productVm.Quantity = Convert.ToInt32(dt.Rows[i]["Quantity"]);
                        productVm.CategoryName = dt.Rows[i]["CategoryName"].ToString();
                        productVm.ImagePath = Application.StartupPath + "\\images\\" + dt.Rows[i]["ImagePath"].ToString();
                        list.Add(productVm);

                    }
                    using (ProductReport report = new ProductReport(list))
                    {
                        report.ShowDialog();
                    }
                }


            }
        }
        
    }
}
