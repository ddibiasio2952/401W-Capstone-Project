using FalveyInsuranceGroup.Db;
using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FalveyInsuranceGroup.Backend.Services
{
    /// <summary>
    /// Handles business logic of fetching locations from database
    /// </summary>
    public class MapService
    {
        private const double EARTH_RADIUS = 6371.0; // in km
        private const double DISTANCE = 25.0; // in km
        private const double MIN_LAT = -90.0;
        private const double MAX_LAT = 90.0;
         private const double MIN_LON = -180.0;
        private const double MAX_LON = 180.0;

        private readonly FalveyInsuranceGroupContext _context;

        public MapService(FalveyInsuranceGroupContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Determines what geosearch function will be used based on search type
        /// </summary>
        /// <param name="location">DTO holds geographical information from client</param>
        /// <returns>List of locations found</returns>
        /// <exception cref="ArgumentException">Search type is not supported</exception>
        public async Task<List<Location>> geoSearchAsync(GeosearchDto location)
        {
            List<Location> nearby_locations = new List<Location>();
            var search_type = location.search_type.ToLower().Trim();


            switch (search_type)
            {
                case "proximity":
                    // Bounding box serves as a pre-filter in the where clause
                    createBoundingBox(ref location);
                    nearby_locations = await geoSearchPoint(location);
                    break;

                case "region":
                    nearby_locations = await geoSearchRegion(location);
                    break;

                default:
                    throw new ArgumentException("Invalid search type");
            }

            return nearby_locations;
        }

        /// <summary>
        ///  Geosearches nearby locations based on a specific address
        ///  Proximity-based search: finds distance between two points
        ///  Source: https://medium.com/spartner/the-best-way-to-locate-in-mysql-8-e47a59892443
        /// </summary>
        /// <param name="l">Address that holds geographical info</param>
        /// <returns>List of location nearby given address</returns>
        private async Task<List<Location>> geoSearchPoint(GeosearchDto l)
        {
            // Use Haversine formula to find shortest distance between two points
            // Return locations with a distance under 25 kilometers
            var query = _context.Locations.FromSqlRaw(@"
            SELECT * FROM (
                SELECT *,
                    6371 * acos(
                        cos(radians({0})) * cos(radians(latitude)) *
                        cos(radians(longitude) - radians({1})) +
                        sin(radians({0})) * sin(radians(latitude))
                    ) AS distance_km
                FROM locations
                WHERE latitude BETWEEN {2} AND {3}
                AND longitude BETWEEN {4} AND {5}
            ) AS t
            WHERE distance_km < 25
            ORDER BY distance_km",
            l.latitude, l.longitude,
            l.min_latitude, l.max_latitude,
            l.min_longitude, l.max_longitude)
            .Include(loc => loc.customer);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Geosearches a region with an approximate bounding box
        /// </summary>
        /// <param name="l">Region that holds geographical info</param>
        /// <returns>List of locations in region</returns>
        private async Task<List<Location>> geoSearchRegion(GeosearchDto l)
        {
            // If frontend does not serve a bounding box, create a default one
            if (!l.min_latitude.HasValue && !l.min_longitude.HasValue)
            {
                double delta = 1.0;
                l.min_latitude = l.latitude - delta;
                l.max_latitude = l.latitude + delta;
                l.min_longitude = l.longitude - delta;
                l.max_longitude = l.longitude + delta;
            }

            // Filter locations between min and max lat/lng coordinates
            var query = _context.Locations.FromSqlRaw(@"
                SELECT *
                FROM
                    locations
                WHERE
                    latitude between {0} and {1}
                AND
                    longitude between {2} and {3}"
            , l.min_latitude, l.max_latitude, l.min_longitude, l.max_longitude)
            .Include(loc => loc.customer);

            return await query.ToListAsync();
        }

        /// <summary>
        ///  Creates a bounding box to serve as a pre-filer for proximity search
        ///  Source: http://janmatuschek.de/LatitudeLongitudeBoundingCoordinates
        ///  Source: https://github.com/anthonyvscode/LonelySharp/blob/master/LonelySharp/GeoLocation.cs
        /// </summary>
        /// <param name="l"> Holds necessary geographical information</param>
        private void createBoundingBox(ref GeosearchDto l)
        {
            // Angular radius of query circle
            double angle_rad = DISTANCE / EARTH_RADIUS;

            double min_lat = l.latitude - radianToDegree(angle_rad);
            double max_lat = l.latitude + radianToDegree(angle_rad);

            l.min_latitude = Math.Clamp(min_lat, MIN_LAT, MAX_LAT);
            l.max_latitude = Math.Clamp(max_lat, MIN_LAT, MAX_LAT);

            // If Coordinates are close to the poles
            if (min_lat <= MIN_LAT || max_lat >= MAX_LAT)
            {
                // Longitude becomes ambiguous as all lines converge at poles
                l.min_longitude = MIN_LON;
                l.max_longitude = MAX_LON;

            }
            else
            {
                double lat_rad = degreeToRadian(l.latitude);
                double delta_lon = radianToDegree(
                     Math.Asin(Math.Sin(angle_rad) / Math.Cos(lat_rad))
                );

                l.min_longitude = l.longitude - delta_lon;
                l.max_longitude = l.longitude + delta_lon;

                // Wraps longitude correctly by normalizing in range [-180, 180]
                if (l.min_longitude < MIN_LON)
                    l.min_longitude = ((l.min_longitude + 180) % 360 + 360) % 360 - 180;

                if (l.max_longitude > MAX_LON)
                    l.max_longitude = ((l.max_longitude + 180) % 360 + 360) % 360 - 180;
            }
        }

        /// <summary>
        /// Converts from degree to radians
        /// </summary>
        /// <param name="degree"></param>
        /// <returns>The radian</returns>
        private double degreeToRadian(double degree)
        {
            return degree * (Math.PI / 180);
        }

        /// <summary>
        /// Converts from radians to degrees
        /// </summary>
        /// <param name="radian"></param>
        /// <returns>The degree</returns>
        private double radianToDegree(double radian)
        {
            return radian * (180/Math.PI);
        }

    }
}
