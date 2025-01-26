using System.Text.Json;
using Test.Common;
using Test.Entities;
using Test.Services;

namespace Test
{
    internal class Program
    {
        static readonly TripService service;

        static Program()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "appSettings.json");
            using var stream = File.OpenRead(path);
            var settings = JsonSerializer.Deserialize<Settings>(stream);
            service = new TripService(settings.ConnectionString);
        }

        static async Task Main()
        {
            var commands = new Dictionary<int, Command>
            {
                { 1,  new Command{Name = "Load data from csv file", Action = LoadDataFromCsv }},
                { 2,  new Command{Name = "Get pick-up location id with highest average tip", Action = GetPULocationWithHighestTipAsync }},
                { 3,  new Command{Name = "Get top 100 fares by distance", Action = GetTop100TripsByDistanceAsync }},
                { 4,  new Command{Name = "Get top 100 fares by spent time", Action = GetTop100TripsBySpentTimeAsync }},
                { 5,  new Command{Name = "Get trips by pick-up location", Action = GetTripsByPULocationIDAsync }}
            };

            while (true)
            {
                Console.Clear();
                PrintCommands(commands);
                var commandKey = InputHelper.InputInt("Enter command: ");
                if (!commands.TryGetValue(commandKey, out var command))
                {
                    Console.WriteLine("Unknown command");
                }
                else
                {
                    await command.Action();
                }

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
        }

        static void PrintCommands(Dictionary<int, Command> commands)
        {
            foreach (var (key, value) in commands)
            {
                Console.WriteLine($"{key}. {value.Name}");
            }
        }

        static async Task LoadDataFromCsv()
        {
            try
            {
                Console.Clear();
                var path = InputHelper.InputFile("Drop csv file here ", [".csv"]);
                await service.LoadDataFromCSVAsync(path);
                Console.WriteLine("Loaded successfully");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File wad not found");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Directory was not found");
            }
            catch (IOException)
            {
                Console.WriteLine("Error occured while reading or writing file. Close other applications that may use that file");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("No access. Try to run this app as administator");
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong");
            }
        }

        static async Task GetPULocationWithHighestTipAsync()
        {
            Console.Clear();
            var result = await service.GetPULocationWithHighestTipAsync();
            if (result == null)
            {
                Console.WriteLine($"No data");
            }
            else
            {
                Console.WriteLine($"Pick-up location with the highest tip: {result}");
            }
        }

        static void PrintTrips(IEnumerable<Trip> trips)
        {
            foreach (var trip in trips)
            {
                Console.WriteLine($"Pick-up datetime: {trip.tpep_pickup_datetime}");
                Console.WriteLine($"Drop-off datetime: {trip.tpep_dropoff_datetime}");
                Console.WriteLine($"Passenger count: {trip.passenger_count}");
                Console.WriteLine($"Trip distance: {trip.trip_distance}");
                Console.WriteLine($"Store and fwd flag: {trip.store_and_fwd_flag}");
                Console.WriteLine($"Pick-up location id: {trip.PULocationID}");
                Console.WriteLine($"Drop-off location id: {trip.DOLocationID}");
                Console.WriteLine($"Fare amount: {trip.fare_amount}");
                Console.WriteLine($"Tip amount: {trip.tip_amount}");
                Console.WriteLine();
            }
        }

        static async Task GetTop100TripsByDistanceAsync()
        {
            Console.Clear();
            Console.WriteLine("Top 100 longest fares by distance");

            var trips = await service.GetTop100TripsByDistanceAsync();

            PrintTrips(trips);
        }

        static async Task GetTop100TripsBySpentTimeAsync()
        {
            Console.Clear();
            Console.WriteLine("Top 100 longest fares by spent time");

            var trips = await service.GetTop100TripsBySpentTimeAsync();

            PrintTrips(trips);
        }

        static async Task GetTripsByPULocationIDAsync()
        {
            Console.Clear();
            var id = InputHelper.InputInt("Input id of pick - up location: ");

            Console.WriteLine("Trips by pick-up location");

            var trips = await service.GetTripsByPULocationIDAsync(id);
            PrintTrips(trips);
        }
    }
}