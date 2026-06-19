using System;
using System.Data;
using System.Data.SqlClient;

namespace CRUDMahasiswaADO
{
    public class DAL
    {
        private readonly string connectionString = "Data Source=SAGAB\\BAGASSSSSSSSSSS;Initial Catalog=DBAkademikADO;Integrated Security=True";

        public string GetConnectionString()
        {
            return connectionString;
        }

        // ===================== LOG =====================
        public void SimpanLog(string pesan)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO LogError VALUES(GETDATE(), @Pesan)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Pesan", pesan);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ===================== GET DATA =====================
        public DataTable GetMahasiswa()
        {
            DataTable dtMahasiswa = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtMahasiswa);
                    }
                }
            }

            return dtMahasiswa;
        }

        // ===================== HITUNG TOTAL =====================
        public int HitungTotal()
        {
            int total = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    total = Convert.ToInt32(outputParam.Value);
                }
            }

            return total;
        }

        // ===================== INSERT =====================
        public void InsertMahasiswa(string nim, string nama, string jenisKelamin, DateTime tanggalLahir,
            string alamat, string kodeProdi, DateTime tanggalDaftar, byte[] foto)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NIM", nim);
                    cmd.Parameters.AddWithValue("@Nama", nama);
                    cmd.Parameters.AddWithValue("@JenisKelamin", jenisKelamin);
                    cmd.Parameters.AddWithValue("@TanggalLahir", tanggalLahir);
                    cmd.Parameters.AddWithValue("@Alamat", alamat);
                    cmd.Parameters.AddWithValue("@KodeProdi", kodeProdi);
                    cmd.Parameters.AddWithValue("@TanggalDaftar", tanggalDaftar);
                    cmd.Parameters.AddWithValue("@Foto", foto ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();

                    SqlCommand cmdLog = new SqlCommand(
                        @"INSERT INTO LogAktivitasSalah (aktivitas, waktu) VALUES (@aktivitas, GETDATE())",
                        conn, trans);
                    cmdLog.Parameters.AddWithValue("@aktivitas", "INSERT MAHASISWA : " + nim);
                    cmdLog.ExecuteNonQuery();

                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        // ===================== UPDATE =====================
        public void UpdateMahasiswa(string nim, string nama, string jenisKelamin, DateTime tanggalLahir,
            string alamat, string kodeProdi, byte[] foto)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NIM", nim);
                    cmd.Parameters.AddWithValue("@Nama", nama);
                    cmd.Parameters.AddWithValue("@JenisKelamin", jenisKelamin);
                    cmd.Parameters.AddWithValue("@TanggalLahir", tanggalLahir);
                    cmd.Parameters.AddWithValue("@Alamat", alamat);
                    cmd.Parameters.AddWithValue("@KodeProdi", kodeProdi);
                    cmd.Parameters.AddWithValue("@Foto", foto ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ===================== DELETE =====================
        public int DeleteMahasiswa(string nim)
        {
            int rowsAffected;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIM", SqlDbType.Char, 11).Value = nim;

                    conn.Open();
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        // ===================== RESET DATA =====================
        public void ResetData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    IF OBJECT_ID('dbo.Mahasiswa_Backup') IS NOT NULL
                    BEGIN
                        DELETE FROM dbo.Mahasiswa;
                        INSERT INTO dbo.Mahasiswa
                        SELECT * FROM dbo.Mahasiswa_Backup;
                    END";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ===================== TEST INJECTION (DEMO TIDAK AMAN) =====================
        public void TestInject(string nim, string nama)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query =
                    "UPDATE Mahasiswa SET Nama='" + nama +
                    "' WHERE NIM='" + nim + "'";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ===================== DASHBOARD / CHART =====================
        public DataTable GetAllDataChart()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DashBoard", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }

        public DataTable GetDataChartByTahun(DateTime thMasuk)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DashBoardByTahun", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@inTglMsuk", thMasuk.Year.ToString());

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }
    }
}