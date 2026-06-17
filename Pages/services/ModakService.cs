using Microsoft.Data.Sqlite;
using System.Collections.Generic;

// =============================================
// MODELS
// =============================================

public class Modak
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public string Unit { get; set; } = "per pc";
    public string Description { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public List<string> Images { get; set; } = new();
    public double AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class Feedback
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ModakName { get; set; } = "";
    public int Rating { get; set; }
    public string Tags { get; set; } = "";
    public string Message { get; set; } = "";
    public string Occasion { get; set; } = "";
    public bool WouldRecommend { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class FestiveEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string ImagePath { get; set; } = "";
    public bool IsActive { get; set; } = false;
}

public class TrustBadge
{
    public int Id { get; set; }
    public string Icon { get; set; } = "";
    public string Title { get; set; } = "";
    public int SortOrder { get; set; }
}

// =============================================
// SERVICE
// =============================================

public class ModakService
{
    private string connectionString = "Data Source=modak.db";

    // ─────────────────────────────────────────
    // DATABASE INIT
    // ─────────────────────────────────────────
    public void InitializeDatabase()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS Modaks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Price INTEGER NOT NULL,
    Unit TEXT DEFAULT 'per pc',
    Description TEXT,
    CategoryId INTEGER,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
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

CREATE TABLE IF NOT EXISTS Feedbacks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Phone TEXT,
    ModakName TEXT,
    Rating INTEGER NOT NULL,
    Tags TEXT,
    Message TEXT NOT NULL,
    Occasion TEXT,
    WouldRecommend INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now','localtime'))
);

CREATE TABLE IF NOT EXISTS FestiveEvent (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT,
    EventDate TEXT NOT NULL,
    ImagePath TEXT,
    IsActive INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS TrustBadges (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Icon TEXT NOT NULL,
    Title TEXT NOT NULL,
    SortOrder INTEGER DEFAULT 0
);
";
        command.ExecuteNonQuery();

        // Seed categories
        var categoryCmd = connection.CreateCommand();
        categoryCmd.CommandText = @"
INSERT OR IGNORE INTO Categories (Id, Name) VALUES
(1, 'Traditional'),
(2, 'Chocolate'),
(3, 'Dry Fruit'),
(4, 'Premium'),
(5, 'Healthy'),
(6, 'Festival Special');
";
        categoryCmd.ExecuteNonQuery();

        // Seed festive event default
        var festiveCmd = connection.CreateCommand();
        festiveCmd.CommandText = @"
INSERT OR IGNORE INTO FestiveEvent (Id, Title, Description, EventDate, ImagePath, IsActive)
VALUES (1, 'Ganesh Chaturthi Special',
        'Pre-orders are now open! Book your favourite modaks in advance and get 5% extra discount.',
        '2025-08-27 00:00:00', '', 0);
";
        festiveCmd.ExecuteNonQuery();

        // Seed trust badges
        var badgeCmd = connection.CreateCommand();
        badgeCmd.CommandText = @"
INSERT OR IGNORE INTO TrustBadges (Id, Icon, Title, SortOrder) VALUES
(1, '🌿', '100% Pure Ingredients', 1),
(2, '🛡️', 'Hygienically Prepared', 2),
(3, '🍡', 'Freshly Made on Order', 3),
(4, '🚚', 'On-time Delivery', 4),
(5, '📦', 'Secure Packaging', 5),
(6, '💬', 'Customer Support', 6);
";
        badgeCmd.ExecuteNonQuery();
    }

    // ─────────────────────────────────────────
    // MODAKS
    // ─────────────────────────────────────────
    public void AddModak(Modak modak)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Modaks (Name, Price, Unit, Description, CategoryId)
VALUES (@name, @price, @unit, @desc, @categoryId);
SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("@name", modak.Name);
        command.Parameters.AddWithValue("@price", modak.Price);
        command.Parameters.AddWithValue("@unit", modak.Unit ?? "per pc");
        command.Parameters.AddWithValue("@desc", modak.Description);
        command.Parameters.AddWithValue("@categoryId", modak.CategoryId);

        long modakId = Convert.ToInt64(command.ExecuteScalar());

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

    public List<Modak> GetAllModaks()
    {
        var modaks = new Dictionary<int, Modak>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    m.Id, m.Name, m.Price, m.Description, m.CategoryId, c.Name, i.ImagePath,
    COALESCE(AVG(f.Rating), 0) AS AvgRating,
    COUNT(DISTINCT f.Id) AS ReviewCount,
    COALESCE(m.Unit, 'per pc') AS Unit
FROM Modaks m
LEFT JOIN Categories c ON m.CategoryId = c.Id
LEFT JOIN ModakImages i ON m.Id = i.ModakId
LEFT JOIN Feedbacks f ON LOWER(f.ModakName) = LOWER(m.Name)
GROUP BY m.Id, m.Name, m.Price, m.Description, m.CategoryId, c.Name, i.ImagePath, i.Id, m.Unit
ORDER BY m.Id";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            int id = reader.GetInt32(0);

            if (!modaks.ContainsKey(id))
            {
                modaks[id] = new Modak
                {
                    Id = id,
                    Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Price = reader.GetInt32(2),
                    Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    CategoryId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    CategoryName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Images = new List<string>(),
                    AverageRating = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
                    ReviewCount = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    Unit = reader.IsDBNull(9) ? "per pc" : reader.GetString(9)
                };
            }

            if (!reader.IsDBNull(6))
                modaks[id].Images.Add(reader.GetString(6));
        }

        return new List<Modak>(modaks.Values);
    }

    public Modak? GetModakById(int id)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    m.Id, m.Name, m.Price, m.Description, m.CategoryId, c.Name, i.ImagePath,
    COALESCE(AVG(f.Rating), 0) AS AvgRating,
    COUNT(DISTINCT f.Id) AS ReviewCount,
    COALESCE(m.Unit, 'per pc') AS Unit
FROM Modaks m
LEFT JOIN Categories c ON m.CategoryId = c.Id
LEFT JOIN ModakImages i ON m.Id = i.ModakId
LEFT JOIN Feedbacks f ON LOWER(f.ModakName) = LOWER(m.Name)
WHERE m.Id = @id
GROUP BY m.Id, m.Name, m.Price, m.Description, m.CategoryId, c.Name, i.ImagePath, i.Id, m.Unit";

        command.Parameters.AddWithValue("@id", id);

        Modak? modak = null;
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (modak == null)
            {
                modak = new Modak
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Price = reader.GetInt32(2),
                    Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    CategoryId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    CategoryName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Images = new List<string>(),
                    AverageRating = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
                    ReviewCount = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    Unit = reader.IsDBNull(9) ? "per pc" : reader.GetString(9)
                };
            }

            if (!reader.IsDBNull(6))
                modak.Images.Add(reader.GetString(6));
        }

        return modak;
    }

    public void UpdateModak(Modak modak)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Modaks
SET Name = @name, Price = @price, Unit = @unit, Description = @desc, CategoryId = @categoryId
WHERE Id = @id";

        command.Parameters.AddWithValue("@name", modak.Name);
        command.Parameters.AddWithValue("@price", modak.Price);
        command.Parameters.AddWithValue("@unit", modak.Unit ?? "per pc");
        command.Parameters.AddWithValue("@desc", modak.Description);
        command.Parameters.AddWithValue("@categoryId", modak.CategoryId);
        command.Parameters.AddWithValue("@id", modak.Id);
        command.ExecuteNonQuery();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM ModakImages WHERE ModakId = @id";
        deleteCmd.Parameters.AddWithValue("@id", modak.Id);
        deleteCmd.ExecuteNonQuery();

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

    public void DeleteModak(int id)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var imgCmd = connection.CreateCommand();
        imgCmd.CommandText = "DELETE FROM ModakImages WHERE ModakId = @id";
        imgCmd.Parameters.AddWithValue("@id", id);
        imgCmd.ExecuteNonQuery();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Modaks WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ─────────────────────────────────────────
    // CATEGORIES
    // ─────────────────────────────────────────
    public List<Category> GetCategories()
    {
        var categories = new List<Category>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM Categories";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? "" : reader.GetString(1)
            });
        }

        return categories;
    }

    // ─────────────────────────────────────────
    // ADMIN AUTH
    // ─────────────────────────────────────────
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

        try { command.ExecuteNonQuery(); return true; }
        catch { return false; }
    }

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

        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    // ─────────────────────────────────────────
    // FEEDBACKS
    // ─────────────────────────────────────────
    public void AddFeedback(Feedback feedback)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Feedbacks (Name, Phone, ModakName, Rating, Tags, Message, Occasion, WouldRecommend)
VALUES (@name, @phone, @modakName, @rating, @tags, @message, @occasion, @recommend)";

        command.Parameters.AddWithValue("@name", feedback.Name);
        command.Parameters.AddWithValue("@phone", feedback.Phone ?? "");
        command.Parameters.AddWithValue("@modakName", feedback.ModakName ?? "");
        command.Parameters.AddWithValue("@rating", feedback.Rating);
        command.Parameters.AddWithValue("@tags", feedback.Tags ?? "");
        command.Parameters.AddWithValue("@message", feedback.Message);
        command.Parameters.AddWithValue("@occasion", feedback.Occasion ?? "");
        command.Parameters.AddWithValue("@recommend", feedback.WouldRecommend ? 1 : 0);

        command.ExecuteNonQuery();
    }

    public List<Feedback> GetAllFeedbacks()
    {
        var list = new List<Feedback>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Phone, ModakName, Rating, Tags, Message, Occasion, WouldRecommend, CreatedAt FROM Feedbacks ORDER BY CreatedAt DESC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Feedback
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                ModakName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Rating = reader.GetInt32(4),
                Tags = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Message = reader.IsDBNull(6) ? "" : reader.GetString(6),
                Occasion = reader.IsDBNull(7) ? "" : reader.GetString(7),
                WouldRecommend = reader.GetInt32(8) == 1,
                CreatedAt = DateTime.TryParse(reader.GetString(9), out var dt) ? dt : DateTime.Now
            });
        }

        return list;
    }

    public List<Feedback> GetLatestFeedbacks(int count = 3)
    {
        var list = new List<Feedback>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, Name, Phone, ModakName, Rating, Tags, Message, Occasion, WouldRecommend, CreatedAt
FROM Feedbacks
ORDER BY CreatedAt DESC
LIMIT @count";
        command.Parameters.AddWithValue("@count", count);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Feedback
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                ModakName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Rating = reader.GetInt32(4),
                Tags = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Message = reader.IsDBNull(6) ? "" : reader.GetString(6),
                Occasion = reader.IsDBNull(7) ? "" : reader.GetString(7),
                WouldRecommend = reader.GetInt32(8) == 1,
                CreatedAt = DateTime.TryParse(reader.GetString(9), out var dt) ? dt : DateTime.Now
            });
        }

        return list;
    }

    public double GetAverageRating()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(AVG(Rating), 0) FROM Feedbacks";
        var result = command.ExecuteScalar();
        return result == DBNull.Value ? 0 : Convert.ToDouble(result);
    }

    public int GetTotalFeedbackCount()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Feedbacks";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // ─────────────────────────────────────────
    // FESTIVE EVENT
    // ─────────────────────────────────────────
    public FestiveEvent? GetActiveFestiveEvent()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, Title, Description, EventDate, ImagePath, IsActive
FROM FestiveEvent
WHERE IsActive = 1
LIMIT 1";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new FestiveEvent
            {
                Id = reader.GetInt32(0),
                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                EventDate = DateTime.Parse(reader.GetString(3)),
                ImagePath = reader.IsDBNull(4) ? "" : reader.GetString(4),
                IsActive = reader.GetInt32(5) == 1
            };
        }

        return null;
    }

    public FestiveEvent GetFestiveEventForAdmin()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, EventDate, ImagePath, IsActive FROM FestiveEvent WHERE Id = 1";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new FestiveEvent
            {
                Id = reader.GetInt32(0),
                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                EventDate = DateTime.Parse(reader.GetString(3)),
                ImagePath = reader.IsDBNull(4) ? "" : reader.GetString(4),
                IsActive = reader.GetInt32(5) == 1
            };
        }

        return new FestiveEvent { Id = 1, Title = "", Description = "", EventDate = DateTime.Now.AddDays(30), IsActive = false };
    }

    public void UpdateFestiveEvent(FestiveEvent ev)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE FestiveEvent
SET Title = @title, Description = @desc, EventDate = @date, IsActive = @active,
    ImagePath = CASE WHEN @image = '' THEN ImagePath ELSE @image END
WHERE Id = 1";

        command.Parameters.AddWithValue("@title", ev.Title);
        command.Parameters.AddWithValue("@desc", ev.Description);
        command.Parameters.AddWithValue("@date", ev.EventDate.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@active", ev.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@image", ev.ImagePath ?? "");

        command.ExecuteNonQuery();
    }

    // ─────────────────────────────────────────
    // TRUST BADGES
    // ─────────────────────────────────────────
    public List<TrustBadge> GetTrustBadges()
    {
        var list = new List<TrustBadge>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Icon, Title, SortOrder FROM TrustBadges ORDER BY SortOrder";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new TrustBadge
            {
                Id = reader.GetInt32(0),
                Icon = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Title = reader.IsDBNull(2) ? "" : reader.GetString(2),
                SortOrder = reader.GetInt32(3)
            });
        }

        return list;
    }

    public void UpdateTrustBadge(TrustBadge badge)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE TrustBadges
SET Icon = @icon, Title = @title
WHERE Id = @id";

        command.Parameters.AddWithValue("@icon", badge.Icon);
        command.Parameters.AddWithValue("@title", badge.Title);
        command.Parameters.AddWithValue("@id", badge.Id);

        command.ExecuteNonQuery();
    }
}
