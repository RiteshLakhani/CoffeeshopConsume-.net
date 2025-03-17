using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;

namespace NiceAdmin.Controllers
{
    public class OrderController : Controller
    {
        private IConfiguration configuration;

        public OrderController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        #region OrderList
        public IActionResult Order()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = "PR_Order_SelectAll";
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(sqlDataReader);

            return View(dataTable);
        }
        #endregion

        #region OrderDelete
        public IActionResult OrderDelete(int OrderID)
        {
            try
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_Order_DeleteByPK";
                sqlCommand.Parameters.Add("@OrderID", SqlDbType.Int).Value = OrderID;
                sqlCommand.ExecuteNonQuery();
                return RedirectToAction("Order");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString);
                return RedirectToAction("Order");
            }
        }
        #endregion

        #region OrderSave
        public IActionResult OrderSave(OrderModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if (model.OrderID == 0)
                {
                    command.CommandText = "PR_Order_Insert";
                }
                else
                {
                    command.CommandText = "PR_Order_UpdateByPK";
                    command.Parameters.Add("@OrderID", SqlDbType.Int).Value = model.OrderID;
                }
                command.Parameters.Add("@OrderCode", SqlDbType.VarChar).Value = model.OrderCode;
                command.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = model.OrderDate;
                command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = model.CustomerID;
                command.Parameters.Add("@PaymentMode", SqlDbType.VarChar).Value = model.PaymentMode;
                command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = model.TotalAmount;
                command.Parameters.Add("@ShippingAddress", SqlDbType.VarChar).Value = model.ShippingAddress;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                command.ExecuteNonQuery();
                TempData["SuccessMessage"] = model.OrderID == 0 ? "Order added successfully!" : "Order updated successfully!";
                return RedirectToAction("Order");
            }

            return View("OrderForm", model);
        }
        #endregion

        #region OrderForm/DropDown
        public IActionResult OrderForm(int? OrderID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            #region CustomerDropdown
            SqlConnection sqlConnection1 = new SqlConnection(connectionString);
            sqlConnection1.Open();
            SqlCommand command1 = sqlConnection1.CreateCommand();
            command1.CommandType = System.Data.CommandType.StoredProcedure;
            command1.CommandText = "PR_Customer_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            List<CustomerDropDownModel> customerList = new List<CustomerDropDownModel>();
            foreach(DataRow data in dataTable1.Rows)
            {
                CustomerDropDownModel customerDropDownModel = new CustomerDropDownModel();
                customerDropDownModel.CustomerID = Convert.ToInt32(data["CustomerID"]);
                customerDropDownModel.CustomerName = data["CustomerName"].ToString();
                customerList.Add(customerDropDownModel);
            }
            ViewBag.Customers = customerList;
            #endregion

            #region UserDropdown

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


            OrderModel orderModel = new OrderModel();
            if (OrderID.HasValue)
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Order_SelectByPK";
                command.Parameters.AddWithValue("@OrderID", OrderID);
                SqlDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);


                foreach (DataRow dataRow in table.Rows)
                {
                    orderModel.OrderID = Convert.ToInt32(@dataRow["OrderID"]);
                    orderModel.OrderCode = @dataRow["OrderCode"].ToString();
                    orderModel.OrderDate = Convert.ToDateTime(@dataRow["OrderDate"]);
                    orderModel.CustomerID = Convert.ToInt32(@dataRow["CustomerID"]);
                    orderModel.PaymentMode = @dataRow["PaymentMode"].ToString();
                    orderModel.TotalAmount = Convert.ToDecimal(@dataRow["TotalAmount"]);
                    orderModel.ShippingAddress = @dataRow["ShippingAddress"].ToString();
                    orderModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
                }

                ViewData["Title"] = "Edit Order";
            }
            else
            {
                ViewData["Title"] = "Add Order";
            }
            return View("OrderForm", orderModel);
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
                    sqlCommand.CommandText = "PR_Order_SelectAll";
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

                            var fileName = "Orders.xlsx";
                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                    }
                }
            }
        }
        #endregion 
    }
}
