using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using ProxyCore;
using ProxyCore.Scripting;

namespace MySQL
{
    public class MySQL : Plugin
    {
        public MySQL()
            : base("mysql", "MySQL Database")
        {
            Author = "Duckbat";
            Version = 1;
            Description = "Provides easy access to MySQL database.";
            UpdateUrl = "www.duckbat.com/plugins/update.mysql.txt";
            Website = "code.google.com/p/proxymud/";

            Config = new MySQLConfig();
        }

        internal static long MSTime;
        private Database db;

        private void InitDB()
        {
            if(db == null)
                db = new Database(Config.GetString("MySQL.Host", "localhost"), Config.GetString("MySQL.User", "root"), Config.GetString("MySQL.Pass", "pass"), Config.GetInt32("MySQL.Port", 3306), Config.GetString("MySQL.Database", "aardwolf"));
        }

        public override void Shutdown()
        {
            base.Shutdown();

            if(db != null)
            {
                db.CloseAll();
                db = null;
            }
        }

        public override void Update(long msTime)
        {
            base.Update(msTime);

            MSTime = msTime;
            if(db != null)
                db.Update(msTime);
        }

        /// <summary>
        /// Query data from the database.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public QueryData Query(string q)
        {
            InitDB();
            return db.Query(q);
        }

        /// <summary>
        /// Execute a query in the database.
        /// </summary>
        /// <param name="q"></param>
        public void Execute(string q)
        {
            InitDB();
            db.Execute(q);
        }
    }

    public class MySQLConfig : ConfigFile
    {
        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("MySQL.Host", "localhost", "Hostname for MySQL connection.");
            CreateSetting("MySQL.Port", 3306, "Port for MySQL connection.");
            CreateSetting("MySQL.User", "root", "Username for MySQL connection.");
            CreateSetting("MySQL.Pass", "pass", "Password for MySQL connection.");
            CreateSetting("MySQL.Database", "aardwolf", "Database name for the MySQL connection.");
        }
    }

    internal class Database
    {
        internal Database(string Host, string Login, string Password, int Port, string dbName)
        {
            host = Host;
            login = Login;
            password = Password;
            port = Port;
            DBName = dbName;
        }

        private readonly string host;
        private readonly string login;
        private readonly string password;
        private readonly int port;
        private readonly string DBName;
        private Dictionary<QueryData, MySqlConnection> _con = new Dictionary<QueryData, MySqlConnection>();
        private MySqlConnection freeConnection = null;
        private long freeTimer = 0;

        internal void Update(long msTime)
        {
            if(freeConnection != null && freeTimer < msTime)
            {
                freeConnection.Close();
                freeConnection = null;
            }

            QueryData idle = null;
            foreach(KeyValuePair<QueryData, MySqlConnection> x in _con)
            {
                if(x.Key.IsIdle)
                {
                    idle = x.Key;
                    break;
                }
            }

            if(idle != null)
                _CloseResult(idle);
        }

        internal void CloseAll()
        {
            if(freeConnection != null)
            {
                freeConnection.Close();
                freeConnection = null;
            }

            foreach(KeyValuePair<QueryData, MySqlConnection> x in _con)
            {
                x.Key._Close();
                x.Value.Close();
            }

            _con.Clear();
        }

        internal void _CloseResult(QueryData data)
        {
            data._Close();
            if(!_con.ContainsKey(data))
                return;

            if(freeConnection == null)
                freeConnection = _con[data];
            _con.Remove(data);
            freeTimer = MySQL.MSTime + 3000;
        }

        internal QueryData Query(string q)
        {
            MySqlConnection con = null;
            if(freeConnection == null)
            {
                string conString = "SERVER=" + host + ";DATABASE=" + DBName + ";UID=" + login + ";PASSWORD=" + password + ";PORT=" + port.ToString() + ";";
                con = new MySqlConnection(conString);
                con.Open();
            }
            else
            {
                con = freeConnection;
                freeConnection = null;
            }

            MySqlCommand cmd = con.CreateCommand();
            cmd.CommandText = q;
            MySqlDataReader reader = cmd.ExecuteReader();

            QueryData m = new QueryData(reader, this);
            _con[m] = con;
            return m;
        }

        internal void Execute(string q)
        {
            MySqlConnection con = null;
            if(freeConnection == null)
            {
                string conString = "SERVER=" + host + ";DATABASE=" + DBName + ";UID=" + login + ";PASSWORD=" + password + ";PORT=" + port.ToString() + ";";
                con = new MySqlConnection(conString);
                con.Open();
            }
            else
                con = freeConnection;

            MySqlCommand cmd = con.CreateCommand();
            cmd.CommandText = q;
            cmd.ExecuteNonQuery();
            if(freeConnection == null)
                freeConnection = con;
            else if(freeConnection != con)
                con.Close();

            freeTimer = MySQL.MSTime + 3000;
        }
    }

    public class QueryData
    {
        internal QueryData(MySqlDataReader res, Database connection)
        {
            r = res;
            db = connection;
            lastUpdate = MySQL.MSTime;
        }

        internal readonly MySqlDataReader r;
        internal readonly Database db;

        private long lastUpdate;

        internal bool IsIdle
        {
            get
            {
                return MySQL.MSTime - lastUpdate > 5000;
            }
        }

        /// <summary>
        /// Close and free the result.
        /// </summary>
        public void Close()
        {
            db._CloseResult(this);
        }

        internal void _Close()
        {
            if(!r.IsClosed)
                r.Close();
        }

        /// <summary>
        /// Read the next row in the result. If next row exists true will be returned, otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            lastUpdate = MySQL.MSTime;
            return r.Read();
        }

        /// <summary>
        /// Read a double value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double GetDouble(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetDouble(i);
            return 0;
        }

        /// <summary>
        /// Read a float value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float GetFloat(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetFloat(i);
            return 0;
        }

        /// <summary>
        /// Read an integer value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetInt32(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetInt32(i);
            return 0;
        }

        /// <summary>
        /// Read an unsigned integer value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public uint GetUInt32(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetUInt32(i);
            return 0;
        }

        /// <summary>
        /// Read a long value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public long GetInt64(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetInt64(i);
            return 0;
        }

        /// <summary>
        /// Read an unsigned long value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ulong GetUInt64(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetUInt64(i);
            return 0;
        }

        /// <summary>
        /// Read a string value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetString(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetString(i);
            return null;
        }

        /// <summary>
        /// Read a short value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public short GetInt16(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetInt16(i);
            return 0;
        }

        /// <summary>
        /// Read an unsigned short value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ushort GetUInt16(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetUInt16(i);
            return 0;
        }

        /// <summary>
        /// Read a byte value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte GetByte(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetByte(i);
            return 0;
        }

        /// <summary>
        /// Read a char value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public char GetChar(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetChar(i);
            return '\0';
        }

        /// <summary>
        /// Read a boolean value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool GetBool(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetBoolean(i);
            return false;
        }

        /// <summary>
        /// Read a date time value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetDateTime(i);
            return DateTime.MinValue;
        }

        /// <summary>
        /// Read a time span value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public TimeSpan GetTimeSpan(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetTimeSpan(i);
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Read an object value in the specified field. Field index starts with 0.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i)
        {
            lastUpdate = MySQL.MSTime;
            if(r.FieldCount > i && !r.IsDBNull(i))
                return r.GetValue(i);
            return 0;
        }
    }
}
