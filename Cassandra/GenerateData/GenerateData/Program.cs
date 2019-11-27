using Cassandra;
using System;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using Cassandra.Mapping;

namespace GenerateData
{
    class Program
    {
        private static string UserName = "";
        private static string Password = "";
        private static string CassandraContactPoint = "";  
        private static int CassandraPort = 0;

        static void Main(string[] args)
        {
            try
            {
                // Connect to the Cassandra database
                UserName = ConfigurationManager.AppSettings["UserName"];
                Password = ConfigurationManager.AppSettings["Password"];
                CassandraContactPoint = ConfigurationManager.AppSettings["CassandraContactPoint"];
                CassandraPort = int.Parse(ConfigurationManager.AppSettings["CassandraPort"]);

                // Connection settings for Bitnami Cassandra server
                Cluster cluster = Cluster.Builder().WithCredentials(UserName, Password).WithPort(CassandraPort).AddContactPoint(CassandraContactPoint).Build();

                // Connection settings for Cosmos DB
                
                // var options = new Cassandra.SSLOptions(SslProtocols.Tls12, true, ValidateServerCertificate);
                // options.SetHostNameResolver((ipAddress) => CassandraContactPoint);
                // Cluster cluster = Cluster.Builder().WithCredentials(UserName, Password).WithPort(CassandraPort).AddContactPoint(CassandraContactPoint).WithSSL(options).Build();
                ISession session = cluster.Connect();

                // Rebuild the keyspaces and tables
                session.Execute("DROP KEYSPACE IF EXISTS customerinfo");
                session.Execute("CREATE KEYSPACE customerinfo WITH " +
                    "REPLICATION = { 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 };");
                Console.WriteLine(String.Format("created keyspace customerinfo"));

                session.Execute("CREATE TABLE IF NOT EXISTS customerinfo.customerdetails (" +
                    "customerid int, " +
                    "firstname text, " +
                    "lastname text, " +
                    "email text, " +
                    "stateprovince text, " +
                    "PRIMARY KEY((stateprovince), customerid))");
                Console.WriteLine(String.Format("created table customerdetails"));

                session.Execute("DROP KEYSPACE IF EXISTS orderinfo");
                session.Execute("CREATE KEYSPACE orderinfo WITH " +
                    "REPLICATION = { 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 };");
                Console.WriteLine(String.Format("created keyspace orderinfo"));

                session.Execute("CREATE TABLE IF NOT EXISTS orderinfo.orderdetails (" +
                    "orderid int, " +
                    "customerid int, " +
                    "orderdate date, " +
                    "ordervalue decimal, " +
                    "PRIMARY KEY((customerid), orderdate, orderid))");
                Console.WriteLine(String.Format("created table orderdetails"));

                session.Execute("CREATE TABLE IF NOT EXISTS orderinfo.orderline (" +
                    "orderid int, " +
                    "orderline int, " +
                    "productname text, " +
                    "quantity smallint, " +
                    "orderlinecost decimal, " +
                    "PRIMARY KEY ((orderid), productname, orderline))");
                Console.WriteLine(String.Format("created table orderline"));

                // Retrieve the customer data from SQL Server
                var sqlDB = new AdventureWorks2016_DataEntities();
                var custQuery = (from c in sqlDB.Customers
                                 where c.CustomerID > 1000
                                 select new { c.CustomerID, c.Person.FirstName, c.Person.LastName, EmailAddress = c.Person.EmailAddresses.FirstOrDefault().EmailAddress1, StateProvince = c.Person.BusinessEntity.BusinessEntityAddresses.FirstOrDefault().Address.StateProvince.Name }).Distinct();

                // Copy the data for each customer
                session = cluster.Connect("customerinfo");
                IMapper mapper = new Mapper(session);

                foreach (var customer in custQuery)
                {
                    Console.WriteLine($"Adding details for {customer.CustomerID}, {customer.FirstName}, {customer.LastName}, {customer.EmailAddress}, {customer.StateProvince}");
                    mapper.Insert<CustomerDetails>(new CustomerDetails
                    {
                        customerid = customer.CustomerID,
                        firstname = customer.FirstName,
                        lastname = customer.LastName,
                        email = customer.EmailAddress,
                        stateprovince = customer.StateProvince ?? "Not specified"
                    });
                }

                // Retrieve product and order data from SQL Server
                var orderQuery = from o in sqlDB.SalesOrderHeaders
                                 select o;

                var productData = (from p in sqlDB.Products
                                   select new { p.ProductID, p.Name }).ToList();

                // Copy the data for each order
                session = cluster.Connect("orderinfo");
                mapper = new Mapper(session);

                foreach (var salesOrder in orderQuery)
                {
                    var orderDetailsQuery = from d in salesOrder.SalesOrderDetails
                                            join p in productData
                                            on d.ProductID equals p.ProductID
                                            select new { d.SalesOrderDetailID, p.Name, d.OrderQty, d.LineTotal };

                    mapper.Insert<OrderDetails>(new OrderDetails
                    {
                        orderid = salesOrder.SalesOrderID,
                        customerid = salesOrder.CustomerID,
                        orderdate = new LocalDate(salesOrder.OrderDate.Year, salesOrder.OrderDate.Month, salesOrder.OrderDate.Day),
                        ordervalue = salesOrder.TotalDue
                    });

                    foreach (var salesOrderLine in orderDetailsQuery)
                    {
                        Console.WriteLine($"Adding details for {salesOrder.SalesOrderID}, {salesOrder.CustomerID}, {salesOrder.OrderDate}, {salesOrder.TotalDue}, {salesOrderLine.SalesOrderDetailID}, {salesOrderLine.Name}, {salesOrderLine.OrderQty}, {salesOrderLine.LineTotal}");

                        mapper.Insert<OrderLine>(new OrderLine
                        {
                            orderid = salesOrder.SalesOrderID,
                            orderline = salesOrderLine.SalesOrderDetailID,
                            productname = salesOrderLine.Name,
                            quantity = salesOrderLine.OrderQty,
                            orderlinecost = salesOrderLine.LineTotal
                        });
                    }
                }

                Console.WriteLine("Data uploaded");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Program failed with error: {e.Message}");
            }
        }

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }
    }
}
