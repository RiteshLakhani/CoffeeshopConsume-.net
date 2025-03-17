using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;

namespace NiceAdmin.Controllers
{
    public class ProductController : Controller
    {

        private IConfiguration configuration;

        public ProductController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        #region ProductList
        public IActionResult Product()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = "PR_Product_SelectAll";
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(sqlDataReader);

            return View(dataTable);
        }
        #endregion

        #region ProductDelete
        public IActionResult ProductDelete(int ProductID)
        {
            try
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_Product_DeleteByPK";
                sqlCommand.Parameters.Add("@ProductID", SqlDbType.Int).Value = ProductID;
                sqlCommand.ExecuteNonQuery();
                return RedirectToAction("Product");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString);
                return RedirectToAction("Product");
            }
        }
        #endregion

        #region ProductSave
        public IActionResult ProductSave(ProdcutModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if(model.ProductID == 0)
                {
                    command.CommandText = "PR_Product_Insert";
                }
                else
                {
                    command.CommandText = "PR_Product_UpdateByPK";
                    command.Parameters.Add("@ProductID", SqlDbType.Int).Value = model.ProductID;
                }
                command.Parameters.Add("@ProductName", SqlDbType.VarChar).Value = model.ProductName;
                command.Parameters.Add("@ProductCode", SqlDbType.VarChar).Value = model.ProductCode;
                command.Parameters.Add("@ProductPrice", SqlDbType.Decimal).Value = model.ProductPrice;
                command.Parameters.Add("@Description", SqlDbType.VarChar).Value = model.Description;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                command.ExecuteNonQuery();
                TempData["SuccessMessage"] = model.ProductID == 0 ? "Product added successfully!" : "Product updated successfully!";
                return RedirectToAction("Product");
            }
            return View("ProductForm", model);
        }
        #endregion

        #region ProductForm/Dropdown
        public IActionResult ProductForm(int? ProductID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            #region User Drop-Down

            SqlConnection sqlConnection1 = new SqlConnection(connectionString);
            sqlConnection1.Open();
            SqlCommand command1 = sqlConnection1.CreateCommand();
            command1.CommandType = System.Data.CommandType.StoredProcedure;
            command1.CommandText = "PR_User_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            sqlConnection1.Close();

            List<UserDropDownModel> userList = new List<UserDropDownModel>();
            foreach (DataRow data in dataTable1.Rows)
            {
                UserDropDownModel userDropDownModel = new UserDropDownModel();
                userDropDownModel.UserID = Convert.ToInt32(data["UserID"]);
                userDropDownModel.UserName = data["UserName"].ToString(); 
                userList.Add(userDropDownModel);
            }

            ViewBag.Users = userList;

            #endregion

            #region ProductByID
            ProdcutModel productModel = new ProdcutModel();
            if (ProductID.HasValue)
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Product_SelectByPK";
                command.Parameters.AddWithValue("@ProductID", ProductID);
                SqlDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);


                foreach (DataRow dataRow in table.Rows)
                {
                    productModel.ProductID = Convert.ToInt32(@dataRow["ProductID"]);
                    productModel.ProductName = @dataRow["ProductName"].ToString();
                    productModel.ProductCode = @dataRow["ProductCode"].ToString();
                    productModel.ProductPrice = Convert.ToDouble(@dataRow["ProductPrice"]);
                    productModel.Description = @dataRow["Description"].ToString();
                    productModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
                }
                ViewData["Title"] = "Edit Product";
            }
            else
            {
                ViewData["Title"] = "Add Product";
            }
                #endregion

            return View("ProductForm",productModel);
        }
        #endregion

        #region ExcelCode
        public IActionResult ExportToExcel()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Product_SelectAll";
                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(sqlDataReader);

                        using (var package = new ExcelPackage())
                        {
                            var worksheet = package.Workbook.Worksheets.Add("Users");
                            worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                            var stream = new MemoryStream();
                            package.SaveAs(stream);
                            stream.Position = 0;

                            var fileName = "Products.xlsx";
                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                    }
                }
            }
        }
        #endregion 
    }
}
