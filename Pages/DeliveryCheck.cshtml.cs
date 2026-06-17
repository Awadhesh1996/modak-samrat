using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;

public class DeliveryCheckModel : PageModel
{
    private static readonly HttpClient _http = new HttpClient();

    // Shop coordinates
    private const double SHOP_LAT = 19.021055;
    private const double SHOP_LNG = 72.835238;

    public async Task<IActionResult> OnGetAsync(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return new JsonResult(new { error = "Please enter a pincode or area." });

        try
        {
            // Build query — if 6 digits treat as pincode
            bool isPin = q.Trim().Length == 6 && q.Trim().All(char.IsDigit);
            string query = isPin ? $"{q.Trim()}, Maharashtra, India" : $"{q.Trim()}, Mumbai, India";

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("User-Agent", "ModakSamrat/1.0 (modaksamrat.runasp.net)");
            _http.DefaultRequestHeaders.Add("Accept-Language", "en");

            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1&countrycodes=in";
            var res = await _http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(res);
            var root = doc.RootElement;

            if (root.GetArrayLength() == 0)
                return new JsonResult(new { error = $"Location \"{q}\" not found. Try a 6-digit Mumbai pincode." });

            var first = root[0];
            double lat = double.Parse(first.GetProperty("lat").GetString()!);
            double lng = double.Parse(first.GetProperty("lon").GetString()!);
            string displayName = first.GetProperty("display_name").GetString() ?? q;

            // Shorten display name to first 2 parts
            var parts = displayName.Split(',');
            string area = parts.Length >= 2
                ? $"{parts[0].Trim()}, {parts[1].Trim()}"
                : parts[0].Trim();

            if (isPin) area += $" ({q.Trim()})";

            double dist = GetDistanceKm(SHOP_LAT, SHOP_LNG, lat, lng);

            // Zone logic
            string zone;
            int charge;
            string time;
            bool deliverable = true;

            if (dist <= 5)
            {
                zone = "Zone A";
                charge = 30; // may become 0 if cart >= 500 (handled client side)
                time = "30–40 mins";
            }
            else if (dist <= 8)
            {
                zone = "Zone B";
                charge = 50;
                time = "45–60 mins";
            }
            else
            {
                deliverable = false;
                zone = "Out of range";
                charge = -1;
                time = "";
            }

            return new JsonResult(new
            {
                success = true,
                area,
                dist = Math.Round(dist, 1),
                zone,
                charge,
                time,
                deliverable
            });
        }
        catch (Exception)
        {
            return new JsonResult(new { error = "Lookup failed. Please try My Location button instead." });
        }
    }

    private static double GetDistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371;
        double dLat = ToRad(lat2 - lat1);
        double dLng = ToRad(lng2 - lng1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                 * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * (Math.PI / 180);
}
