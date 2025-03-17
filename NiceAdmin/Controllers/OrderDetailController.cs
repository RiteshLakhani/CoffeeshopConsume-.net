using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;

namespace NiceAdmin.Controllers
{
    public class OrderDetailController : Controller
    {
        private IConfiguration configuration;

        public OrderDetailController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        #region OrderDetailList
        public IActionResult OrderDetail()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = "PR_OrderDetail_SelectAll";
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(sqlDataReader);

            return View(dataTable);
        }
        #endregion

        #region OrderDetailDelete
        public IActionResult OrderDetailDelete(int OrderDetailID)
        {
            try
            {
                string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection sqlConnection = new SqlConnection(ConnectionString);
                sqlConnection.Open();   
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_OrderDetail_DeleteByPK";
                sqlCommand.Parameters.Add("@OrderDetailID",SqlDbType.Int).Value = OrderDetailID;
                sqlCommand.ExecuteNonQuery();
                return RedirectToAction("OrderDetail");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString);
                return RedirectToAction("OrderDetail");
            }
        }
        #endregion

        #region OrderDetailSave
        public IActionResult OrderDetailSave(OrderDetailModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if (model.OrderDetailID == 0)
                {
                    command.CommandText = "PR_OrderDetail_Insert";
                }
                else
                {
                    command.CommandText = "PR_OrderDetail_UpdateByPK";
                    command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = model.OrderDetailID;
                }
                command.Parameters.Add("@OrderID", SqlDbType.Int).Value = model.OrderID;
                command.Parameters.Add("@ProductID", SqlDbType.VarChar).Value = model.ProductID;
                command.Parameters.Add("@Quantity", SqlDbType.Int).Value = model.Quantity;
                command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = model.Amount;
                command.Parameters.Add("TotalAmount", SqlDbType.Decimal).Value = model.TotalAmount;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                command.ExecuteNonQuery();
                TempData["SuccessMessage"] = model.OrderDetailID == 0 ? "OrderDetail added successfully!" : "OrderDetail updated successfully!";
                return RedirectToAction("OrderDetail");
            }

            return View("OrderDetailForm", model);
        }
        #endregion

        #region OrderDetailForm/Dropdown
        public IActionResult OrderDetailForm(int? OrderDetailID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            #region ProductDropdown
            SqlConnection sqlConnection1 = new SqlConnection(connectionString);
            sqlConnection1.Open();
            SqlCommand command1 = sqlConnection1.CreateCommand();
            command1.CommandType = System.Data.CommandType.StoredProcedure;
            command1.CommandText = "PR_Product_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            List<ProductDropDownModel> productList = new List<ProductDropDownModel>();
            foreach (DataRow data in dataTable1.Rows)
            {
                ProductDropDownModel productDropDownModel = new ProductDropDownModel();
                productDropDownModel.ProductID = Convert.ToInt32(data["ProductID"]);
                productDropDownModel.ProductName = data["ProductName"].ToString();
                productList.Add(productDropDownModel);
            }
            ViewBag.Products = productList;
            #endregion

            #region UserDropDown
            SqlConnection sqlConnection2 = new SqlConnection(connectionString);
            sqlConnection2.Open();
            SqlCommand command2 = sqlConnection1.CreateCommand();
            command2.CommandType = System.Data.CommandType.StoredProcedure;
            command2.CommandText = "PR_User_DropDown";
            SqlDataReader reader2 = command2.ExecuteReader();
            DataTable dataTable2 = new DataTable();
            dataTable2.Load(reader2);
            List<UserDropDownModel> userList = new List<UserDropDownModel>();
            foreach (DataRow data in dataTable2.Rows)
            {
                UserDropDownModel userDropDownModel = new UserDropDownModel();
                userDropDownModel.UserID = Convert.ToInt32(data["UserID"]);
                userDropDownModel.UserName = data["UserName"].ToString();
                userList.Add(userDropDownModel);
            }

            ViewBag.Users = userList;
            #endregion

            #region OrderDropDown
            SqlConnection sqlConnection3 = new SqlConnection(connectionString);
            sqlConnection3.Open();
            SqlCommand command3 = sqlConnection1.CreateCommand();
            command3.CommandType = System.Data.CommandType.StoredProcedure;
            command3.CommandText = "PR_Orders_DropDown";
            SqlDataReader reader3 = command3.ExecuteReader();
            DataTable dataTable3 = new DataTable();
            dataTable3.Load(reader3);
            List<OrderDropDownModel> orderList = new List<OrderDropDownModel>();
            foreach (DataRow data in dataTable3.Rows)
            {
                OrderDropDownModel orderDropDownModel = new OrderDropDownModel();
                orderDropDownModel.OrderID = Convert.ToInt32(data["OrderID"]);
                orderDropDownModel.OrderCode = data["OrderCode"].ToString();
                orderList.Add(orderDropDownModel);
            }

            ViewBag.Orders = orderList;
            #endregion

            OrderDetailModel orderDetailModel = new OrderDetailModel();
            if (OrderDetailID.HasValue)
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_OrderDetail_SelectByPK";
                command.Parameters.AddWithValue("@OrderDetailID", OrderDetailID);
                SqlDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);

               foreach (DataRow dataRow in table.Rows)
               {
                   orderDetailModel.OrderDetailID = Convert.ToInt32(dataRow["OrderDetailID"]);
                   orderDetailModel.OrderID = Convert.ToInt32(@dataRow["OrderID"]);
                   orderDetailModel.ProductID = @dataRow["ProductID"].ToString();
                   orderDetailModel.Quantity = Convert.ToInt32(@dataRow["Quantity"]);
                   orderDetailModel.Amount = Convert.ToDecimal(@dataRow["Amount"]);
                   orderDetailModel.TotalAmount = Convert.ToDecimal(@dataRow["TotalAmount"]);
                   orderDetailModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
               }

                ViewData["Title"] = "Edit OrderDetail";
            }
            else
            {
                ViewData["Title"] = "Add OrderDetail";
            }
            return View("OrderDetailForm", orderDetailModel);
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
                    sqlCommand.CommandText = "PR_OrderDetail_SelectAll";
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

                            var fileName = "Order-Details.xlsx";
                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
