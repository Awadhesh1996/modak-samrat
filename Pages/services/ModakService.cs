using Microsoft.Data.Sqlite;
using System.Collections.Generic;

public class Modak
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Price { get; set; }

    public string Description { get; set; } = "";

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = "";

    public List<string> Images { get; set; } = new();
}

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
}
public class ModakService
{


    private string connectionString = "Data Source=modak.db";

    public void InitializeDatabase()
    {
        using var connection = new SqliteConnection("Data Source=modak.db");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS Modaks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Price INTEGER NOT NULL,
    Description TEXT,
    CategoryId INTEGER,

    FOREIGN KEY (CategoryId)
    REFERENCES Categories(Id)
);

    CREATE TABLE IF NOT EXISTS ModakImages (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        ModakId INTEGER NOT NULL,
        ImagePath TEXT NOT NULL,
        FOREIGN KEY (ModakId) REFERENCES Modaks(Id)
    );

    CREATE TABLE IF NOT EXISTS Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
    );
    
    CREATE TABLE IF NOT EXISTS Admins (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Phone TEXT,
    Username TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL
    );
    ";

        command.ExecuteNonQuery();
        var categoryCmd = connection.CreateCommand();

        categoryCmd.CommandText = @"
INSERT OR IGNORE INTO Categories (Id, Name)
VALUES
(1, 'Traditional'),
(2, 'Chocolate'),
(3, 'Dry Fruit'),
(4, 'Premium'),
(5, 'Healthy'),
(6, 'Festival Special');
";

        categoryCmd.ExecuteNonQuery();
    }

    // ✅ INSERT MODAK WITH MULTIPLE IMAGES
    public void AddModak(Modak modak)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Modaks (Name, Price, Description, CategoryId)
        VALUES (@name, @price, @desc, @categoryId);
        SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("@name", modak.Name);
        command.Parameters.AddWithValue("@price", modak.Price);
        command.Parameters.AddWithValue("@desc", modak.Description);
        command.Parameters.AddWithValue("@categoryId", modak.CategoryId);

        long modakId = Convert.ToInt64(command.ExecuteScalar());

        // Insert images
        foreach (var img in modak.Images)
        {
            var imgCommand = connection.CreateCommand();
            imgCommand.CommandText = @"
            INSERT INTO ModakImages (ModakId, ImagePath)
            VALUES (@modakId, @img)";

            imgCommand.Parameters.AddWithValue("@modakId", modakId);
            imgCommand.Parameters.AddWithValue("@img", img);

            imgCommand.ExecuteNonQuery();
        }
    }

    // ✅ GET ALL MODAKS
    public List<Modak> GetAllModaks()
    {
        var modaks = new Dictionary<int, Modak>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT
m.Id,
m.Name,
m.Price,
m.Description,
m.CategoryId,
c.Name,
i.ImagePath

FROM Modaks m

LEFT JOIN Categories c
ON m.CategoryId = c.Id

LEFT JOIN ModakImages i
ON m.Id = i.ModakId";

        using var reader = command.ExecuteReader();

while (reader.Read())
{
    int id = reader.GetInt32(0);

    if (!modaks.ContainsKey(id))
    {
        modaks[id] = new Modak
        {
            Id = id,

            Name = reader.IsDBNull(1)
                ? ""
                : reader.GetString(1),

            Price = reader.GetInt32(2),

            Description = reader.IsDBNull(3)
                ? ""
                : reader.GetString(3),

            CategoryId = reader.IsDBNull(4)
                ? 0
                : reader.GetInt32(4),

            CategoryName = reader.IsDBNull(5)
                ? ""
                : reader.GetString(5),

            Images = new List<string>()
        };
    }

    // ImagePath is now index 6
    if (!reader.IsDBNull(6))
    {
        modaks[id]
        .Images
        .Add(reader.GetString(6));
    }
}

return new List<Modak>(modaks.Values);
    }

    // ✅ UPDATE MODAK
    public void UpdateModak(Modak modak)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE Modaks
        SET Name = @name, Price = @price, Description = @desc, CategoryId = @categoryId
        WHERE Id = @id";

        command.Parameters.AddWithValue("@name", modak.Name);
        command.Parameters.AddWithValue("@price", modak.Price);
        command.Parameters.AddWithValue("@desc", modak.Description);
        command.Parameters.AddWithValue("@categoryId", modak.CategoryId);
        command.Parameters.AddWithValue("@id", modak.Id);

        command.ExecuteNonQuery();

        // Delete old images
        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM ModakImages WHERE ModakId = @id";
        deleteCmd.Parameters.AddWithValue("@id", modak.Id);
        deleteCmd.ExecuteNonQuery();

        // Insert new images
        foreach (var img in modak.Images)
        {
            var imgCommand = connection.CreateCommand();
            imgCommand.CommandText = @"
            INSERT INTO ModakImages (ModakId, ImagePath)
            VALUES (@modakId, @img)";

            imgCommand.Parameters.AddWithValue("@modakId", modak.Id);
            imgCommand.Parameters.AddWithValue("@img", img);

            imgCommand.ExecuteNonQuery();
        }
    }

    // ✅ DELETE MODAK
    public void DeleteModak(int id)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        // Delete images first
        var imgCmd = connection.CreateCommand();
        imgCmd.CommandText = "DELETE FROM ModakImages WHERE ModakId = @id";
        imgCmd.Parameters.AddWithValue("@id", id);
        imgCmd.ExecuteNonQuery();

        // Delete modak
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Modaks WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ✅ REGISTER ADMIN
    public bool RegisterAdmin(string name, string phone, string username, string password)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Admins (Name, Phone, Username, Password)
        VALUES (@name, @phone, @username, @password)";

        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@phone", phone);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        try
        {
            command.ExecuteNonQuery();
            return true;
        }
        catch
        {
            return false; // username exists
        }
    }


    // ✅ LOGIN ADMIN
    public bool LoginAdmin(string username, string password)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
    SELECT COUNT(*) FROM Admins
    WHERE Username = @username AND Password = @password";

        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        var result = Convert.ToInt64(command.ExecuteScalar());

        return result > 0;
    }
    public List<Category> GetCategories()
{
    var categories = new List<Category>();

    using var connection =
    new SqliteConnection(connectionString);

    connection.Open();

    var command = connection.CreateCommand();

    command.CommandText =
    "SELECT Id, Name FROM Categories";

    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
        categories.Add(new Category
        {
            Id = reader.GetInt32(0),

            Name = reader.IsDBNull(1)
                ? ""
                : reader.GetString(1)
        });
    }

    return categories;
}
}