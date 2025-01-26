using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using Test.Entities;
using Test.Models;

namespace Test.Services
{
    public class TripService
    {
        private readonly string _connectionString;
        private const string DUPLICATE_FILE = "duplicate.csv";

        public TripService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task LoadDataFromCSVAsync(string filePath)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, csvConfig);

            var groupedData = csv.GetRecords<TripCsv>().ToList()
                .GroupBy(c => new { c.tpep_dropoff_datetime, c.passenger_count, c.tpep_pickup_datetime });
            var duplicateValues = groupedData.Where(g => g.Count() > 1).SelectMany(g => g);
            var uniqueValues = groupedData.Where(g => g.Count() == 1).Select(g => g.Single());
            var dataTable = ToDataTable(uniqueValues);

            using var sqlBulk = new SqlBulkCopy(_connectionString);
            sqlBulk.DestinationTableName = "Trips";
            await sqlBulk.WriteToServerAsync(dataTable);

            WriteDuplicates(duplicateValues);
        }

        private DataTable ToDataTable(IEnumerable<TripCsv> trips)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(nameof(Trip.tpep_pickup_datetime));
            dataTable.Columns.Add(nameof(Trip.tpep_dropoff_datetime));
            dataTable.Columns.Add(nameof(Trip.passenger_count));
            dataTable.Columns.Add(nameof(Trip.trip_distance));
            dataTable.Columns.Add(nameof(Trip.store_and_fwd_flag));
            dataTable.Columns.Add(nameof(Trip.PULocationID));
            dataTable.Columns.Add(nameof(Trip.DOLocationID));
            dataTable.Columns.Add(nameof(Trip.fare_amount));
            dataTable.Columns.Add(nameof(Trip.tip_amount));

            foreach (var trip in trips)
            {
                TransformAndClearData(trip);
                dataTable.Rows.Add(ToDateTimeOffset(trip.tpep_pickup_datetime), ToDateTimeOffset(trip.tpep_dropoff_datetime),
                    trip.passenger_count, trip.trip_distance, trip.store_and_fwd_flag, trip.PULocationID, trip.DOLocationID,
                    trip.fare_amount, trip.tip_amount
                );
            }

            return dataTable;
        }

        private DateTimeOffset ToDateTimeOffset(DateTime date)
        {
            return new DateTimeOffset(date, TimeSpan.Zero);
        }

        private void TransformAndClearData(TripCsv data)
        {
            data.store_and_fwd_flag = data.store_and_fwd_flag.Trim();
            data.store_and_fwd_flag = data.store_and_fwd_flag == "N" ? "No" : "Yes";

            //From est time
            data.tpep_dropoff_datetime = data.tpep_dropoff_datetime.AddHours(5);
            data.tpep_pickup_datetime = data.tpep_pickup_datetime.AddHours(5);
        }

        private void WriteDuplicates(IEnumerable<TripCsv> duplicates)
        {
            using var writer = new StreamWriter(DUPLICATE_FILE);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(duplicates);
        }

        public async Task<int?> GetPULocationWithHighestTipAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var result = await connection.QuerySingleOrDefaultAsync<int>("SELECT TOP 1 PULocationID FROM Trips GROUP BY PULocationID ORDER BY avg(tip_amount) DESC");

            return result;
        }

        public async Task<IEnumerable<Trip>> GetTop100TripsByDistanceAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var result = await connection.QueryAsync<Trip>("SELECT TOP 100 * FROM Trips ORDER BY trip_distance DESC");

            return result;
        }

        public async Task<IEnumerable<Trip>> GetTop100TripsBySpentTimeAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var result = await connection.QueryAsync<Trip>("SELECT TOP 100 * FROM Trips ORDER BY DATEDIFF(SECOND, tpep_pickup_datetime, tpep_dropoff_datetime) DESC");

            return result;
        }

        public async Task<IEnumerable<Trip>> GetTripsByPULocationIDAsync(int PULocationID)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var parameters = new { PULocationID };

            var result = await connection.QueryAsync<Trip>("SELECT * FROM TRIPS WHERE PULocationID = @PULocationID", parameters);

            return result;
        }
    }
}
