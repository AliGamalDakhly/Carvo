﻿using Carvo.Business_Logic_Layer.IServices;
using Carvo.Data_Access_Layer.Entities.Users;
using Carvo.Data_Access_Layer.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Data;


namespace Carvo.User_Interface_Layer
{
    public partial class AdminCustomersForm : Form
    {
        private readonly ICustomerService _customerService;
        private IServiceProvider _serviceProvider;

        public AdminCustomersForm(ICustomerService customerService, IServiceProvider serviceProvider)
        {
            _customerService = customerService;
            _serviceProvider = serviceProvider;
            InitializeComponent();
            this.Paint += Form1_Paint;
            LoadCustomersAsync();
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            int borderThickness = 4;
            Color borderColor = Color.LightGray;

            using (Pen pen = new Pen(borderColor, borderThickness))
            {
                e.Graphics.DrawRectangle(pen,
                    new Rectangle(0, 0, this.Width - borderThickness, this.Height - borderThickness));
            }
        }
        //  الداله الصح
        private async Task LoadCustomersAsync()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            dgvCustomers.AllowUserToAddRows = false;
            dgvCustomers.DataSource = customers.Select(c => new
            {
                c.Id,
                c.CreatedAt,
                c.RemainingBalance,
                c.PhoneNumber,
                c.Name,
            }).ToList();

            dgvCustomers.Columns["id"].Visible = false;
            dgvCustomers.Columns["CreatedAt"].Visible = false;
            dgvCustomers.Columns[dgvCustomers.ColumnCount - 1].HeaderText = "الاسم";
            dgvCustomers.Columns[dgvCustomers.ColumnCount - 2].HeaderText = "رقم الهاتف";
            dgvCustomers.Columns[dgvCustomers.ColumnCount - 3].HeaderText = "الرصيد المتبقي";
            //UsersGridView.Columns[5].HeaderText = "تاريخ الاضافة";
        }


        private void dgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow != null && dgvCustomers.CurrentRow.Index >= 0)
            {
                txtCustomerName.Text = dgvCustomers.CurrentRow.Cells["Name"].Value?.ToString();
                txtPhoneNumber.Text = dgvCustomers.CurrentRow.Cells["PhoneNumber"].Value?.ToString();
                //txtNationalId.Text = dgvCustomers.CurrentRow.Cells["NationalId"].Value?.ToString();
            }
        }


        private async void btnAddCustomer_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            var customer = new Customer
            {
                Name = txtCustomerName.Text,
                PhoneNumber = txtPhoneNumber.Text,
                //NationalId = txtNationalId.Text
            };

            AddAlertForm addAlert = _serviceProvider.GetRequiredService<AddAlertForm>();
            addAlert.ShowDialog();

            await _customerService.AddCustomerAsync(customer);
            LoadCustomersAsync();
            ClearInputs();
        }

        private async void btnUpdateCustomer_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null || !ValidateInputs()) return;

            int id = (int)dgvCustomers.CurrentRow.Cells["Id"].Value;

            var updatedCustomer = new Customer
            {
                Id = id,
                Name = txtCustomerName.Text,
                PhoneNumber = txtPhoneNumber.Text,
                //NationalId = txtNationalId.Text
            };

            await _customerService.UpdateCustomerAsync(updatedCustomer);

            UpdateAlertForm updateAlert = _serviceProvider.GetRequiredService<UpdateAlertForm>();
            updateAlert.ShowDialog();

            await LoadCustomersAsync();
            ClearInputs();
        }


        private async void btnDeleteCustomer_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null) return;

            int id = (int)dgvCustomers.CurrentRow.Cells["Id"].Value;

            var confirm = MessageBox.Show("هل أنت متأكد من حذف العميل؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
            try
            {
                if (confirm == DialogResult.Yes)
                {
                    await _customerService.DeleteCustomerAsync(id);

                    DeleteAlertForm deleteAlert = _serviceProvider.GetRequiredService<DeleteAlertForm>();
                    deleteAlert.ShowDialog();

                    await LoadCustomersAsync();
                    ClearInputs();
                }
            }
            catch
            {
                MessageBox.Show("لا يمكن مسح هذا العميل , الرجاء مسح الفواتير الخاصة به اولا");
            }

        }


        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("الرجاء إدخال اسم العميل.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
            {
                MessageBox.Show("الرجاء إدخال رقم الهاتف.");
                return false;
            }

            return true;
        }

        private void ClearInputs()
        {
            txtCustomerName.Clear();
            txtPhoneNumber.Clear();
        }


        private void CloseFormBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MinimizeFormBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void AddVehicleBtn_Click(object sender, EventArgs e)
        {
            VehicleDashboardForm vehicleDashboardForm = _serviceProvider.GetRequiredService<VehicleDashboardForm>();
            vehicleDashboardForm.Show();
            this.Close();
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            LoggedUser.loggedUserId = 0;
            LoggedUser.loggedUserName = "";
            LoggedUser.mainWindowForm.Show();
            this.Close();
        }

        private void PrevImageAsBtn_Click(object sender, EventArgs e)
        {
            if (LoggedUser.Role == Role.Admin)
            {
                HomeDashboardForm homeDashboardForm = _serviceProvider.GetRequiredService<HomeDashboardForm>();
                homeDashboardForm.Show();
            }
            else
            {
                EmployeeDashboardForm employeeDashboardForm = _serviceProvider.GetRequiredService<EmployeeDashboardForm>();
                employeeDashboardForm.Show();
            }
            this.Close();
        }



        private void cancelBtn_Click_1(object sender, EventArgs e)
        {
            txtCustomerName.Text = string.Empty;
            txtPhoneNumber.Text = string.Empty;
        }
    }
}
