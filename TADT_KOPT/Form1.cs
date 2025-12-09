using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TADT_KOPT
{
    public partial class Form1 : Form
    {
        int editOk = 0;
        private readonly string connectionString = Properties.Settings.Default.connString;
        private int facultyID = 0;

        public Form1()
        {
            InitializeComponent();
            LoadFacultyGrid();
            ClearForm();
        }

        void IDExists(ref int editOk)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.connString))
            {
                // Abre la conexion:
                connection.Open();

                SqlDataAdapter SqlDa = new SqlDataAdapter("SELECT * FROM Faculty WHERE FacultyID='" + txtFacultyID.Text + "'", connection);  //SqlDa=tabla con ID indicado.

                DataTable dt = new DataTable();//dt= variable de una tabla.

                SqlDa.Fill(dt);//llena la variable dt con la tabla de SqlDa.

                int i = dt.Rows.Count;  //cuenta el número de filas de dt.
                                        //Si i=0, es un registro nuevo. Si i>0, encontramos un registro existente con ese id.
                if (i > 0)
                {
                    editOk = 1;
                    MessageBox.Show("FacultyID already exists. The record will be updated.");
                    dt.Clear();
                }
                else
                {
                    editOk = 0;
                  
                }
                connection.Close();
            }
        }
        private void ClearForm()
        {
            txtFacultyID.Text = "0";
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtOfficePhone.Clear();
            txtDeptCode.Clear();
            txtSearchLastName.Clear();

            btnSave.Text = "Save";
            btnDelete.Enabled = false;
        }

        
        private void LoadFacultyGrid(string lastNameFilter = "")
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = sqlCon;

                    if (string.IsNullOrWhiteSpace(lastNameFilter))
                    {
                        cmd.CommandText = "FacultyViewAll";
                        cmd.CommandType = CommandType.StoredProcedure;
                    }
                    else
                    {
                        cmd.CommandText = "FacultySearchByLastName";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LastName", lastNameFilter);
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvFaculty.DataSource = dt;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // VALIDACIÓN DE CAMPOS VACÍOS
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtOfficePhone.Text) ||
                string.IsNullOrWhiteSpace(txtDeptCode.Text))
            {
                MessageBox.Show("All fields must be filled in before saving.",
                    "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // VALIDACIÓN DE EMAIL @hu.edu
            if (!txtEmail.Text.Trim().EndsWith("@hu.edu"))
            {
                MessageBox.Show("Email must end with @hu.edu",
                    "Invalid Email Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                IDExists(ref editOk);
                using (SqlCommand cmd = new SqlCommand("FacultyAddOrEdit", sqlCon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //// 🔹 AQUÍ DECIDIMOS SI ES INSERT (0) O UPDATE (1)
                    //int editOk;

                    //if (string.IsNullOrWhiteSpace(txtFacultyID.Text))
                    //{
                    //    // Nuevo registro → INSERT
                    //    editOk = 0;
                    //}
                    //else
                    //{
                    //    // Registro existente → UPDATE
                    //    editOk = 1;
                    //}

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            using (SqlCommand sqlCommand = new SqlCommand("FacultyAddOrEdit", connection))
                            {
                                connection.Open();


                                IDExists(ref editOk);

                                sqlCommand.CommandType = CommandType.StoredProcedure;

                                // Parámetros 

                                sqlCommand.Parameters.Add(new SqlParameter("@editok", SqlDbType.Int));
                                sqlCommand.Parameters["@editok"].Value = editOk;

                                sqlCommand.Parameters.Add(new SqlParameter("@FacultyID", SqlDbType.VarChar, 25));
                                sqlCommand.Parameters["@FacultyID"].Value = txtFacultyID.Text.Trim();

                                sqlCommand.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.VarChar, 25));
                                sqlCommand.Parameters["@FirstName"].Value = txtFirstName.Text.Trim();

                                sqlCommand.Parameters.Add(new SqlParameter("@LastName", SqlDbType.VarChar, 25));
                                sqlCommand.Parameters["@LastName"].Value = txtLastName.Text.Trim();

                                sqlCommand.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar, 100));
                                sqlCommand.Parameters["@Email"].Value = txtEmail.Text.Trim();

                                sqlCommand.Parameters.Add(new SqlParameter("@OfficePhone", SqlDbType.VarChar, 20));
                                sqlCommand.Parameters["@OfficePhone"].Value = txtOfficePhone.Text.Trim();

                                sqlCommand.Parameters.Add(new SqlParameter("@DeptCode", SqlDbType.VarChar, 10));
                                sqlCommand.Parameters["@DeptCode"].Value = txtDeptCode.Text.Trim();

                                sqlCommand.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Record saved successfully.",
                            "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ClearForm();
                        LoadFacultyGrid();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(ex.Message, "Database Operation failed!");
                    }

                    ClearForm();
                    LoadFacultyGrid();
                }
            }
        }

        // DELETE
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtFacultyID.Text == "0" || string.IsNullOrWhiteSpace(txtFacultyID.Text))
            {
                MessageBox.Show("Select a record to delete first.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this record?",
                    "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();

                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM faculty WHERE FacultyID = @FacultyID", sqlCon))
                {
                    cmd.Parameters.AddWithValue("@FacultyID", txtFacultyID.Text.Trim());
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Record deleted.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            Clear();
            LoadFacultyGrid();
        }

        // CLEAR
        private void btnClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        // SEARCH
        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadFacultyGrid(txtSearchLastName.Text.Trim());

        }
        private void dgvFaculty_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvFaculty.Rows[e.RowIndex];

            txtFacultyID.Text = row.Cells["FacultyID"].Value.ToString();
            txtFirstName.Text = row.Cells["FirstName"].Value.ToString();
            txtLastName.Text = row.Cells["LastName"].Value.ToString();
            txtEmail.Text = row.Cells["Email"].Value.ToString();
            txtOfficePhone.Text = row.Cells["OfficePhone"].Value.ToString();
            txtDeptCode.Text = row.Cells["DeptCode"].Value.ToString();

            btnSave.Text = "Update";
            btnDelete.Enabled = true;
        }

        private void dgvFaculty_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void txtFacultyID_TextChanged(object sender, EventArgs e)
        {

        }

        void Clear()
        {
            txtFacultyID.Text = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtOfficePhone.Text = "";
            txtDeptCode.Text = "";

            btnSave.Text = "Save";
            btnDelete.Enabled = false;

            txtFacultyID.Focus();
        }


        private void dgvFaculty_DoubleClick(object sender, EventArgs e)
        {
            if (dgvFaculty.CurrentRow != null)
            {
                txtFacultyID.Text = dgvFaculty.CurrentRow.Cells[0].Value.ToString();
                txtFirstName.Text = dgvFaculty.CurrentRow.Cells[1].Value.ToString();
                txtLastName.Text = dgvFaculty.CurrentRow.Cells[2].Value.ToString();
                txtEmail.Text = dgvFaculty.CurrentRow.Cells[3].Value.ToString();
                txtOfficePhone.Text = dgvFaculty.CurrentRow.Cells[4].Value.ToString();
                txtDeptCode.Text = dgvFaculty.CurrentRow.Cells[5].Value.ToString();
            }
        }
    }
}
          